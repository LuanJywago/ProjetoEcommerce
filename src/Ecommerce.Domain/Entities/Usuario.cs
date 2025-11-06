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

        // --- NOVOS CAMPOS PARA A2F ---
        // Guarda o código A2F de 6 dígitos (como string para facilitar)
        public string? CodigoA2F { get; set; } // '?' indica que pode ser nulo

        // Guarda a data/hora que o código expira (UTC)
        public DateTime? DataExpiracaoCodigoA2F { get; set; } // '?' indica que pode ser nulo
        // --- FIM DOS NOVOS CAMPOS ---
    }
}