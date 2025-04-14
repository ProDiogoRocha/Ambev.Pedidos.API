namespace Ambev.Pedidos.API.Entities
{
    public class PedidoComErro
    {
        public int Id { get; set; }

        public string RevendaId { get; set; } = string.Empty;

        public string JsonPedido { get; set; } = string.Empty;

        public DateTime DataErro { get; set; } = DateTime.UtcNow;
    }
}
