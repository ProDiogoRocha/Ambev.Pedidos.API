using Ambev.Pedidos.API.DbContexts;
using Ambev.Pedidos.API.Models;
using System.Text.Json;

namespace Ambev.Pedidos.API.Services
{
    public class RetryPedidosService : BackgroundService
    {
        private readonly ILogger<RetryPedidosService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;

        public RetryPedidosService(
            ILogger<RetryPedidosService> logger,
            IHttpClientFactory httpClientFactory,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de retry de pedidos iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
                    var pedidosComErro = db.PedidosComErro.ToList();

                    foreach (var pedidoErro in pedidosComErro)
                    {
                        var pedido = JsonSerializer.Deserialize<PedidoAmbevRequest>(pedidoErro.JsonPedido);

                        var sender = new PedidoSenderService(_httpClientFactory);
                        var enviado = await sender.EnviarPedidoAsync(pedido!);

                        if (enviado)
                        {
                            db.PedidosComErro.Remove(pedidoErro);
                            await db.SaveChangesAsync();
                            _logger.LogInformation("Pedido reprocessado com sucesso: {Id}", pedidoErro.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Falha ao reprocessar pedido: {Id}", pedidoErro.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no serviço de retry.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); 
            }
        }
    }
}
