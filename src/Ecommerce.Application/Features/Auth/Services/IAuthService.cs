using Ecommerce.Application.Features.Auth.DTOs;

namespace Ecommerce.Application.Interfaces
{
    /// <summary>
    /// Interface para o serviço de autenticação,
    /// definindo os contratos para registro, login (passo 1) e validação A2F (passo 2).
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registra um novo usuário no sistema.
        /// </summary>
        /// <param name="dto">Dados de registro (email, senha, cargo).</param>
        /// <returns>True se o registro for bem-sucedido, False caso contrário.</returns>
        Task<bool> RegistrarAsync(RegistrarUsuarioDto dto);

        /// <summary>
        /// Tenta autenticar um usuário (Passo 1 do Login).
        /// Valida as credenciais e, se corretas, gera um código A2F.
        /// </summary>
        /// <param name="dto">Dados de login (email, senha).</param>
        /// <returns>
        /// Um DTO de resposta do Passo 1 (LoginPasso1ResponseDto) se as credenciais forem válidas.
        /// Retorna null se o login falhar (usuário ou senha incorretos).
        /// </returns>
        Task<LoginPasso1ResponseDto?> LoginAsync(LoginUsuarioDto dto);

        /// <summary>
        /// Valida o código A2F fornecido pelo usuário (Passo 2 do Login).
        /// Se o código for válido e não tiver expirado, gera o token JWT final.
        /// </summary>
        /// <param name="dto">Dados de validação (email, código A2F).</param>
        /// <returns>
        /// Um DTO de resposta do Login final (LoginResponseDto) contendo o Token JWT.
        /// Retorna null se o código for inválido ou expirado.
        /// </returns>
        Task<LoginResponseDto?> ValidarA2FAsync(ValidarA2FDto dto);
    }
}
