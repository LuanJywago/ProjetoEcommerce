using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface IPecaRepository
    {
        Task AdicionarAsync(Peca peca);
        Task<IEnumerable<Peca>> ObterTodasAsync();
        Task<Peca?> ObterPorIdAsync(Guid id);
        Task AtualizarAsync(Peca peca);
        Task DeletarAsync(Peca peca);
    }
}