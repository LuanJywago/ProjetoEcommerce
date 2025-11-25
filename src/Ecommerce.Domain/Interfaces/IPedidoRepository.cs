using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        Task<Pedido> CriarAsync(Pedido pedido);
        Task<Pedido?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Pedido>> ListarPorUsuarioAsync(Guid usuarioId);

        Task<decimal> ObterFaturamentoTotalAsync();
        Task<int> ObterTotalPedidosAsync();
    }
}