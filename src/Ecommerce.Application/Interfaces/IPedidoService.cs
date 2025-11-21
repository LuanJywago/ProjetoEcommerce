using Ecommerce.Application.Features.Pedidos.DTOs;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic; // <--- Necessário para IEnumerable
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<Pedido> RegistrarVendaAsync(RealizarPedidoDto pedidoDto, Guid usuarioId);

        // --- O ERRO ESTAVA AQUI: FALTAVA DECLARAR ESTA LINHA ---
        Task<IEnumerable<Pedido>> ObterPedidosPorUsuarioAsync(Guid usuarioId);
    }
}