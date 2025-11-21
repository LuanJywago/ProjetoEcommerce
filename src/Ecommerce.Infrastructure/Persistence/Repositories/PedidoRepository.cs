using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Persistence.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _context;

        public PedidoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CriarAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task<Pedido?> ObterPorIdAsync(Guid id)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Peca)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pedido>> ListarPorUsuarioAsync(Guid usuarioId)
        {
            return await _context.Pedidos
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Peca)
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();
        }

        // --- LÓGICA DO RELATÓRIO ---
        public async Task<decimal> ObterFaturamentoTotalAsync()
        {
            // Se não tiver pedidos, retorna 0 para não dar erro
            if (!await _context.Pedidos.AnyAsync()) return 0;
            
            return await _context.Pedidos.SumAsync(p => p.ValorTotal);
        }

        public async Task<int> ObterTotalPedidosAsync()
        {
            return await _context.Pedidos.CountAsync();
        }
    }
}