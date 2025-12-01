using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Ecommerce.API.Middleware;
using Ecommerce.API.Services;
using Ecommerce.API.Swagger;
using Ecommerce.Application.Features.Auditoria.Services;
using Ecommerce.Application.Features.Auth.DTOs;
using Ecommerce.Application.Features.Auth.Services;
using Ecommerce.Application.Features.Pecas.DTOs;
using Ecommerce.Application.Features.Pecas.Services;
using Ecommerce.Application.Features.Pedidos.DTOs;
using Ecommerce.Application.Features.Pedidos.Services;
using Ecommerce.Application.Features.Ponto.Services;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ========================================================================
// === CONFIGURAÇÃO DE JSON (CORREÇÃO ENUM) ===
// ========================================================================
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ========================================================================
// === BANCO DE DADOS E SWAGGER ===
// ========================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
); //Conecta o EF ao MySQL pelo AppDbContext

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
//Configura o Swagger para aceitar o cadeado de login (Bearer Token)
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: 'Bearer [SEU TOKEN]'"
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddHttpContextAccessor();

// ========================================================================
// === INJEÇÃO DE DEPENDÊNCIA (IOC) ===
// ========================================================================

//ADD SCOPED Diz que, toda vez que alguem pedir o IPecaRepository, entregue uma nova instancia de PecaRepository, tipo "ZERA" ELA durante o Http (scoped)
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

// ========================================================================
// === AUTENTICAÇÃO E AUTORIZAÇÃO ===
// ========================================================================

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => //Ensina a ler o token
    //Bearer é ensinado a validar a assinatura usando a chave secreta
    {
        var jwtKey = builder.Configuration["Jwt:Key"]; //Chave secreta (appsetings.json)
        var issuer = builder.Configuration["Jwt:Issuer"] ?? "Ecommerce.API";
        var audience = builder.Configuration["Jwt:Audience"] ?? "Ecommerce.App";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

// Políticas de Acesso
builder.Services.AddAuthorization(options =>
{   //Criando as regras - Define quem pode entrar onde
    // Staff: Só Admin e Funcionário podem mexer no Estoque e Ponto
    // Política Staff so deixa passar se no token estiver com a Role correta (função)
    options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Funcionario"));

    // AdminOnly: Só Admin vê o relatório financeiro
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    // Comprador: Qualquer um logado (incluindo Cliente) pode comprar
    options.AddPolicy("Comprador", policy => policy.RequireAuthenticatedUser());
});

var app = builder.Build();

// ========================================================================
// === MIDDLEWARES PIPELINE ===
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
// === ENDPOINTS (MINIMAL APIs) ===
// ========================================================================

// --- PEÇAS (Protegido por "Staff") ---
var pecasApi = app.MapGroup("/api/pecas");

pecasApi.MapPost("/", async ([FromBody] CriarPecaDto dto, IPecaService service) => 
{ 
    var peca = await service.CriarPeca(dto); 
    return Results.Created($"/api/pecas/{peca.Id}", peca); 
})
.RequireAuthorization("Staff");

// Público para ver vitrine
pecasApi.MapGet("/", async (IPecaService service) => 
    Results.Ok(await service.ObterTodasPecas())); 

pecasApi.MapGet("/{id:guid}", async (Guid id, IPecaService service) => 
{ 
    var peca = await service.ObterPecaPorId(id); 
    return peca is null ? Results.NotFound() : Results.Ok(peca); 
});

pecasApi.MapPut("/{id:guid}", async (Guid id, [FromBody] AtualizarPecaDto dto, IPecaService service) => 
{ 
    await service.AtualizarPeca(id, dto); 
    return Results.NoContent(); 
})
.RequireAuthorization("Staff");

pecasApi.MapDelete("/{id:guid}", async (Guid id, IPecaService service) => 
{ 
    await service.DeletarPeca(id); 
    return Results.NoContent(); 
})
.RequireAuthorization("Staff");


// --- AUTH ---
var authApi = app.MapGroup("/api/auth");

authApi.MapPost("/registrar", async ([FromBody] RegistrarUsuarioDto dto, IAuthService authService) => 
{ 
    var sucesso = await authService.RegistrarAsync(dto); 
    return sucesso ? Results.Ok("Sucesso") : Results.BadRequest("Erro ao registrar"); 
});

authApi.MapPost("/login", async ([FromBody] LoginUsuarioDto dto, IAuthService authService) => 
{ 
    var response = await authService.LoginAsync(dto); 
    return response is null ? Results.Unauthorized() : Results.Ok(response); 
});


// --- PONTO (Protegido por "Staff") ---
var pontoApi = app.MapGroup("/api/ponto").RequireAuthorization("Staff");

pontoApi.MapPost("/entrada", async (IPontoService pontoService) => 
    Results.Ok(await pontoService.RegistrarEntradaAsync()));

pontoApi.MapPost("/saida", async (IPontoService pontoService) => 
    Results.Ok(await pontoService.RegistrarSaidaAsync()));


// --- PEDIDOS ---
var pedidosApi = app.MapGroup("/api/pedidos").RequireAuthorization("Comprador");

// 1. Venda (Todos logados)
pedidosApi.MapPost("/realizar-venda", async ([FromBody] RealizarPedidoDto dto, IPedidoService pedidoService, ClaimsPrincipal user) =>
{
    try
    {
        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

        var usuarioId = Guid.Parse(userIdString);
        var pedido = await pedidoService.RegistrarVendaAsync(dto, usuarioId);

        return Results.Ok(new
        {
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

// 2. Histórico de Pedidos (Todos logados)
pedidosApi.MapGet("/meus-pedidos", async (IPedidoService pedidoService, ClaimsPrincipal user) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

    var usuarioId = Guid.Parse(userIdString);

    var pedidos = await pedidoService.ObterPedidosPorUsuarioAsync(usuarioId);

    return Results.Ok(pedidos);
});

// 3. Relatório (SÓ ADMIN)
pedidosApi.MapGet("/relatorio-admin", async (IPedidoService pedidoService) =>
{
    return Results.Ok(await pedidoService.GerarRelatorioAdminAsync());
})
.RequireAuthorization("AdminOnly");

app.Run();