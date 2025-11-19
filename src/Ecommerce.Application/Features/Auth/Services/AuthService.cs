using Ecommerce.Application.Features.Auth.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;
using System;
using Ecommerce.Application.Features.Auditoria.Services;

namespace Ecommerce.Application.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly IAuditoriaService _auditoriaService;

        public AuthService(IUsuarioRepository usuarioRepository, ITokenService tokenService, IAuditoriaService auditoriaService)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
            _auditoriaService = auditoriaService;
        }

        public async Task<bool> RegistrarAsync(RegistrarUsuarioDto dto)
        {
            var usuarioExistente = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuarioExistente != null)
            {
                await _auditoriaService.RegistrarLog("REGISTRO_FALHA", $"E-mail já existente: {dto.Email}");
                return false; 
            }
            
            string senhaHash = BCryptNet.HashPassword(dto.Senha); 
            
            var novoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = senhaHash,
                Tipo = dto.Tipo
            };

            await _usuarioRepository.AddAsync(novoUsuario);
            await _usuarioRepository.SaveChangesAsync();

            // Correção do log com 2 argumentos
            await _auditoriaService.RegistrarLog("REGISTRO_SUCESSO", $"Usuário registrado: {novoUsuario.Email} (ID: {novoUsuario.Id})");
            
            return true;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginUsuarioDto dto)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuario == null)
            {
                await _auditoriaService.RegistrarLog("LOGIN_FALHA", $"Usuário não encontrado: {dto.Email}");
                return null;
            }

            if (!BCryptNet.Verify(dto.Senha, usuario.SenhaHash))
            {
                await _auditoriaService.RegistrarLog("LOGIN_FALHA", $"Senha incorreta para: {dto.Email}");
                return null;
            }

            var token = _tokenService.GerarToken(usuario);

            await _auditoriaService.RegistrarLog("LOGIN_SUCESSO", $"Login: {usuario.Email}");

            return new LoginResponseDto
            {
                Token = token,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }
    }
}