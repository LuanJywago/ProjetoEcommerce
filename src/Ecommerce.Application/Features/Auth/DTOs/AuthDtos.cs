using Ecommerce.Domain.Entities; // Necessário para reconhecer TipoUsuario
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Features.Auth.DTOs
{
    public class RegistrarUsuarioDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;
        
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Senha { get; set; } = string.Empty;
        
        // Entrada: O sistema aceita "Funcionario" (texto) no JSON e converte para Enum aqui
        public TipoUsuario Tipo { get; set; } = TipoUsuario.Cliente; 
    }

    public class LoginUsuarioDto
    {
        [Required, EmailAddress] // Validei aqui, Guilherme, pra gente saber
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; 
    }
}