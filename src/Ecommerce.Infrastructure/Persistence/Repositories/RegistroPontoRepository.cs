using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence; // AppDbContext
using Microsoft.EntityFrameworkCore; // Para FirstOrDefaultAsync, OrderByDescending
using System;
using System.Linq; // Para OrderByDescending, Where
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Persistence.Repositories
{
    public class RegistroPontoRepository : IRegistroPontoRepository
    {
        private readonly AppDbContext _context;

        public RegistroPontoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RegistroPonto registroPonto)
        {
            await _context.RegistrosPonto.AddAsync(registroPonto);
        }

        public async Task<RegistroPonto?> GetUltimoPontoAbertoAsync(Guid funcionarioId)
        {
            // Busca na tabela RegistrosPonto
            return await _context.RegistrosPonto
                // Filtra pelo ID do funcionário
                .Where(rp => rp.FuncionarioId == funcionarioId)
                // Filtra apenas os que NÃO têm DataHoraSaida preenchida
                .Where(rp => rp.DataHoraSaida == null)
                // Ordena pelos mais recentes primeiro
                .OrderByDescending(rp => rp.DataHoraEntrada)
                // Pega o primeiro (o mais recente aberto) ou null se não houver nenhum
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Update(RegistroPonto registroPonto)
        {
            _context.RegistrosPonto.Update(registroPonto);
        }
    }
}