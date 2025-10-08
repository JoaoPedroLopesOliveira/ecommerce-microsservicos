using Microsoft.AspNetCore.Mvc;
using Vendas.API.DTOs;
using Vendas.API.Enuns;
using Vendas.API.Servicos;

namespace Vendas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly PedidoServico _servico;

        public PedidosController(PedidoServico servico)
        {
            _servico = servico;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromBody] List<ItemPedidoDTO> itens)
        {
            try
            {
                var pedidoCriado = await _servico.CriarPedidoAsync(itens);
                return CreatedAtAction(nameof(ObterPedidoPorId), new { pedidoId = pedidoCriado.Id }, pedidoCriado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{pedidoId:int}")]
        public async Task<IActionResult> DeletarPedido(int pedidoId)
        {
            try
            {
                var pedidoDeletado = await _servico.DeletarPedidoAsync(pedidoId);
                return Ok(pedidoDeletado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{pedidoId:int}/status")]
        public async Task<IActionResult> AtualizarStatusPedido(int pedidoId, [FromBody] string novoStatus)
        {
            try
            {
                if (!Enum.TryParse<StatusPedido>(novoStatus, true, out var status))
                    return BadRequest(new { mensagem = "Status inválido. Valores permitidos: PENDENTE, APROVADO, CANCELADO." });

                await _servico.AtualizarStatusPedidoAsync(pedidoId, status);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("pedidos-pagina/{pagina:int}")]
        public async Task<IActionResult> ObterPedidos(int pagina = 1)
        {
            try
            {
                var pedidos = await _servico.ConsultarPedidosAsync(pagina);
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("{pedidoId:int}")]
        public async Task<IActionResult> ObterPedidoPorId(int pedidoId)
        {
            try
            {
                var pedido = await _servico.ConsultarPedidoAsync(pedidoId);
                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }
    }
}
