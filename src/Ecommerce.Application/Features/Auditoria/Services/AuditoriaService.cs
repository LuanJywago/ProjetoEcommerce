using Ecommerce.Application.Features.Auditoria.Services; // Ajuste se necessário
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces; // Para ILogAuditoriaRepository
using System;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Auditoria.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogAuditoriaRepository _repository;

        public AuditoriaService(ILogAuditoriaRepository repository)
        {
            _repository = repository;
        }

        public async Task RegistrarLog(string acao, string detalhes)
        {
            try 
            {
                var log = new LogAuditoria
                {
                    Id = Guid.NewGuid(),
                    DataHora = DateTime.UtcNow,
                    Acao = acao,
                    Detalhes = detalhes,
                    UsuarioId = Guid.Empty 
                };

                await _repository.CriarAsync(log); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FALHA AO GRAVAR LOG: {ex.Message}");
            }
        }
    }
}