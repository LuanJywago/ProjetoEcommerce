using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence; // Para AppDbContext
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Persistence.Repositories
{
    public class LogAuditoriaRepository : ILogAuditoriaRepository
    {
        private readonly AppDbContext _context;

        public LogAuditoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LogAuditoria log)
        {
            // Apenas adiciona ao contexto. O SaveChangesAsync será chamado depois.
            await _context.LogsAuditoria.AddAsync(log);
        }
    }
}