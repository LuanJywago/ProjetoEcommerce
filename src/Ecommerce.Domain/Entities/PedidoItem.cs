using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Importante para evitar ciclos no JSON se necessário

namespace Ecommerce.Domain.Entities
{
    // Renomeado de ItensPedido para PedidoItem para combinar com a lista do Pedido.cs
    public class PedidoItem 
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PedidoId { get; set; }
        [JsonIgnore] // Evita loop infinito ao serializar
        public virtual Pedido? Pedido { get; set; } 
        
        public Guid PecaId { get; set; }
        public virtual Peca? Peca { get; set; } // Necessário para acessar o Nome e Preço da peça depois

        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }
    }
}