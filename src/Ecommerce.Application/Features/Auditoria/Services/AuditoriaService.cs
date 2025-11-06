using Ecommerce.Application.Interfaces; // ILogAuditoriaRepository, ICurrentUserService
using Ecommerce.Domain.Entities; // LogAuditoria
using System;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Auditoria.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogAuditoriaRepository _logRepository;
        private readonly ICurrentUserService _currentUserService;

        public AuditoriaService(ILogAuditoriaRepository logRepository, ICurrentUserService currentUserService)
        {
            _logRepository = logRepository;
            _currentUserService = currentUserService;
        }

        public async Task RegistrarLog(string acao, string detalhes)
        {
            // Pega o ID do usuário logado (ou usa um Guid vazio se não houver usuário - ex: erro interno)
            var usuarioId = _currentUserService.GetUserId() ?? Guid.Empty;

            var log = new LogAuditoria
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow, // Usa a hora universal (boa prática)
                UsuarioId = usuarioId,
                Acao = acao,
                Detalhes = detalhes
            };

            await _logRepository.AddAsync(log);
            // NÃO chama SaveChangesAsync aqui. Quem chamou o serviço de log é que vai salvar.
        }
    }
}