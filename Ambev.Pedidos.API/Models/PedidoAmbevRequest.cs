namespace Ambev.Pedidos.API.Models
{
    public class PedidoAmbevRequest
    {
        public string RevendaId { get; set; } = string.Empty;

        public List<ItemPedido> Produtos { get; set; } = new();
    }
}
