using Ecommerce.Domain.Entities; // Garante que o TipoUsuario é encontrado
using System.Text.Json.Serialization; // Para [JsonIgnore]

namespace Ecommerce.Application.Features.Auth.DTOs
{
    // --- DTOs ANTIGOS ---
    public class RegistrarUsuarioDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public TipoUsuario Tipo { get; set; } = TipoUsuario.Cliente; 
    }

    public class LoginUsuarioDto
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // --- DTOs NOVOS (Para A2F) ---
    public class LoginA2FRequeridoDto
    {
        public string Mensagem { get; set; } = string.Empty;
        public string CodigoA2FSimulado { get; set; } = string.Empty; 
    }

    public class ValidarA2FDto
    {
        public string Email { get; set; } = string.Empty;
        public string CodigoA2F { get; set; } = string.Empty;
    }

    public class LoginPasso1ResponseDto
    {
        public bool A2FRequerido { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LoginResponseDto? LoginSucesso { get; set; } 

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LoginA2FRequeridoDto? A2FInfo { get; set; }
    }
}