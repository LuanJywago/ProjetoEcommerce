using Ecommerce.Domain.Entities;
using Microsoft.Extensions.Configuration; // Para ler o appsettings.json
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System; // Para Guid e DateTime

namespace Ecommerce.Application.Features.Auth.Services
{
    // A INTERFACE ITokenService NÃO está definida aqui dentro
    // Ela está definida corretamente no arquivo ITokenService.cs

    // Classe (Implementa a interface ITokenService)
    public class TokenService : ITokenService // <-- Implementa a interface externa
    {
        private readonly SymmetricSecurityKey _chave;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(IConfiguration config)
        {
            // Leitura segura da chave
            var chaveSecreta = config["Jwt:Key"];
            if (string.IsNullOrEmpty(chaveSecreta))
            {
                throw new ArgumentNullException(nameof(chaveSecreta), "A chave secreta JWT ('Jwt:Key') não foi encontrada ou está vazia no appsettings.json");
            }
            _chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));

            // Leitura do Issuer e Audience (com valores padrão)
            _issuer = config["Jwt:Issuer"] ?? "Ecommerce.API";
            _audience = config["Jwt:Audience"] ?? "Ecommerce.App";

            // Logs de Depuração (Para compararmos com o Program.cs)
            Console.WriteLine("--- CRIAÇÃO JWT ---");
            Console.WriteLine($"[TokenService.cs] Lendo Jwt:Key: '{chaveSecreta}'");
            Console.WriteLine($"[TokenService.cs] Lendo Jwt:Issuer: '{_issuer}'");
            Console.WriteLine($"[TokenService.cs] Lendo Jwt:Audience: '{_audience}'");
            Console.WriteLine("-------------------");
        }

        public string GerarToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.NameId, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.Tipo.ToString()) // Adiciona o Cargo/Tipo
            };

            // Algoritmo HmacSha256
            var credenciais = new SigningCredentials(_chave, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3), // Expira em 3 horas
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = credenciais
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}