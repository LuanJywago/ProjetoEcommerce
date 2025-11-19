using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Domain.Entities
{
    public class LogAuditoria
    {
        [Key]
        public Guid Id { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string Detalhes { get; set; } = string.Empty;
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
        public Guid? UsuarioId { get; set; }
    }
}