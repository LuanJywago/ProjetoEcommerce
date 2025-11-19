using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Domain.Entities
{
    public class PedidoItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PedidoId { get; set; }
        
        public Guid PecaId { get; set; }

        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }
    }
}