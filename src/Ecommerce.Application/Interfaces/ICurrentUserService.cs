using System;

namespace Ecommerce.Application.Interfaces
{
    public interface ICurrentUserService
    {
        // Retorna o ID do usuário logado, ou null se ninguém estiver logado
        Guid? GetUserId();
    }
}