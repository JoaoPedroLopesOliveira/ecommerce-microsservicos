using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Estoque.API.Infraestrutura.Db;
using Messaging.Contracts;
using Messaging.Contracts.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Estoque.API.Infraestrutura.Consumer
{
    public class EstoqueConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly string _exchange;
        private readonly string _queue = "verificar.estoque.queue";
        private readonly string _routingKey = "pedido.criado";

        public EstoqueConsumerService(IServiceProvider serviceProvider, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _exchange = config["RabbitMq:Exchange"] ?? "ecommerce.exchange";
            InitRabbit();
        }

        private void InitRabbit()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMq:HostName"] ?? "rabbitmq",
                UserName = _config["RabbitMq:UserName"] ?? "guest",
                Password = _config["RabbitMq:Password"] ?? "guest",
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: _queue, exchange: _exchange, routingKey: _routingKey);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                PedidoMensagem? pedido = null;

                try
                {
                    pedido = JsonSerializer.Deserialize<PedidoMensagem>(json);
                    using var scope = _serviceProvider.CreateScope();
                    var contexto = scope.ServiceProvider.GetRequiredService<DbContexto>();

                    bool aprovado = true;
                    string? motivo = null;

                    // Verifica estoque
                    foreach (var item in pedido.Itens)
                    {
                        var produto = await contexto.Produtos.FindAsync(item.ProdutoId);
                        if (produto == null)
                        {
                            aprovado = false;
                            motivo = $"Produto {item.ProdutoId} não encontrado.";
                            break;
                        }
                        if (produto.QuantidadeEstoque < item.Quantidade)
                        {
                            aprovado = false;
                            motivo = $"Estoque insuficiente para o produto {item.ProdutoId}.";
                            break;
                        }
                    }

                    // Atualiza se aprovado
                    if (aprovado)
                    {
                        foreach (var item in pedido.Itens)
                        {
                            var produto = await contexto.Produtos.FindAsync(item.ProdutoId);
                            produto.QuantidadeEstoque -= item.Quantidade;
                        }
                        await contexto.SaveChangesAsync();
                    }

                    // Publica resposta
                    var resposta = new RespostaEstoqueMessagem.RespostaEstoque(
                        PedidoId: pedido.PedidoId,
                        Aprovado: aprovado,
                        CorrelationId: pedido.CorrelationId,
                        Motivo: motivo
                    );

                    var respostaJson = JsonSerializer.Serialize(resposta);
                    var respostaBytes = Encoding.UTF8.GetBytes(respostaJson);

                    var props = _channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.CorrelationId = pedido.CorrelationId;

                    _channel.BasicPublish(_exchange, "pedido.resposta", props, respostaBytes);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    if (pedido != null)
                    {
                        var resposta = new RespostaEstoqueMessagem.RespostaEstoque(
                            PedidoId: pedido.PedidoId,
                            Aprovado: false,
                            CorrelationId: pedido.CorrelationId,
                            Motivo: ex.Message
                        );
                        var respostaJson = JsonSerializer.Serialize(resposta);
                        var respostaBytes = Encoding.UTF8.GetBytes(respostaJson);
                        var props = _channel.CreateBasicProperties();
                        props.Persistent = true;
                        props.CorrelationId = pedido?.CorrelationId;

                        _channel.BasicPublish(_exchange, "pedido.resposta", props, respostaBytes);
                    }
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: _queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch
            {
                // Ignorar erros ao fechar
            }
            base.Dispose();
        }
    }
}