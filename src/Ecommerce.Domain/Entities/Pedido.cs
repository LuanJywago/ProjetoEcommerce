using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Domain.Entities
{
    public class Pedido
    {
        [Key]
        public Guid Id { get; set; }

        // Quem fez o pedido?
        public Guid UsuarioId { get; set; }

        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        // Lista de itens deste pedido
        public List<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
    }
}