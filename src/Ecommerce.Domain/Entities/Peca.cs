using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Domain.Entities
{
    public class Peca
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        public int Estoque { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}