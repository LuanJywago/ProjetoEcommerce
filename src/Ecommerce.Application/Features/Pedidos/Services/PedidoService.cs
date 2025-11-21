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
                // 1. Busca a peça no banco
                var peca = await _pecaRepository.ObterPorIdAsync(itemDto.PecaId);
                
                if (peca == null)
                    throw new Exception($"Peça com ID {itemDto.PecaId} não encontrada.");

                // 2. Valida Estoque
                if (peca.Estoque < itemDto.Quantidade)
                    throw new Exception($"Estoque insuficiente para a peça '{peca.Nome}'. Disponível: {peca.Estoque}, Solicitado: {itemDto.Quantidade}");

                // 3. Cria o Item do Pedido
                var itemPedido = new PedidoItem
                {
                    Id = Guid.NewGuid(),
                    PecaId = peca.Id,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = peca.Preco // Pega o preço atual da peça
                };

                // 4. Adiciona ao pedido e soma o total
                pedido.Itens.Add(itemPedido);
                pedido.ValorTotal += itemPedido.Quantidade * itemPedido.PrecoUnitario;

                // 5. BAIXA NO ESTOQUE
                peca.Estoque -= itemDto.Quantidade;
                
                // Atualiza o estoque da peça no banco
                await _pecaRepository.AtualizarAsync(peca); 
            }

            // 6. Salva o pedido completo
            return await _pedidoRepository.CriarAsync(pedido);
        }

        // --- NOVO MÉTODO PARA O HISTÓRICO ---
        public async Task<IEnumerable<Pedido>> ObterPedidosPorUsuarioAsync(Guid usuarioId)
        {
            return await _pedidoRepository.ListarPorUsuarioAsync(usuarioId);
        }
    }
}