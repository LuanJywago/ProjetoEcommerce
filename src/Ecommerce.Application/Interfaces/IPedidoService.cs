using Ecommerce.Application.Features.Pedidos.DTOs;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<Pedido> RegistrarVendaAsync(RealizarPedidoDto pedidoDto, Guid usuarioId);
        Task<IEnumerable<Pedido>> ObterPedidosPorUsuarioAsync(Guid usuarioId);
        
        // --- NOVO MÉTODO PARA O RELATÓRIO ---
        Task<RelatorioVendasDto> GerarRelatorioAdminAsync();
    }
}