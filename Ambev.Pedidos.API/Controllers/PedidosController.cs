using Ambev.Pedidos.API.DTOs;
using Ambev.Pedidos.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.Pedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(ILogger<PedidosController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] PedidoAmbevRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Verificação de model validation
            }

            if (string.IsNullOrEmpty(request.RevendaId) || request.Produtos.Count == 0)
            {
                return BadRequest("Pedido inválido.");
            }

            try
            {
                var numeroPedido = Guid.NewGuid().ToString()[..8].ToUpper(); // Mock do número do pedido

                _logger.LogInformation("Pedido recebido da revenda {RevendaId}. Número: {Numero}", request.RevendaId, numeroPedido);

                var response = new PedidoAmbevResponse
                {
                    NumeroPedidoAmbev = numeroPedido,
                    ProdutosConfirmados = request.Produtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedido.");
                return StatusCode(500, "Erro interno ao processar pedido.");
            }
        }
    }
}
