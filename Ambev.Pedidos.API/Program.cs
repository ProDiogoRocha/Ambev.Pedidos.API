using Ambev.Pedidos.API.Configurations;
using Ambev.Pedidos.API.DbContexts;
using Ambev.Pedidos.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registro do HttpClient para a API da AMBEV
builder.Services.AddHttpClient("AmbevApi", client =>
{
    client.BaseAddress = new Uri("http://mock-ambev"); // nome do container no docker
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PedidosDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ")
);

// Registro dos serviços principais
builder.Services.AddScoped<PedidoSenderService>(); // Serviço para envio de pedidos
builder.Services.AddHostedService<PedidoConsumerService>(); // Consumidor de pedidos da fila RabbitMQ
builder.Services.AddHostedService<RetryPedidosService>(); // Serviço para reprocessar pedidos com falha

builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuração do pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();