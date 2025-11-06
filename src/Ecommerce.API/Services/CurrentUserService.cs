using Ecommerce.Application.Interfaces; // Nossa interface
using Microsoft.AspNetCore.Http; // Para IHttpContextAccessor
using System;
using System.Security.Claims; // Para ClaimTypes

namespace Ecommerce.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetUserId()
        {
            // Tenta pegar o Claim "NameIdentifier" (que configuramos no TokenService para ser o ID)
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tenta converter para Guid
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }

            // Se não encontrar ou não conseguir converter, retorna null
            return null;
        }
    }
}