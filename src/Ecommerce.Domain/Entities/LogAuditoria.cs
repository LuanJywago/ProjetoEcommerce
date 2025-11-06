using System;

namespace Ecommerce.Domain.Entities
{
    public class LogAuditoria
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Acao { get; set; } = string.Empty; // Ex: "Exclusão de Peça"
        
        public Guid UsuarioId { get; set; } // Quem fez (Atende RF09 e RN04)
        public Usuario? Usuario { get; set; }
        
        public string Detalhes { get; set; } = string.Empty; // Ex: "Peça ID 123 foi excluída"
    }
}