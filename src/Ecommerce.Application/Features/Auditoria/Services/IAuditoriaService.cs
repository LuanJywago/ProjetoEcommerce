using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Auditoria.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarLog(string acao, string detalhes);
    }
}