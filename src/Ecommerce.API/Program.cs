// ========================================================================
// === IMPORTAÇÕES (USINGS) ===
// ========================================================================
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Interfaces;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Ecommerce.Application.Features.Pecas.Services;
using Ecommerce.Application.Features.Pecas.DTOs;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.Features.Auth.Services;
using Ecommerce.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Ecommerce.API.Swagger;
using Ecommerce.API.Middleware;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using Ecommerce.Application.Features.Auditoria.Services;
using Ecommerce.API.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

// --- USINGS PARA PONTO ---
using Ecommerce.Application.Features.Ponto.Services;
using Ecommerce.Application.Features.Ponto.DTOs;

// --- USINGS PARA PEDIDOS ---
using Ecommerce.Domain.Interfaces; 
using Ecommerce.Application.Features.Pedidos.Services;
using Ecommerce.Application.Features.Pedidos.DTOs;

// ========================================================================
// === CONFIGURAÇÃO INICIAL (BUILDER) ===
// ========================================================================
var builder = WebApplication.CreateBuilder(args);

// ========================================================================
// === REGISTRO DE SERVIÇOS ===
// ========================================================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "Bearer", BearerFormat = "JWT",
        In = ParameterLocation.Header, Description = "Insira o token JWT no formato: 'Bearer [SEU TOKEN]'"
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddHttpContextAccessor();

// Peças
builder.Services.AddScoped<IPecaRepository, PecaRepository>();
builder.Services.AddScoped<IPecaService, PecaService>();

// Auth
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Auditoria
builder.Services.AddScoped<ILogAuditoriaRepository, LogAuditoriaRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

// Ponto
builder.Services.AddScoped<IRegistroPontoRepository, RegistroPontoRepository>();
builder.Services.AddScoped<IPontoService, PontoService>();

// Pedidos
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();


// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        var issuer = builder.Configuration["Jwt:Issuer"] ?? "Ecommerce.API";
        var audience = builder.Configuration["Jwt:Audience"] ?? "Ecommerce.App";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
            ValidateIssuerSigningKey = true, ValidIssuer = issuer, ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOuFuncionario", policy => policy.RequireRole(ClaimTypes.Role, "Admin", "Funcionario"));
});

var app = builder.Build();

// ========================================================================
// === MIDDLEWARES ===
// ========================================================================

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// ========================================================================
// === ENDPOINTS ===
// ========================================================================

// --- PEÇAS ---
var pecasApi = app.MapGroup("/api/pecas");
pecasApi.MapPost("/", async ([FromBody] CriarPecaDto dto, IPecaService s) => { var p = await s.CriarPeca(dto); return Results.Created($"/api/pecas/{p.Id}", p); }).RequireAuthorization("AdminOuFuncionario");
pecasApi.MapGet("/", async (IPecaService s) => Results.Ok(await s.ObterTodasPecas()));
pecasApi.MapGet("/{id:guid}", async (Guid id, IPecaService s) => { var p = await s.ObterPecaPorId(id); return p is null ? Results.NotFound() : Results.Ok(p); });
pecasApi.MapPut("/{id:guid}", async (Guid id, [FromBody] AtualizarPecaDto dto, IPecaService s) => { await s.AtualizarPeca(id, dto); return Results.NoContent(); }).RequireAuthorization("AdminOuFuncionario");
pecasApi.MapDelete("/{id:guid}", async (Guid id, IPecaService s) => { await s.DeletarPeca(id); return Results.NoContent(); }).RequireAuthorization("AdminOuFuncionario");

// --- AUTH ---
var authApi = app.MapGroup("/api/auth");
authApi.MapPost("/registrar", async ([FromBody] RegistrarUsuarioDto dto, IAuthService s) => { var ok = await s.RegistrarAsync(dto); return ok ? Results.Ok("Sucesso") : Results.BadRequest("Erro ao registrar"); });
authApi.MapPost("/login", async ([FromBody] LoginUsuarioDto dto, IAuthService s) => { var res = await s.LoginAsync(dto); return res is null ? Results.Unauthorized() : Results.Ok(res); });

// --- PONTO ---
var pontoApi = app.MapGroup("/api/ponto").RequireAuthorization("AdminOuFuncionario");
pontoApi.MapPost("/entrada", async (IPontoService s) => Results.Ok(await s.RegistrarEntradaAsync()));
pontoApi.MapPost("/saida", async (IPontoService s) => Results.Ok(await s.RegistrarSaidaAsync()));

// --- PEDIDOS ---
var pedidosApi = app.MapGroup("/api/pedidos").RequireAuthorization();

// 1. Realizar Venda
pedidosApi.MapPost("/realizar-venda", async ([FromBody] RealizarPedidoDto dto, IPedidoService pedidoService, ClaimsPrincipal user) => 
{
    try 
    {
        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

        var usuarioId = Guid.Parse(userIdString);
        var pedido = await pedidoService.RegistrarVendaAsync(dto, usuarioId);

        return Results.Ok(new { 
            mensagem = "Venda realizada com sucesso!", 
            pedidoId = pedido.Id, 
            total = pedido.ValorTotal 
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
});

// 2. Histórico de Pedidos (NOVO)
pedidosApi.MapGet("/meus-pedidos", async (IPedidoService pedidoService, ClaimsPrincipal user) => 
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

    var usuarioId = Guid.Parse(userIdString);
    
    var pedidos = await pedidoService.ObterPedidosPorUsuarioAsync(usuarioId);

    return Results.Ok(pedidos);
});

app.Run();