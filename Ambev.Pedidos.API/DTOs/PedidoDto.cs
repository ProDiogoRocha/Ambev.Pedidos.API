namespace Ambev.Pedidos.API.DTOs
{
    public class PedidoDto
    {
        public string Id { get; set; }
        public string Produto { get; set; }
        public int Quantidade { get; set; }
        public string Cliente { get; set; }
    }
}
