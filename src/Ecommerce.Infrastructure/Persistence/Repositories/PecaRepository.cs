// O namespace DEVE ser este:
namespace Ecommerce.Infrastructure.Persistence.Repositories
{
    using Ecommerce.Application.Interfaces;
    using Ecommerce.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class PecaRepository : IPecaRepository
    {
        private readonly AppDbContext _context;

        public PecaRepository(AppDbContext context)
        {
            _context = context; 
        }

        public async Task AddAsync(Peca peca)
        {
            await _context.Pecas.AddAsync(peca);
        }

        public async Task<IEnumerable<Peca>> GetAllAsync()
        {
            return await _context.Pecas.AsNoTracking().ToListAsync();
        }

        public async Task<Peca?> GetByIdAsync(Guid id)
        {
            return await _context.Pecas.FindAsync(id);
        }

        public void Remove(Peca peca)
        {
            _context.Pecas.Remove(peca);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Update(Peca peca)
        {
            _context.Pecas.Update(peca);
        }
    }
}