using Ambev.Pedidos.API.Models;

namespace Ambev.Pedidos.API.DTOs
{
    public class PedidoAmbevResponse
    {
        public string NumeroPedidoAmbev { get; set; } = string.Empty;

        public List<ItemPedido> ProdutosConfirmados { get; set; } = new();
    }
}
