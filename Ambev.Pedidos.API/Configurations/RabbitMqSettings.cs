namespace Ambev.Pedidos.API.Configurations
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Queue { get; set; } = default!;
    }
}
