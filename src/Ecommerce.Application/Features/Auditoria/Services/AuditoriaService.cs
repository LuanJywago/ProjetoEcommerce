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
                    // Se você tiver como pegar o ID do usuário aqui, ótimo. 
                    // Se não, pode deixar null ou Guid.Empty por enquanto para não travar.
                    UsuarioId = Guid.Empty 
                };

                // ATENÇÃO: Verifique se no seu ILogAuditoriaRepository o nome é CriarAsync ou AdicionarAsync
                await _repository.CriarAsync(log); 
            }
            catch (Exception ex)
            {
                // Se o log falhar, não queremos derrubar o sistema inteiro (como aconteceu no seu erro)
                Console.WriteLine($"FALHA AO GRAVAR LOG: {ex.Message}");
            }
        }
    }
}