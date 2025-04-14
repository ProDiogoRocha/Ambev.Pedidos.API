using System.Text.Json;
using System.Text;
using Ambev.Pedidos.API.Models;

namespace Ambev.Pedidos.API.Services
{
    public class PedidoSenderService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PedidoSenderService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> EnviarPedidoAsync(PedidoAmbevRequest pedido)
        {
            var total = pedido.Produtos.Sum(p => p.Quantidade);
            if (total < 1000)
                return false;

            var client = _httpClientFactory.CreateClient("AmbevApi");

            var content = new StringContent(JsonSerializer.Serialize(pedido), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("/api/pedidos", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false; 
            }
        }
    }
}
