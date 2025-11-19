using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Auditoria.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogAuditoriaRepository _logRepository;

        public AuditoriaService(ILogAuditoriaRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task RegistrarLog(string acao, string detalhes, Guid? usuarioId = null)
        {
            var log = new LogAuditoria
            {
                Id = Guid.NewGuid(),
                Acao = acao,
                Detalhes = detalhes,
                // CORREÇÃO: Usamos 'DataHora' porque é o nome que está na entidade LogAuditoria.cs
                DataHora = DateTime.UtcNow, 
                UsuarioId = usuarioId
            };

            await _logRepository.AddAsync(log);
        }

        public Task RegistrarLog(string acao, string detalhes)
        {
            throw new NotImplementedException();
        }
    }
}