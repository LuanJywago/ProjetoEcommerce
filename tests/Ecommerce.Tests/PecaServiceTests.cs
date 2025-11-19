using Xunit;
using Moq;
using FluentAssertions;
using Ecommerce.Application.Features.Pecas.Services;
using Ecommerce.Application.Interfaces; // Para IPecaRepository
using Ecommerce.Application.Features.Pecas.DTOs;
using Ecommerce.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ecommerce.Tests
{
    public class PecaServiceTests
    {
        // Mockamos (simulamos) o repositório
        private readonly Mock<IPecaRepository> _mockPecaRepository;
        // Testamos o serviço real
        private readonly PecaService _pecaService;

        public PecaServiceTests()
        {
            _mockPecaRepository = new Mock<IPecaRepository>();
            
            // CORREÇÃO: O construtor agora só recebe 1 argumento (o repositório)
            _pecaService = new PecaService(_mockPecaRepository.Object);
        }

        [Fact]
        public async Task CriarPeca_ComDadosValidos_DeveChamarRepositorio()
        {
            // Arrange (Preparar)
            var dto = new CriarPecaDto 
            { 
                Nome = "Peça Teste", 
                Categoria = "Motor", // CORREÇÃO: Usamos Categoria
                Preco = 50.0m,
                Estoque = 100 // CORREÇÃO: Usamos Estoque
            };

            // Act (Agir)
            var result = await _pecaService.CriarPeca(dto);

            // Assert (Verificar)
            // Verifica se AdicionarAsync foi chamado 1 vez
            _mockPecaRepository.Verify(repo => repo.AdicionarAsync(It.IsAny<Peca>()), Times.Once);
            
            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Estoque.Should().Be(dto.Estoque);
        }

        [Fact]
        public async Task ObterTodasPecas_DeveRetornarLista()
        {
            // Arrange
            var listaMock = new List<Peca>
            {
                new Peca { Id = Guid.NewGuid(), Nome = "Peca 1", Estoque = 10 },
                new Peca { Id = Guid.NewGuid(), Nome = "Peca 2", Estoque = 20 }
            };

            // CORREÇÃO: Usamos ObterTodasAsync
            _mockPecaRepository.Setup(repo => repo.ObterTodasAsync())
                               .ReturnsAsync(listaMock);

            // Act
            var result = await _pecaService.ObterTodasPecas();

            // Assert
            result.Should().HaveCount(2);
            _mockPecaRepository.Verify(repo => repo.ObterTodasAsync(), Times.Once);
        }

        [Fact]
        public async Task ObterPecaPorId_ComIdExistente_DeveRetornarPeca()
        {
            // Arrange
            var id = Guid.NewGuid();
            var pecaMock = new Peca { Id = id, Nome = "Peca Teste" };

            // CORREÇÃO: Usamos ObterPorIdAsync
            _mockPecaRepository.Setup(repo => repo.ObterPorIdAsync(id))
                               .ReturnsAsync(pecaMock);

            // Act
            var result = await _pecaService.ObterPecaPorId(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task AtualizarPeca_DeveChamarUpdateNoRepositorio()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new AtualizarPecaDto 
            { 
                Nome = "Nome Atualizado", 
                Categoria = "Nova Categoria",
                Preco = 99.9m,
                Estoque = 50 
            };

            var pecaExistente = new Peca { Id = id, Nome = "Antigo" };

            _mockPecaRepository.Setup(repo => repo.ObterPorIdAsync(id))
                               .ReturnsAsync(pecaExistente);

            // Act
            await _pecaService.AtualizarPeca(id, dto);

            // Assert
            // Verifica se AtualizarAsync foi chamado
            _mockPecaRepository.Verify(repo => repo.AtualizarAsync(It.Is<Peca>(p => p.Nome == "Nome Atualizado")), Times.Once);
        }

        [Fact]
        public async Task DeletarPeca_DeveChamarDeleteNoRepositorio()
        {
            // Arrange
            var id = Guid.NewGuid();
            var pecaExistente = new Peca { Id = id };

            _mockPecaRepository.Setup(repo => repo.ObterPorIdAsync(id))
                               .ReturnsAsync(pecaExistente);

            // Act
            await _pecaService.DeletarPeca(id);

            // Assert
            // Verifica se DeletarAsync foi chamado
            _mockPecaRepository.Verify(repo => repo.DeletarAsync(pecaExistente), Times.Once);
        }
    }
}