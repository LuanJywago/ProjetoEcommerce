using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Persistence;
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

        public Task AddAsync(LogAuditoria log)
        {
            throw new NotImplementedException();
        }

        public async Task CriarAsync(LogAuditoria log)
        {
            // Adiciona o log no banco e salva
            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}