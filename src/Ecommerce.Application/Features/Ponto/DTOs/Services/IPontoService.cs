using Ecommerce.Application.Features.Ponto.DTOs; // Nosso DTO
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Ponto.Services
{
    public interface IPontoService
    {
        // Retorna o DTO do ponto registrado ou lança exceção se não puder
        Task<RegistroPontoDto> RegistrarEntradaAsync();
        Task<RegistroPontoDto> RegistrarSaidaAsync();
    }
}