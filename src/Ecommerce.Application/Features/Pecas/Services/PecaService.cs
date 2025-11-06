using Ecommerce.Application.Features.Pecas.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Exceptions;
// --- NOVO USING ---
using Ecommerce.Application.Features.Auditoria.Services; // Para IAuditoriaService

namespace Ecommerce.Application.Features.Pecas.Services
{
    // A interface IPecaService não muda

    public class PecaService : IPecaService
    {
        private readonly IPecaRepository _pecaRepository;
        // --- NOVA DEPENDÊNCIA ---
        private readonly IAuditoriaService _auditoriaService;

        // --- CONSTRUTOR ATUALIZADO ---
        public PecaService(IPecaRepository pecaRepository, IAuditoriaService auditoriaService)
        {
            _pecaRepository = pecaRepository;
            _auditoriaService = auditoriaService; // Injeta o serviço de auditoria
        }

        public async Task<PecaResponseDto> CriarPeca(CriarPecaDto dto)
        {
            if (dto.Preco <= 0) { throw new ValidacaoException("..."); }
            if (dto.QuantidadeEstoque < 0) { throw new ValidacaoException("..."); }

            var peca = new Peca { /* ... mapeamento ... */
                Id = Guid.NewGuid(), Nome = dto.Nome, Descricao = dto.Descricao, Tamanho = dto.Tamanho, Preco = dto.Preco, QuantidadeEstoque = dto.QuantidadeEstoque
             };

            await _pecaRepository.AddAsync(peca);

            // --- REGISTRAR LOG DE CRIAÇÃO ---
            await _auditoriaService.RegistrarLog("Criar Peça", $"Peça '{peca.Nome}' (ID: {peca.Id}) criada.");

            await _pecaRepository.SaveChangesAsync(); // Salva a peça E o log

            return new PecaResponseDto { /* ... mapeamento ... */
                Id = peca.Id, Nome = peca.Nome, Descricao = peca.Descricao, Tamanho = peca.Tamanho, Preco = peca.Preco, QuantidadeEstoque = peca.QuantidadeEstoque
             };
        }

        public async Task AtualizarPeca(Guid id, AtualizarPecaDto dto)
        {
            var pecaExistente = await _pecaRepository.GetByIdAsync(id);
            if (pecaExistente == null) { throw new KeyNotFoundException("..."); }
            if (dto.Preco <= 0) { throw new ValidacaoException("..."); }
            if (dto.QuantidadeEstoque < 0) { throw new ValidacaoException("..."); }

            // Guarda o nome antigo para o log
            string nomeAntigo = pecaExistente.Nome;

            // Atualiza o objeto
            pecaExistente.Nome = dto.Nome;
            pecaExistente.Descricao = dto.Descricao;
            pecaExistente.Tamanho = dto.Tamanho;
            pecaExistente.Preco = dto.Preco;
            pecaExistente.QuantidadeEstoque = dto.QuantidadeEstoque;

            _pecaRepository.Update(pecaExistente);

            // --- REGISTRAR LOG DE ATUALIZAÇÃO ---
            await _auditoriaService.RegistrarLog("Atualizar Peça", $"Peça ID: {id} (Nome anterior: '{nomeAntigo}') atualizada para Nome: '{dto.Nome}'.");

            await _pecaRepository.SaveChangesAsync(); // Salva a peça E o log
        }

        public async Task DeletarPeca(Guid id)
        {
            var pecaExistente = await _pecaRepository.GetByIdAsync(id);
            if (pecaExistente == null) { throw new KeyNotFoundException("..."); }

            _pecaRepository.Remove(pecaExistente);

            // --- REGISTRAR LOG DE DELEÇÃO ---
            await _auditoriaService.RegistrarLog("Deletar Peça", $"Peça '{pecaExistente.Nome}' (ID: {id}) deletada.");

            await _pecaRepository.SaveChangesAsync(); // Salva a deleção E o log
        }

        // --- MÉTODOS DE LEITURA (SEM LOG) ---
        public async Task<PecaResponseDto?> ObterPecaPorId(Guid id)
        {
            // ... (código igual) ...
            var peca = await _pecaRepository.GetByIdAsync(id);
            if (peca == null) return null;
            return new PecaResponseDto { /* ... mapeamento ... */ Id = peca.Id, Nome = peca.Nome, Descricao = peca.Descricao, Tamanho = peca.Tamanho, Preco = peca.Preco, QuantidadeEstoque = peca.QuantidadeEstoque };
        }

        public async Task<IEnumerable<PecaResponseDto>> ObterTodasPecas()
        {
            // ... (código igual) ...
             var pecas = await _pecaRepository.GetAllAsync();
            return pecas.Select(peca => new PecaResponseDto { /* ... mapeamento ... */ Id = peca.Id, Nome = peca.Nome, Descricao = peca.Descricao, Tamanho = peca.Tamanho, Preco = peca.Preco, QuantidadeEstoque = peca.QuantidadeEstoque });
        }
    }
}