using Xunit;
using Moq;
using FluentAssertions;
using Ecommerce.Application.Features.Pecas.Services;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Features.Pecas.DTOs;
using Ecommerce.Application.Exceptions;
using Ecommerce.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
// --- NOVO USING PARA O PASSO 13 ---
using Ecommerce.Application.Features.Auditoria.Services; // Para IAuditoriaService

namespace Ecommerce.Tests
{
    public class PecaServiceTests
    {
        private readonly Mock<IPecaRepository> _mockPecaRepository;
        // --- NOVA VARIÁVEL PARA O MOCK DE AUDITORIA ---
        private readonly Mock<IAuditoriaService> _mockAuditoriaService;
        private readonly PecaService _pecaService;

        public PecaServiceTests()
        {
            _mockPecaRepository = new Mock<IPecaRepository>();
            // --- CRIA O MOCK DE AUDITORIA ---
            _mockAuditoriaService = new Mock<IAuditoriaService>();

            // --- CORREÇÃO AQUI: PASSA OS DOIS MOCKS PARA O CONSTRUTOR ---
            _pecaService = new PecaService(
                _mockPecaRepository.Object,
                _mockAuditoriaService.Object // Passa o mock de auditoria
            );
        }

        // --- TESTE 1 (Já tínhamos) ---
        [Fact]
        public async Task CriarPeca_ComPrecoZero_DeveLancarValidacaoException()
        {
            // Arrange
            var dtoComPrecoZero = new CriarPecaDto { Preco = 0, /*...*/ Nome="T", Descricao="D", Tamanho="M", QuantidadeEstoque=10 };
            // Act
            Func<Task> act = () => _pecaService.CriarPeca(dtoComPrecoZero);
            // Assert
            await act.Should().ThrowAsync<ValidacaoException>().WithMessage("*...*");
        }

        // --- TESTE 2 (Já tínhamos) ---
        [Fact]
        public async Task AtualizarPeca_ComIdInexistente_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var idInexistente = Guid.NewGuid();
            var dtoAtualizacao = new AtualizarPecaDto { /*...*/ Nome="N", Preco=10, QuantidadeEstoque=1 };
            _mockPecaRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                               .ReturnsAsync((Peca?)null);
            // Act
            Func<Task> act = () => _pecaService.AtualizarPeca(idInexistente, dtoAtualizacao);
            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        // --- TESTE 3 (Já tínhamos) ---
        [Fact]
        public async Task CriarPeca_ComDadosValidos_DeveChamarRepositorioCorretamenteERetornarDto()
        {
            // Arrange
            var dtoValido = new CriarPecaDto { Nome = "Camiseta", Preco = 50m, QuantidadeEstoque = 10, /*...*/ Descricao="D", Tamanho="M" };
            _mockPecaRepository.Setup(repo => repo.AddAsync(It.IsAny<Peca>())).Returns(Task.CompletedTask);
            _mockPecaRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);
            // --- NOVO SETUP PARA AUDITORIA ---
            // Configura o mock de auditoria para não fazer nada quando RegistrarLog for chamado
            _mockAuditoriaService.Setup(auditoria => auditoria.RegistrarLog(It.IsAny<string>(), It.IsAny<string>()))
                                 .Returns(Task.CompletedTask);

            // Act
            var resultadoDto = await _pecaService.CriarPeca(dtoValido);

            // Assert
            resultadoDto.Should().NotBeNull();
            resultadoDto!.Nome.Should().Be(dtoValido.Nome);
            // Verifica se AddAsync e SaveChangesAsync foram chamados
            _mockPecaRepository.Verify(repo => repo.AddAsync(It.Is<Peca>(p => p.Nome == dtoValido.Nome)), Times.Once);
            _mockPecaRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            // --- NOVA VERIFICAÇÃO PARA AUDITORIA ---
            // Verifica se o método RegistrarLog foi chamado exatamente uma vez
            _mockAuditoriaService.Verify(auditoria => auditoria.RegistrarLog("Criar Peça", It.IsAny<string>()), Times.Once);
        }
    }
}