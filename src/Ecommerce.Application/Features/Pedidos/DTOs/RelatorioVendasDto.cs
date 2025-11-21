namespace Ecommerce.Application.Features.Pedidos.DTOs
{
    public class RelatorioVendasDto
    {
        public decimal FaturamentoTotal { get; set; }
        public int TotalPedidos { get; set; }
        public decimal TicketMedio { get; set; } // Média de valor por pedido
    }
}