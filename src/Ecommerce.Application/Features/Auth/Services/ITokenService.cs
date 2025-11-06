using Ecommerce.Domain.Entities; // Garanta que este using está presente

namespace Ecommerce.Application.Features.Auth.Services
{
    public interface ITokenService
    {
        string GerarToken(Usuario usuario);
    }
}