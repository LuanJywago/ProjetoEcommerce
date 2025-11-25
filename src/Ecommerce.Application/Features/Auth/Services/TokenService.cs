// ========================================================================
// === IMPORTAÇÕES (USINGS) ===
// (Estes são os 'usings' que estavam faltando e causando os erros)
// ========================================================================
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.Extensions.Configuration; // Para ler o appsettings.json
using Microsoft.IdentityModel.Tokens;     // Para SecurityAlgorithms e SymmetricSecurityKey
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;    // Para JwtRegisteredClaimNames e JwtSecurityToken
using System.Security.Claims;             // Para Claim e ClaimTypes
using System.Text;                        // Para Encoding

namespace Ecommerce.Application.Features.Auth.Services
{
    public class TokenService : ITokenService
    {
        // --- MUDANÇA (PASSO 47) ---
        // Precisamos do IConfiguration para ler o appsettings.json
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key; // Chave simétrica
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Lemos as configurações do appsettings.json
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException(nameof(jwtKey), "A chave secreta JWT ('Jwt:Key') não foi encontrada no appsettings.json para a GERAÇÃO.");
            }
            
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            _issuer = _configuration["Jwt:Issuer"] ?? "Ecommerce.API";
            _audience = _configuration["Jwt:Audience"] ?? "Ecommerce.App";

            // Log de depuração (Opcional, mas bom)
            Console.WriteLine("--- CRIAÇÃO JWT ---");
            Console.WriteLine($"[TokenService.cs] Lendo Jwt:Key: '{jwtKey}'");
            Console.WriteLine($"[TokenService.cs] Lendo Jwt:Issuer: '{_issuer}'");
            Console.WriteLine($"[TokenService.cs] Lendo Jwt:Audience: '{_audience}'");
            Console.WriteLine("-------------------");
        }

        public string GerarToken(Usuario usuario)
        {
            // 1. Criar a lista de "Claims" (informações dentro do token)
            var claims = new List<Claim>
            {
                // Subject (O ID do usuário)
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                
                // Email
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                
                // NameId (Nome de usuário)
                new Claim(JwtRegisteredClaimNames.NameId, usuario.Email),
                
                // Jti (ID único do token)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                new Claim(ClaimTypes.Role, usuario.Tipo.ToString())
            };

            // 2. Definir as credenciais de assinatura
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            // 3. Definir a data de expiração (ex: 2 horas)
            var expiraEm = DateTime.UtcNow.AddHours(2); 

            // 4. Criar o Token
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiraEm,
                signingCredentials: creds
            );

            // 5. Escrever o token como uma string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}