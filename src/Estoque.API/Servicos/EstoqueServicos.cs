using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estoque.API.DTOs;
using Estoque.API.Infraestrutura.Db;
using Gateway.API.Entidades;
using Messaging.Contracts.Infraestrutura;
using Messaging.Contracts.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Estoque.API.Servicos
{
    public class EstoqueServicos
    {
        private readonly DbContexto _contexto;
        private readonly RabbitMqPublisher _publisher;
        private readonly IConfiguration _configuration;
        public EstoqueServicos(DbContexto contexto, RabbitMqPublisher publisher, IConfiguration configuration)
        {
            _contexto = contexto;
            _publisher = publisher;
            _configuration = configuration;
        }
        public async Task<Produto> CadastrarProduto(Produto produto)
        {
            var existente = await _contexto.Produtos.AnyAsync(p => p.Nome == produto.Nome);
            if (existente)
            {
                throw new Exception("Produto já cadastrado.");
            }
            _contexto.Produtos.Add(produto);
            await _contexto.SaveChangesAsync();
            return produto;
        }

        public async Task AutorizaVenda(List<int> produtoIds, List<int> quantidades, int PedidoId, string CorrelationId)
        {
            if (produtoIds.Count != quantidades.Count)
                throw new Exception("A lista de produtos e quantidades devem ter o mesmo tamanho.");

            bool aprovado = true;
            string? motivo = null;

            for (int i = 0; i < produtoIds.Count; i++)
            {
                var produtoId = produtoIds[i];
                var quantidade = quantidades[i];

                var produto = await _contexto.Produtos.FindAsync(produtoId);
                if (produto == null)
                {
                    aprovado = false;
                    motivo = $"Produto com id {produtoId} não encontrado";
                    break;
                }
                if (produto.QuantidadeEstoque < quantidade)
                {
                    aprovado = false;
                    motivo = $"Estoque insuficiente para o produto {produto.Nome}. Disponível: {produto.QuantidadeEstoque}, Requerido: {quantidade}";
                    break;
                }

                produto.QuantidadeEstoque -= quantidade;
            }

            if (aprovado)
                await _contexto.SaveChangesAsync();

            var resposta = new RespostaEstoqueMensagem
            {
                PedidoId = PedidoId,
                Aprovado = aprovado,
                CorrelationId = CorrelationId,
                Motivo = motivo
            };

            await _publisher.PublicarMensagemAsync(
                exchange: _configuration["RabbitMq:Exchange"] ?? "ecommerce.exchange",
                routingKey: "pedido.resposta",
                message: resposta
            );

            Console.WriteLine($"[✔] Resposta enviada para Vendas: PedidoId {PedidoId} - Aprovado: {aprovado}");
        }
        public async Task<ProdutoDTO> ConsultarEstoqueProduto(int produtoId)
        {
            var produto = await _contexto.Produtos.FindAsync(produtoId);
            if (produto == null)
            {
                throw new Exception("Produto não encontrado.");
            }
            var produtoDTO = new ProdutoDTO
            {
                Nome = produto.Nome,
                Preco = produto.Preco,
                QuantidadeEstoque = produto.QuantidadeEstoque
            };
            return produtoDTO;
        }

        public async Task<List<Produto>> ConsultarEstoqueGeral(int pagina = 1)
        {
            int itensPorPagina = 10;
            var produtosQuery = _contexto.Produtos.AsQueryable();
            produtosQuery = produtosQuery.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina);
            return await produtosQuery.ToListAsync();
        }

        public async Task RemoverProduto(int produtoId)
        {
            var encontrado = await _contexto.Produtos.FindAsync(produtoId);
            if (encontrado == null)
            {
                throw new Exception("Produto não encontrado.");
            }
            _contexto.Produtos.Remove(encontrado);
            await _contexto.SaveChangesAsync();
            Console.WriteLine("Produto " + encontrado.Nome + " removido com sucesso.");
            return;
        }

        public async Task<Produto> AtualizarProduto(ProdutoDTO produtoDTO, int produtoId)
        {
            var existente = await _contexto.Produtos.FindAsync(produtoId);
            if (existente == null)
            {
                throw new Exception("Produto não encontrado.");
            }
            existente.Nome = produtoDTO.Nome;
            existente.Preco = produtoDTO.Preco;
            existente.QuantidadeEstoque = produtoDTO.QuantidadeEstoque;
            await _contexto.SaveChangesAsync();
            return existente;
        }

        public async Task<Produto> AtualizarEstoque(int produtoId, int quantidade)
        {
            var existente = await _contexto.Produtos.FindAsync(produtoId);
            if (existente == null)
            {
                throw new Exception("Produto não encontrado.");
            }
            existente.QuantidadeEstoque = quantidade;
            await _contexto.SaveChangesAsync();
            return existente;
        }
    }
}