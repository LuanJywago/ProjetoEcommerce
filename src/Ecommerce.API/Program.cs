// Importações necessárias (usings)
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Interfaces;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Ecommerce.Application.Features.Pecas.Services;

// USINGS NOVOS (para os DTOs e para os endpoints)
using Ecommerce.Application.Features.Pecas.DTOs;
using Microsoft.AspNetCore.Mvc; // Para o [FromBody] e [FromRoute]

var builder = WebApplication.CreateBuilder(args);

// --- Início da Seção de Serviços ---
// (Todo o seu código de AddDbContext, AddSwaggerGen, AddScoped, etc. continua aqui)

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

builder.Services.AddScoped<IPecaRepository, PecaRepository>();
builder.Services.AddScoped<IPecaService, PecaService>();

// --- Fim da Seção de Serviços ---

var app = builder.Build();

// --- Início do Pipeline HTTP ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}
app.UseHttpsRedirection();
// --- Fim do Pipeline HTTP ---


// ========================================================================
// === PASSO 7: ADICIONE O BLOCO DE ENDPOINTS DE API AQUI ===
// ========================================================================

// Agrupa todos os endpoints de peças sob o prefixo "/api/pecas"
var pecasApi = app.MapGroup("/api/pecas");

// --- RF05: Cadastro Peças (Create) ---
// Rota: POST /api/pecas
pecasApi.MapPost("/", async ([FromBody] CriarPecaDto dto, IPecaService pecaService) =>
{
    var peca = await pecaService.CriarPeca(dto);
    if (peca == null)
    {
        // Retorna um erro 400 (Bad Request) se a validação falhar (ex: preço 0)
        return Results.BadRequest("Dados inválidos. Verifique o preço e o estoque.");
    }
    
    // Retorna 201 (Created) com a peça criada
    return Results.Created($"/api/pecas/{peca.Id}", peca);
});

// --- RQ04: Consulta das peças (Read All) ---
// Rota: GET /api/pecas
pecasApi.MapGet("/", async (IPecaService pecaService) =>
{
    var pecas = await pecaService.ObterTodasPecas();
    return Results.Ok(pecas);
});

// --- RQ04: Consulta das peças (Read by ID) ---
// Rota: GET /api/pecas/{id}
pecasApi.MapGet("/{id:guid}", async ([FromRoute] Guid id, IPecaService pecaService) =>
{
    var peca = await pecaService.ObterPecaPorId(id);
    if (peca == null)
    {
        return Results.NotFound("Peça não encontrada.");
    }
    return Results.Ok(peca);
});

// --- RF08/RQ02: Edição/Atualização de peças (Update) ---
// Rota: PUT /api/pecas/{id}
pecasApi.MapPut("/{id:guid}", async ([FromRoute] Guid id, [FromBody] AtualizarPecaDto dto, IPecaService pecaService) =>
{
    var sucesso = await pecaService.AtualizarPeca(id, dto);
    if (!sucesso)
    {
        return Results.NotFound("Peça não encontrada ou dados inválidos.");
    }
    
    // Retorna 204 (No Content) - sucesso, mas sem corpo de resposta
    return Results.NoContent();
});

// --- RQ03: Exclusão de peças (Delete) ---
// Rota: DELETE /api/pecas/{id}
pecasApi.MapDelete("/{id:guid}", async ([FromRoute] Guid id, IPecaService pecaService) =>
{
    var sucesso = await pecaService.DeletarPeca(id);
    if (!sucesso)
    {
        return Results.NotFound("Peça não encontrada.");
    }
    return Results.NoContent();
});

// ========================================================================
// === FIM DO BLOCO DE ENDPOINTS ===
// ========================================================================


app.Run(); // Esta deve ser a ÚLTIMA linha do arquivo