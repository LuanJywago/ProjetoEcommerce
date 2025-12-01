using Ecommerce.Application.Features.Pecas.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Pecas.Services
{
    public class PecaService : IPecaService
    {
        //Essa parte é responsável por guardar o Repositório usando a INTERFACE, fazendo um desacoplamento, pois o service não sabe qual BD Existente
        //Não cria conexão com o banco, ele ja recebe pronto, podendo alterar o BD futuramente sem quebrar a RN
        private readonly IPecaRepository _pecaRepository;
        // CONSTRUTOR: Injeção de dependencia
        // Quando a API pede um PecaService, o sistema já entrega lá automaticamente pra quem estiver implementando o IPecaRepository pela infraestrutura
        public PecaService(IPecaRepository pecaRepository)
        {
            _pecaRepository = pecaRepository;
        }

        public async Task<PecaResponseDto> CriarPeca(CriarPecaDto dto)
        {
            //1. o DTO (Data Transfer Object - Caixa de transporte somente) Chega da API feita
            //2. 
            // Mapeamento Manual (DTO -> Entidade)
            var peca = new Peca
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Categoria = dto.Categoria,
                Preco = dto.Preco,
                Estoque = dto.Estoque, // Nome correto
                DataCadastro = DateTime.UtcNow
            };

            await _pecaRepository.AdicionarAsync(peca);
            //Repositório é chamado para salvar
            // PQ Async? Não trava o Servidor enquanto o BD trabalha

            // Mapeamento Manual (Entidade -> DTO de Resposta)
            return new PecaResponseDto
            {
                Id = peca.Id,
                Nome = peca.Nome,
                Categoria = peca.Categoria,
                Preco = peca.Preco,
                Estoque = peca.Estoque,
                DataCadastro = peca.DataCadastro
            };
        }

        public async Task<IEnumerable<PecaResponseDto>> ObterTodasPecas()
        {
            var pecas = await _pecaRepository.ObterTodasAsync();

            // Converte a lista de Entidades para lista de DTOs
            return pecas.Select(p => new PecaResponseDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Categoria = p.Categoria,
                Preco = p.Preco,
                Estoque = p.Estoque,
                DataCadastro = p.DataCadastro
            });
        }

        public async Task<PecaResponseDto?> ObterPecaPorId(Guid id)
        {
            var peca = await _pecaRepository.ObterPorIdAsync(id);
            if (peca == null) return null;

            return new PecaResponseDto
            {
                Id = peca.Id,
                Nome = peca.Nome,
                Categoria = peca.Categoria,
                Preco = peca.Preco,
                Estoque = peca.Estoque,
                DataCadastro = peca.DataCadastro
            };
        }

        public async Task AtualizarPeca(Guid id, AtualizarPecaDto dto)
        {
            var peca = await _pecaRepository.ObterPorIdAsync(id);
            if (peca == null) throw new Exception("Peça não encontrada");

            // Atualiza os campos
            peca.Nome = dto.Nome;
            peca.Categoria = dto.Categoria;
            peca.Preco = dto.Preco;
            peca.Estoque = dto.Estoque;

            await _pecaRepository.AtualizarAsync(peca);
        }

        public async Task DeletarPeca(Guid id)
        {
            var peca = await _pecaRepository.ObterPorIdAsync(id);
            if (peca != null)
            {
                await _pecaRepository.DeletarAsync(peca);
            }
        }
    }
}