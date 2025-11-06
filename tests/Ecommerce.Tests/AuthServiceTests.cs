using Xunit;
using Moq;
using FluentAssertions;
using Ecommerce.Application.Features.Auth.Services; // Serviço a testar
using Ecommerce.Application.Interfaces; // IUsuarioRepository
using Ecommerce.Application.Features.Auth.DTOs; // DTOs
using Ecommerce.Domain.Entities; // Usuario
using System;
using System.Threading.Tasks;
// Precisamos referenciar o BCrypt para simular a verificação de senha
using BCrypt.Net;
// --- ADICIONADO ---
using Ecommerce.Application.Features.Auditoria.Services; // Para o IAuditoriaService

namespace Ecommerce.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
        private readonly Mock<ITokenService> _mockTokenService; 
        private readonly Mock<IAuditoriaService> _mockAuditoriaService; // --- ADICIONADO ---
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUsuarioRepository = new Mock<IUsuarioRepository>();
            _mockTokenService = new Mock<ITokenService>();
            _mockAuditoriaService = new Mock<IAuditoriaService>(); // --- ADICIONADO ---

            // Cria a instância real do AuthService, injetando os Mocks
            // --- LINHA CORRIGIDA ---
            _authService = new AuthService(
                _mockUsuarioRepository.Object, 
                _mockTokenService.Object,
                _mockAuditoriaService.Object // --- ADICIONADO ---
            );
        }

        // --- TESTE 4 (Novo) ---
        [Fact]
        public async Task RegistrarAsync_ComEmailJaExistente_DeveRetornarFalse()
        {
            // Arrange
            var dtoRegistro = new RegistrarUsuarioDto { Nome = "Teste", Email = "existente@email.com", Senha = "123" };
            var usuarioExistente = new Usuario { Id = Guid.NewGuid(), Email = "existente@email.com", Nome = "Outro", SenhaHash = "hash" };

            // Configura o Mock: Quando GetByEmailAsync for chamado com o email do DTO,
            // retorna o usuarioExistente (simulando que o email já está no banco).
            _mockUsuarioRepository.Setup(repo => repo.GetByEmailAsync(dtoRegistro.Email))
                                    .ReturnsAsync(usuarioExistente);

            // Act
            var resultado = await _authService.RegistrarAsync(dtoRegistro);

            // Assert
            // Verifica se o resultado foi 'false'
            resultado.Should().BeFalse();

            // Verifica se AddAsync NUNCA foi chamado (pois o usuário já existia)
            _mockUsuarioRepository.Verify(repo => repo.AddAsync(It.IsAny<Usuario>()), Times.Never);
            _mockUsuarioRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);

            // Verifica se a auditoria NUNCA foi chamada (pois o método retornou 'false' antes)
            // Nota: Se a lógica mudasse para logar a tentativa, este teste precisaria mudar.
            _mockAuditoriaService.Verify(aud => aud.RegistrarLog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // --- TESTE 5 (Novo) ---
        [Fact]
        public async Task LoginAsync_ComSenhaIncorreta_DeveRetornarNull()
        {
            // Arrange
            var dtoLogin = new LoginUsuarioDto { Email = "usuario@email.com", Senha = "senhaErrada" };
            var senhaCorretaHash = BCrypt.Net.BCrypt.HashPassword("senhaCorreta"); // Gera um hash da senha correta
            var usuarioNoBanco = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "usuario@email.com",
                Nome = "Usuario Teste",
                SenhaHash = senhaCorretaHash // O hash salvo no banco
            };

            // Configura o Mock: Quando GetByEmailAsync for chamado, retorna o usuarioNoBanco
            _mockUsuarioRepository.Setup(repo => repo.GetByEmailAsync(dtoLogin.Email))
                                    .ReturnsAsync(usuarioNoBanco);

            // Act
            var resultadoLogin = await _authService.LoginAsync(dtoLogin);

            // Assert
            // Verifica se o resultado foi null (pois a senha "senhaErrada" não bate com o hash de "senhaCorreta")
            resultadoLogin.Should().BeNull();

            // Verifica se o GerarToken NUNCA foi chamado
            _mockTokenService.Verify(ts => ts.GerarToken(It.IsAny<Usuario>()), Times.Never);

            // --- ADICIONADO ---
            // Verifica se a auditoria de FALHA DE SENHA foi chamada 1 vez
            _mockAuditoriaService.Verify(aud => aud.RegistrarLog(
                "Auth: Login (Passo 1)", 
                It.Is<string>(s => s.Contains("Falha no login (senha incorreta)"))
            ), Times.Once);
        }
    }
}
