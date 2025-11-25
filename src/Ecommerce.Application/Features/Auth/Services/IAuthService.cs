using Ecommerce.Application.Features.Auth.DTOs;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<bool> RegistrarAsync(RegistrarUsuarioDto dto);

        Task<LoginResponseDto?> LoginAsync(LoginUsuarioDto dto);

        // Removemos o método ValidarA2FAsync
    }
}