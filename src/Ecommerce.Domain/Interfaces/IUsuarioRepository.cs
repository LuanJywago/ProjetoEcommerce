using Ecommerce.Domain.Entities;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        // Contrato simplificado e em Português para bater com o Service
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task CriarAsync(Usuario usuario);
        void GetByEmailAsync(string email);
        void AddAsync(Usuario usuario);
        void SaveChangesAsync();
    }
}