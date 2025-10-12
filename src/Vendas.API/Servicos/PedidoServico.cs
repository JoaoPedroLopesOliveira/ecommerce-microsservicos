using Messaging.Contracts.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Vendas.API.DTOs;
using Vendas.API.Entidades;
using Vendas.API.Enuns;
using Vendas.API.Infraestrutura.Db;
using Messaging.Contracts;
using Messaging.Contracts.Messages;

namespace Vendas.API.Servicos
{
    public class PedidoServico
    {
        private readonly DbContexto _contexto;
        private readonly RabbitMqPublisher _publisher;
        private readonly IConfiguration _configuration;

        public PedidoServico(DbContexto contexto, RabbitMqPublisher publisher, IConfiguration configuration)
        {
            _contexto = contexto;
            _publisher = publisher;
            _configuration = configuration;
        }

        public async Task<PedidoDTO> ConsultarPedidoAsync(int pedidoId)
        {
            var pedidoEncontrado = await _contexto.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedidoEncontrado == null)
                throw new Exception($"O pedido com Id {pedidoId} não foi encontrado.");

            return MapearParaDTO(pedidoEncontrado);
        }

        public async Task<List<PedidoDTO>> ConsultarPedidosAsync(int pagina)
        {
            int tamanhoPagina = 10;

            var pedidos = await _contexto.Pedidos
                .Include(p => p.Itens)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return pedidos.Select(MapearParaDTO).ToList();
        }

        public async Task<PedidoDTO> CriarPedidoAsync(List<ItemPedidoDTO> itens)
        {
            var novoPedido = new Pedido();

            foreach (var item in itens)
            {
                if (item.Quantidade <= 0)
                    throw new Exception($"Quantidade do item {item.ProdutoId} deve ser maior que zero.");

                novoPedido.Itens.Add(new ItemPedido
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade
                });
            }

            novoPedido.DataPedido = DateTime.UtcNow;
            novoPedido.Status = StatusPedido.PENDENTE;

            _contexto.Pedidos.Add(novoPedido);
            await _contexto.SaveChangesAsync();
            var mensagem = new PedidoMensagem
            {
                PedidoId = novoPedido.Id,
                Itens = novoPedido.Itens.Select(i => new ItemPedidoMensagem
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade
                }).ToList(),
                CorrelationId = Guid.NewGuid().ToString()
            };
            await _publisher.PublicarMensagemAsync(
            exchange: _configuration["RabbitMq:Exchange"] ?? "ecommerce.exchange",
            routingKey: "pedido.criado",
            message: mensagem
        );
            return MapearParaDTO(novoPedido);
        }

        public async Task AtualizarStatusPedidoAsync(int pedidoId, StatusPedido novoStatus)
        {
            var pedido = await _contexto.Pedidos.FindAsync(pedidoId);

            if (pedido == null)
                throw new Exception($"O pedido com Id {pedidoId} não foi encontrado.");

            pedido.Status = novoStatus;
            _contexto.Pedidos.Update(pedido);
            await _contexto.SaveChangesAsync();
        }

        public async Task<PedidoDTO> DeletarPedidoAsync(int pedidoId)
        {
            var pedido = await _contexto.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new Exception($"O pedido com Id {pedidoId} não foi encontrado.");

            _contexto.Pedidos.Remove(pedido);
            await _contexto.SaveChangesAsync();

            return MapearParaDTO(pedido);
        }

        private PedidoDTO MapearParaDTO(Pedido pedido)
        {
            return new PedidoDTO
            {
                Id = pedido.Id,
                DataPedido = pedido.DataPedido,
                Status = pedido.Status.ToString(),
                Itens = pedido.Itens.Select(i => new ItemPedidoDTO
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade
                }).ToList()
            };
        }
    }
}
