using Ecommerce.Application.Features.Pecas.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Pecas.Services
{
    public interface IPecaService
    {
        // Método que lança exceção se o preço for 0
        Task<PecaResponseDto> CriarPeca(CriarPecaDto dto); 
        
        // Método que lança exceção se não encontrar ou o preço for 0
        Task AtualizarPeca(Guid id, AtualizarPecaDto dto); 
        
        // Método que lança exceção se não encontrar
        Task DeletarPeca(Guid id); 
        
        // Este continua igual (retorna null para 404)
        Task<PecaResponseDto?> ObterPecaPorId(Guid id); 
        
        Task<IEnumerable<PecaResponseDto>> ObterTodasPecas();
    }
}