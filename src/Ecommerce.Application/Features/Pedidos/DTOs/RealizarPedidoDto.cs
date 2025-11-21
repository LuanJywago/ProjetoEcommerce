using System;
using System.Collections.Generic;

namespace Ecommerce.Application.Features.Pedidos.DTOs
{
    public class RealizarPedidoDto
    {
        public List<ItemPedidoDto> Itens { get; set; } = new List<ItemPedidoDto>();
    }

    public class ItemPedidoDto
    {
        public Guid PecaId { get; set; }
        public int Quantidade { get; set; }
    }
}