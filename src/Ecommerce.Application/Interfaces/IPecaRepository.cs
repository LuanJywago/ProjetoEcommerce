using Ecommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    // Usamos 'Task' para programação assíncrona
    public interface IPecaRepository
    {
        Task<Peca?> GetByIdAsync(Guid id);
        Task<IEnumerable<Peca>> GetAllAsync();
        Task AddAsync(Peca peca);
        void Update(Peca peca); // Update e Remove não são async no EF Core
        void Remove(Peca peca);
        Task<int> SaveChangesAsync(); // Para persistir as mudanças
    }
}