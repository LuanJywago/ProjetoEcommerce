using Ecommerce.Application.Features.Ponto.DTOs;
using Ecommerce.Application.Interfaces; // IRegistroPontoRepository, ICurrentUserService
using Ecommerce.Domain.Entities; // RegistroPonto
using Ecommerce.Application.Exceptions; // ValidacaoException
using System;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Ponto.Services
{
    public class PontoService : IPontoService
    {
        private readonly IRegistroPontoRepository _pontoRepository;
        private readonly ICurrentUserService _currentUserService;

        public PontoService(IRegistroPontoRepository pontoRepository, ICurrentUserService currentUserService)
        {
            _pontoRepository = pontoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<RegistroPontoDto> RegistrarEntradaAsync()
        {
            var funcionarioId = _currentUserService.GetUserId();
            // Verifica se o usuário está logado (CurrentUserService retorna null se não estiver)
            if (funcionarioId == null)
            {
                // Em teoria, a segurança [Authorize] já barraria isso, mas é bom validar
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            // Verifica se já existe um ponto aberto para este funcionário
            var pontoAbertoExistente = await _pontoRepository.GetUltimoPontoAbertoAsync(funcionarioId.Value);
            if (pontoAbertoExistente != null)
            {
                // Regra de Negócio Implícita: Não pode bater entrada se já tem uma aberta
                throw new ValidacaoException("Já existe um registo de ponto em aberto. Registe a saída primeiro.");
            }

            // Cria o novo registo de ponto
            var novoRegistro = new RegistroPonto
            {
                Id = Guid.NewGuid(),
                FuncionarioId = funcionarioId.Value,
                DataHoraEntrada = DateTime.UtcNow // Hora atual universal
                // DataHoraSaida fica null por padrão
            };

            await _pontoRepository.AddAsync(novoRegistro);
            await _pontoRepository.SaveChangesAsync();

            // Mapeia para o DTO de resposta
            return new RegistroPontoDto
            {
                Id = novoRegistro.Id,
                FuncionarioId = novoRegistro.FuncionarioId,
                DataHoraEntrada = novoRegistro.DataHoraEntrada,
                DataHoraSaida = novoRegistro.DataHoraSaida,
                Status = "Entrada registrada com sucesso."
            };
        }

        public async Task<RegistroPontoDto> RegistrarSaidaAsync()
        {
            var funcionarioId = _currentUserService.GetUserId();
            if (funcionarioId == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            // Busca o último ponto em aberto deste funcionário
            var ultimoPontoAberto = await _pontoRepository.GetUltimoPontoAbertoAsync(funcionarioId.Value);

            // RN03: Precisa ter um ponto aberto para registrar a saída
            if (ultimoPontoAberto == null)
            {
                throw new ValidacaoException("Não há registo de ponto em aberto para registar a saída.");
            }

            // Atualiza a hora de saída
            ultimoPontoAberto.DataHoraSaida = DateTime.UtcNow;

            _pontoRepository.Update(ultimoPontoAberto);
            await _pontoRepository.SaveChangesAsync();

            // Mapeia para o DTO de resposta
            return new RegistroPontoDto
            {
                Id = ultimoPontoAberto.Id,
                FuncionarioId = ultimoPontoAberto.FuncionarioId,
                DataHoraEntrada = ultimoPontoAberto.DataHoraEntrada,
                DataHoraSaida = ultimoPontoAberto.DataHoraSaida,
                Status = "Saída registrada com sucesso."
            };
        }
    }
}