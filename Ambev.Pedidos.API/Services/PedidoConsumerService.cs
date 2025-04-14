using Ambev.Pedidos.API.Configurations;
using Ambev.Pedidos.API.DTOs;
using Ambev.Pedidos.API.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace Ambev.Pedidos.API.Services
{
    public class PedidoConsumerService : BackgroundService
    {
        private readonly ILogger<PedidoConsumerService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private IChannel _channel = null!;
        private IConnection _connection = null!;
        private readonly RabbitMqSettings _settings;
        private const string QueueName = "ambev-pedidos";

        public PedidoConsumerService(ILogger<PedidoConsumerService> logger, IHttpClientFactory httpClientFactory, IOptions<RabbitMqSettings> options)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
            InitializeRabbitMQ();
        }

        private async void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Host,
                UserName = _settings.Username,
                Password = _settings.Password
            };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var pedido = JsonSerializer.Deserialize<PedidoAmbevRequest>(json);

                    if (pedido is null)
                    {
                        _logger.LogWarning("Mensagem inválida recebida da fila.");
                        return;
                    }

                    var total = pedido.Produtos.Sum(p => p.Quantidade);
                    if (total < 1000)
                    {
                        _logger.LogWarning("Pedido descartado: quantidade total inferior a 1000 unidades.");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    var success = await EnviarPedidoParaAmbev(pedido);

                    if (success)
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    else
                    {
                        _logger.LogWarning("Falha ao enviar pedido. Pedido será reenfileirado.");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true); // requeue para retry
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila.");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
                }
            };

            _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        private async Task<bool> EnviarPedidoParaAmbev(PedidoAmbevRequest pedido)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AmbevApi");
                var content = new StringContent(JsonSerializer.Serialize(pedido), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/pedidos", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Erro HTTP ao chamar AMBEV: {StatusCode}", response.StatusCode);
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var resposta = JsonSerializer.Deserialize<PedidoAmbevResponse>(responseContent);

                _logger.LogInformation("Pedido enviado com sucesso. Nº AMBEV: {Numero}", resposta?.NumeroPedidoAmbev);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar enviar pedido para AMBEV.");
                return false;
            }
        }

        public override void Dispose()
        {
            _channel.CloseAsync();
            _connection.CloseAsync();
            base.Dispose();
        }
    }
}
