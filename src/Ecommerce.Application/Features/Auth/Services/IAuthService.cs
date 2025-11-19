using Ecommerce.Application.Features.Auth.DTOs;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<bool> RegistrarAsync(RegistrarUsuarioDto dto);

        // ========================================================================
        // === MUDANÇA (RESET PASSO 41.2) ===
        // Mudamos a assinatura de volta.
        // O LoginAsync agora retorna o LoginResponseDto (o token final)
        // em vez do LoginPasso1ResponseDto.
        // ========================================================================
        Task<LoginResponseDto?> LoginAsync(LoginUsuarioDto dto);

        // Removemos o método ValidarA2FAsync
    }
}