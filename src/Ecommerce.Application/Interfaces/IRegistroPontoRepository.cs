using Ecommerce.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface IRegistroPontoRepository
    {
        // Procura o último registo de ponto para um funcionário que AINDA NÃO tem DataHoraSaida - ou seja, Ponto de Saída.
        Task<RegistroPonto?> GetUltimoPontoAbertoAsync(Guid funcionarioId);

        Task AddAsync(RegistroPonto registroPonto);
        void Update(RegistroPonto registroPonto); 
        Task<int> SaveChangesAsync();
    }
}