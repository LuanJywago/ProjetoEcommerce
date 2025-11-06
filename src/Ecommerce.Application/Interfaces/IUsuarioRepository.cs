using Ecommerce.Domain.Entities;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task AddAsync(Usuario usuario);

        // --- NOVO MÉTODO ---
        // Adiciona a capacidade de atualizar um usuário
        void Update(Usuario usuario);
        // --- FIM DO NOVO MÉTODO ---

        Task<int> SaveChangesAsync();
    }
}
