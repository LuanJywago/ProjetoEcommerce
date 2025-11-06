using Ecommerce.Domain.Entities;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface ILogAuditoriaRepository
    {
        Task AddAsync(LogAuditoria log);
        // Não precisamos de SaveChangesAsync aqui, pois o PecaService já o chama.
    }
}