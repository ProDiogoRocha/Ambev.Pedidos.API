using Ambev.Pedidos.API.DTOs;
using System.Text.Json;

namespace Ambev.Pedidos.API.Handler
{
    public class PedidoMessageHandler
    {
        private readonly ILogger<PedidoMessageHandler> _logger;

        public PedidoMessageHandler(ILogger<PedidoMessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                // Exemplo: desserializa o JSON da mensagem
                var pedido = JsonSerializer.Deserialize<PedidoDto>(message);

                if (pedido == null)
                {
                    _logger.LogWarning("Mensagem recebida é inválida ou vazia.");
                    return;
                }

                // Lógica de negócio simulada
                if (pedido.Quantidade < 1000)
                {
                    _logger.LogWarning("Pedido recusado: quantidade inferior ao mínimo permitido. Pedido: {@Pedido}", pedido);
                    return;
                }

                // Simula envio do pedido para API da Ambev ou serviço de persistência
                _logger.LogInformation("Pedido processado com sucesso: {@Pedido}", pedido);

                // Aqui você pode chamar um serviço que salva no banco ou envia para a Ambev
                await Task.CompletedTask;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao desserializar a mensagem: {Mensagem}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar a mensagem: {Mensagem}", message);
            }
        }
    }
}
