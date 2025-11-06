using Xunit;
using Moq;
using FluentAssertions;
using Ecommerce.Application.Features.Ponto.Services; // Serviço a testar
using Ecommerce.Application.Interfaces; // Repositório e CurrentUser
using Ecommerce.Application.Features.Ponto.DTOs; // DTO
using Ecommerce.Domain.Entities; // Entidade RegistroPonto
using Ecommerce.Application.Exceptions; // ValidacaoException
using System;
using System.Threading.Tasks;

namespace Ecommerce.Tests
{
    public class PontoServiceTests // Mudança de nome aqui também pode ter ocorrido, verifique
    {
        private readonly Mock<IRegistroPontoRepository> _mockPontoRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly PontoService _pontoService;
        private readonly Guid _funcionarioIdLogado = Guid.NewGuid(); // ID Fixo para os testes

        public PontoServiceTests() // E aqui
        {
            _mockPontoRepository = new Mock<IRegistroPontoRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();

            // Configura o mock do usuário atual para SEMPRE retornar nosso ID fixo
            _mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_funcionarioIdLogado);

            // Cria a instância do serviço com os mocks
            _pontoService = new PontoService(_mockPontoRepository.Object, _mockCurrentUserService.Object);
        }

        // --- TESTE 1: Registrar Entrada com Sucesso ---
        [Fact]
        public async Task RegistrarEntradaAsync_QuandoNaoHaPontoAberto_DeveRegistrarComSucesso()
        {
            // Arrange
            _mockPontoRepository.Setup(r => r.GetUltimoPontoAbertoAsync(_funcionarioIdLogado))
                                .ReturnsAsync((RegistroPonto?)null); // Nenhum ponto aberto
            _mockPontoRepository.Setup(r => r.AddAsync(It.IsAny<RegistroPonto>())).Returns(Task.CompletedTask);
            _mockPontoRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var resultadoDto = await _pontoService.RegistrarEntradaAsync();

            // Assert
            resultadoDto.Should().NotBeNull();
            resultadoDto.FuncionarioId.Should().Be(_funcionarioIdLogado);
            resultadoDto.DataHoraSaida.Should().BeNull();
            _mockPontoRepository.Verify(r => r.AddAsync(It.IsAny<RegistroPonto>()), Times.Once);
            _mockPontoRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // --- TESTE 2: Tentar Registrar Entrada com Ponto Já Aberto ---
        [Fact]
        public async Task RegistrarEntradaAsync_QuandoJaHaPontoAberto_DeveLancarValidacaoException()
        {
            // Arrange
            var pontoAbertoExistente = new RegistroPonto { Id = Guid.NewGuid(), FuncionarioId = _funcionarioIdLogado, DataHoraEntrada = DateTime.UtcNow };
            _mockPontoRepository.Setup(r => r.GetUltimoPontoAbertoAsync(_funcionarioIdLogado))
                                .ReturnsAsync(pontoAbertoExistente); // Retorna ponto aberto

            // Act
            Func<Task> act = () => _pontoService.RegistrarEntradaAsync();

            // Assert
            await act.Should().ThrowAsync<ValidacaoException>()
                     .WithMessage("*Já existe*");
        }

        // --- TESTE 3: Registrar Saída com Sucesso ---
        [Fact]
        public async Task RegistrarSaidaAsync_QuandoHaPontoAberto_DeveRegistrarComSucesso()
        {
            // Arrange
            var pontoAbertoParaFechar = new RegistroPonto { Id = Guid.NewGuid(), FuncionarioId = _funcionarioIdLogado, DataHoraEntrada = DateTime.UtcNow.AddHours(-1) };
            _mockPontoRepository.Setup(r => r.GetUltimoPontoAbertoAsync(_funcionarioIdLogado))
                                .ReturnsAsync(pontoAbertoParaFechar); // Retorna ponto aberto
            _mockPontoRepository.Setup(r => r.Update(It.IsAny<RegistroPonto>()));
            _mockPontoRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var resultadoDto = await _pontoService.RegistrarSaidaAsync();

            // Assert
            resultadoDto.Should().NotBeNull();
            resultadoDto.DataHoraSaida.Should().NotBeNull();
            _mockPontoRepository.Verify(r => r.Update(It.Is<RegistroPonto>(p => p.DataHoraSaida != null)), Times.Once);
            _mockPontoRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // --- TESTE 4: Tentar Registrar Saída Sem Ponto Aberto ---
        [Fact]
        public async Task RegistrarSaidaAsync_QuandoNaoHaPontoAberto_DeveLancarValidacaoException()
        {
            // Arrange
            _mockPontoRepository.Setup(r => r.GetUltimoPontoAbertoAsync(_funcionarioIdLogado))
                                .ReturnsAsync((RegistroPonto?)null); // Nenhum ponto aberto

            // Act
            Func<Task> act = () => _pontoService.RegistrarSaidaAsync();

            // Assert
            await act.Should().ThrowAsync<ValidacaoException>()
                     .WithMessage("*Não há registo*");
        }
    }
}