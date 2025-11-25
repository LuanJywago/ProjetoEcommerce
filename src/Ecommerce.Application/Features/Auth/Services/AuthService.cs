using Ecommerce.Application.Features.Auth.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces; // Agora vai puxar o Repositório certo do Domain
using System;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUsuarioRepository usuarioRepository, ITokenService tokenService, Auditoria.Services.IAuditoriaService @object)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
        }

        public async Task<bool> RegistrarAsync(RegistrarUsuarioDto dto)
        {
            // Verifica se já existe
            if (await _usuarioRepository.ObterPorEmailAsync(dto.Email) != null)
                return false;

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Email = dto.Email,
                // Atribuição direta do Enum (Funciona porque corrigimos o DTO)
                Tipo = dto.Tipo, 
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
            };

            await _usuarioRepository.CriarAsync(usuario);
            return true;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginUsuarioDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);
            
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
                return null;

            var token = _tokenService.GerarToken(usuario);

            return new LoginResponseDto
            {
                Token = token,
                Nome = usuario.Nome,
                Email = usuario.Email,
                // Converte o Enum para String para o Frontend ler ("Admin", "Cliente")
                Tipo = usuario.Tipo.ToString() 
            };
        }
    }
}