using System;

namespace Ecommerce.Domain.Entities
{
    // O enum TipoUsuario continua o mesmo
    public enum TipoUsuario
    {
        Cliente,     // 0
        Funcionario, // 1
        Admin        // 2
    }

    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public TipoUsuario Tipo { get; set; }
    }
}