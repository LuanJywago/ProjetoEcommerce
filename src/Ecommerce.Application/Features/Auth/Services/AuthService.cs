using Ecommerce.Application.Features.Auth.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt; // Alias para BCrypt
using System; // Para Guid, DateTime e Random
using Ecommerce.Application.Exceptions; // Para ValidacaoException
using Ecommerce.Application.Features.Auditoria.Services; // Importa o IAuditoriaService

namespace Ecommerce.Application.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly IAuditoriaService _auditoriaService; 

        // Construtor modificado
        public AuthService(
            IUsuarioRepository usuarioRepository, 
            ITokenService tokenService, 
            IAuditoriaService auditoriaService) 
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
            _auditoriaService = auditoriaService; 
        }

        // Método de Registro (com Auditoria CORRIGIDA)
        public async Task<bool> RegistrarAsync(RegistrarUsuarioDto dto)
        {
            var usuarioExistente = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuarioExistente != null)
            {
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

            // LOG DE AUDITORIA (CORRIGIDO)
            await _auditoriaService.RegistrarLog(
                "Auth: Registrar", 
                $"Novo usuário registrado: {novoUsuario.Email} (ID: {novoUsuario.Id})"
            );

            return true;
        }

        // --- MÉTODO DE LOGIN (COM AUDITORIA CORRIGIDA) ---
        public async Task<LoginPasso1ResponseDto?> LoginAsync(LoginUsuarioDto dto)
        {
            // 1. Buscar o usuário pelo e-mail
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuario == null)
            {
                return null; 
            }

            // 2. Verificar a senha
            if (!BCryptNet.Verify(dto.Senha, usuario.SenhaHash))
            {
                // LOG DE AUDITORIA (CORRIGIDO)
                if (usuario != null)
                {
                    await _auditoriaService.RegistrarLog(
                        "Auth: Login (Passo 1)", 
                        $"Falha no login (senha incorreta) para: {dto.Email} (ID: {usuario.Id})"
                    );
                }
                return null; 
            }

            // --- LÓGICA DE A2F (RF11) ---
            var codigoA2F = new Random().Next(100000, 999999).ToString("D6"); 
            
            usuario.CodigoA2F = codigoA2F;
            usuario.DataExpiracaoCodigoA2F = DateTime.UtcNow.AddMinutes(5);

            _usuarioRepository.Update(usuario);
            await _usuarioRepository.SaveChangesAsync();

            // LOG DE AUDITORIA (CORRIGIDO)
            await _auditoriaService.RegistrarLog(
                "Auth: Login (Passo 1)", 
                $"Login (Passo 1) bem-sucedido. Código A2F gerado para: {usuario.Email} (ID: {usuario.Id})"
            );

            // Simula o envio do código por e-mail
            var responseA2F = new LoginA2FRequeridoDto
            {
                Mensagem = "Senha correta. Insira o código de 6 dígitos (simulando envio por e-mail).",
                CodigoA2FSimulado = codigoA2F
            };

            return new LoginPasso1ResponseDto
            {
                A2FRequerido = true,
                A2FInfo = responseA2F,
                LoginSucesso = null 
            };
        }

        // --- NOVO MÉTODO: VALIDAR A2F (COM AUDITORIA CORRIGIDA) ---
        public async Task<LoginResponseDto?> ValidarA2FAsync(ValidarA2FDto dto)
        {
            // 1. Buscar o usuário pelo e-mail
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuario == null)
            {
                return null; 
            }

            // 2. Validar o Código e a Expiração
            if (usuario.CodigoA2F != dto.CodigoA2F || usuario.DataExpiracaoCodigoA2F <= DateTime.UtcNow)
            {
                // Limpa o código inválido/expirado
                usuario.CodigoA2F = null;
                usuario.DataExpiracaoCodigoA2F = null;
                _usuarioRepository.Update(usuario);
                await _usuarioRepository.SaveChangesAsync();
                
                // LOG DE AUDITORIA (CORRIGIDO)
                await _auditoriaService.RegistrarLog(
                    "Auth: Login (Passo 2)", 
                    $"Falha na validação A2F (código incorreto ou expirado) para: {dto.Email} (ID: {usuario.Id})"
                );

                return null; 
            }

            // 3. Sucesso! Limpa o código
            usuario.CodigoA2F = null;
            usuario.DataExpiracaoCodigoA2F = null;
            _usuarioRepository.Update(usuario);

            // 4. Gerar o Token JWT
            var token = _tokenService.GerarToken(usuario);

            await _usuarioRepository.SaveChangesAsync(); 

            // LOG DE AUDITORIA (CORRIGIDO)
            await _auditoriaService.RegistrarLog(
                "Auth: Login (Passo 2)", 
                $"Login (Passo 2) bem-sucedido. Token JWT gerado para: {usuario.Email} (ID: {usuario.Id})"
            );

            // 5. Retornar a Resposta Final
            return new LoginResponseDto
            {
                Token = token,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }
    }
}

