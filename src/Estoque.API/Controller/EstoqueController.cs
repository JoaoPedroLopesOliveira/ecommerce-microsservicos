using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estoque.API.DTOs;
using Estoque.API.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoqueController : ControllerBase
    {
        private readonly EstoqueServicos _servico;

        public EstoqueController(EstoqueServicos servico)
        {
            _servico = servico;
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarProduto([FromBody] Gateway.API.Entidades.Produto produto)
        {
            try
            {
                var produtoCadastrado = await _servico.CadastrarProduto(produto);
                return CreatedAtAction(nameof(CadastrarProduto), new { id = produtoCadastrado.Id }, produtoCadastrado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
        [HttpPut("{produtoId:int}")]
        public async Task<IActionResult> AtualizarEstoque(int produtoId, [FromBody] int quantidade)
        {
            try
            {
                var produtoAtualizado = await _servico.AtualizarEstoque(produtoId, quantidade);
                return Ok(produtoAtualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{produtoId:int}")]
        public async Task<IActionResult> RemoverProduto(int produtoId)
        {
            try
            {
                await _servico.RemoverProduto(produtoId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("{produtoId:int}")]
        public async Task<IActionResult> ConsultarEstoqueProduto(int produtoId)
        {
            try
            {
                var produto = await _servico.ConsultarEstoqueProduto(produtoId);
                if (produto == null)
                {
                    return NotFound(new { mensagem = "Produto não encontrado." });
                }
                return Ok(produto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("estoque-geral")]
        public async Task<IActionResult> ConsultarEstoqueGeral([FromQuery] int pagina = 1)
        {
            try
            {
                var produtos = await _servico.ConsultarEstoqueGeral(pagina);
                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost("autorizar-venda")]
        public async Task<IActionResult> AutorizarVenda([FromBody] AutorizarVendaDTO dto)
        {
            try
            {
                await _servico.AutorizaVenda(dto.ProdutoIds, dto.Quantidades);
                return Ok(new { mensagem = "Venda autorizada." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}