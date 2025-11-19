using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Persistence.Repositories
{
    public class PecaRepository : IPecaRepository
    {
        private readonly AppDbContext _context;

        public PecaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Peca peca)
        {
            await _context.Pecas.AddAsync(peca);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Peca>> ObterTodasAsync()
        {
            return await _context.Pecas.ToListAsync();
        }

        public async Task<Peca?> ObterPorIdAsync(Guid id)
        {
            return await _context.Pecas.FindAsync(id);
        }

        public async Task AtualizarAsync(Peca peca)
        {
            _context.Pecas.Update(peca);
            await _context.SaveChangesAsync();
        }

        public async Task DeletarAsync(Peca peca)
        {
            _context.Pecas.Remove(peca);
            await _context.SaveChangesAsync();
        }
    }
}