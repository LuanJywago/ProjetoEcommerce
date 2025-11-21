using Ecommerce.Application.Features.Pedidos.DTOs;
using Ecommerce.Application.Interfaces; 
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Pedidos.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IPecaRepository _pecaRepository; 

        public PedidoService(IPedidoRepository pedidoRepository, IPecaRepository pecaRepository)
        {
            _pedidoRepository = pedidoRepository;
            _pecaRepository = pecaRepository;
        }

        public async Task<Pedido> RegistrarVendaAsync(RealizarPedidoDto pedidoDto, Guid usuarioId)
        {
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                DataPedido = DateTime.UtcNow,
                Itens = new List<PedidoItem>(),
                ValorTotal = 0
            };

            foreach (var itemDto in pedidoDto.Itens)
            {
                // 1. Busca a peça e valida
                var peca = await _pecaRepository.ObterPorIdAsync(itemDto.PecaId);
                if (peca == null) throw new Exception($"Peça {itemDto.PecaId} não encontrada.");
                
                // 2. Valida Estoque
                if (peca.Estoque < itemDto.Quantidade) 
                    throw new Exception($"Estoque insuficiente para a peça '{peca.Nome}'.");

                // 3. Cria o Item do Pedido
                var itemPedido = new PedidoItem
                {
                    Id = Guid.NewGuid(),
                    PecaId = peca.Id,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = peca.Preco
                };

                // 4. Adiciona ao pedido e soma o total
                pedido.Itens.Add(itemPedido);
                pedido.ValorTotal += itemPedido.Quantidade * itemPedido.PrecoUnitario;

                // 5. BAIXA NO ESTOQUE
                peca.Estoque -= itemDto.Quantidade;
                await _pecaRepository.AtualizarAsync(peca); 
            }

            return await _pedidoRepository.CriarAsync(pedido);
        }

        public async Task<IEnumerable<Pedido>> ObterPedidosPorUsuarioAsync(Guid usuarioId)
        {
            return await _pedidoRepository.ListarPorUsuarioAsync(usuarioId);
        }

        // --- LÓGICA DO RELATÓRIO ---
        public async Task<RelatorioVendasDto> GerarRelatorioAdminAsync()
        {
            var total = await _pedidoRepository.ObterFaturamentoTotalAsync();
            var qtd = await _pedidoRepository.ObterTotalPedidosAsync();

            return new RelatorioVendasDto
            {
                FaturamentoTotal = total,
                TotalPedidos = qtd,
                // Evita divisão por zero se não tiver pedidos
                TicketMedio = qtd > 0 ? total / qtd : 0
            };
        }
    }
}