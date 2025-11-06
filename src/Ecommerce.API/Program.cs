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
using Microsoft.OpenApi.Models; // Para o Swagger
using Ecommerce.API.Swagger; // Para o nosso Filtro
using Ecommerce.API.Middleware; // Para o Tratamento de Erro
using System; // Para Guid, DateTime, Console, ArgumentNullException
using Microsoft.Extensions.Logging; // Para ILogger
using Microsoft.AspNetCore.Authentication; // Para AuthenticationFailedContext
using System.Threading.Tasks; // Para Task
using Ecommerce.Application.Features.Auditoria.Services; // Para Auditoria
using Ecommerce.API.Services; // Para CurrentUserService
using Microsoft.AspNetCore.Http; // Para AddHttpContextAccessor
using System.Linq; // Para Select nas claims do log JWT
using System.IdentityModel.Tokens.Jwt; // Para JwtSecurityTokenHandler na depuração
using Microsoft.Extensions.DependencyInjection; // Para GetRequiredService na depuração
using Microsoft.Extensions.Configuration; // Para IConfiguration na depuração
// --- USINGS PARA PONTO ---
using Ecommerce.Application.Features.Ponto.Services;
using Ecommerce.Application.Features.Ponto.DTOs;

// ========================================================================
// === CONFIGURAÇÃO INICIAL (BUILDER) ===
// ========================================================================

// --- MUDANÇA CORS (1 de 3): Damos um nome para a nossa política ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// ========================================================================
// === REGISTRO DE SERVIÇOS (INJEÇÃO DE DEPENDÊNCIA) ===
// ========================================================================

// --- MUDANÇA CORS (2 de 3): Adicionamos o serviço de CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          // Permite que o nosso frontend (http://localhost:5013)
                          // faça requisições para esta API.
                          policy.WithOrigins("http://localhost:5013") 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// --- 1. Banco de Dados (Entity Framework + MySQL) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// --- 2. Swagger / OpenAPI (Documentação da API) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define o esquema de segurança "Bearer" para o botão Authorize
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: 'Bearer [SEU TOKEN]'"
    });
    // Adiciona o filtro para mostrar cadeados 🔒 apenas nos endpoints protegidos
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// --- Serviço Essencial para ICurrentUserService ---
// Disponibiliza o HttpContext atual para injeção de dependência
builder.Services.AddHttpContextAccessor();

// --- 3. Serviços de Peças (Repositório e Lógica) ---
builder.Services.AddScoped<IPecaRepository, PecaRepository>();
builder.Services.AddScoped<IPecaService, PecaService>();

// --- 4. Serviços de Autenticação (Repositório, Lógica e Token) ---
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// --- 5. Serviços de Auditoria ---
builder.Services.AddScoped<ILogAuditoriaRepository, LogAuditoriaRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); // Implementação da API
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

// --- 6. Serviços de Ponto ---
builder.Services.AddScoped<IRegistroPontoRepository, RegistroPontoRepository>();
builder.Services.AddScoped<IPontoService, PontoService>();

// --- 7. Configuração da Autenticação JWT (Validação do Token com Logs de Eventos) ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Leitura segura da chave, issuer e audience do appsettings.json
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
             throw new ArgumentNullException(nameof(jwtKey), "A chave secreta JWT ('Jwt:Key') não foi encontrada ou está vazia no appsettings.json para a validação.");
        }
        var issuer = builder.Configuration["Jwt:Issuer"] ?? "Ecommerce.API";
        var audience = builder.Configuration["Jwt:Audience"] ?? "Ecommerce.App";

        // Logs de Depuração (Para verificar a leitura na inicialização)
        Console.WriteLine("--- VALIDAÇÃO JWT (Configuração) ---");
        Console.WriteLine($"[Program.cs] Lendo Jwt:Key: '{jwtKey}'");
        Console.WriteLine($"[Program.cs] Lendo Jwt:Issuer: '{issuer}'");
        Console.WriteLine($"[Program.cs] Lendo Jwt:Audience: '{audience}'");
        Console.WriteLine("-----------------------------------");

        // Define os parâmetros para validar um token recebido
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Adiciona logs detalhados para falhas ou sucesso na validação em runtime
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("--- FALHA NA AUTENTICAÇÃO JWT ---");
                Console.WriteLine($"Exceção: {context.Exception.GetType().FullName}");
                Console.WriteLine($"Mensagem: {context.Exception.Message}");
                Console.WriteLine("----------------------------------");
                Console.WriteLine($"Cabeçalho Auth Recebido: {context.Request.Headers["Authorization"]}");
                Console.WriteLine("----------------------------------");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("--- TOKEN JWT VALIDADO COM SUCESSO ---");
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
                Console.WriteLine($"Claims no Token Validado: {string.Join(" | ", claims ?? Enumerable.Empty<string>())}");
                Console.WriteLine("--------------------------------------");
                return Task.CompletedTask;
            }
        };
    });

// --- 8. Configuração da Autorização (Definição de Políticas) ---
builder.Services.AddAuthorization(options =>
{
    // Define a política "AdminOuFuncionario" que exige o cargo (Role) correspondente
    options.AddPolicy("AdminOuFuncionario", policy =>
        policy.RequireRole(ClaimTypes.Role, "Admin", "Funcionario"));
});


// --- MUDANÇA (PASSO 32.0): Força o C# a enviar JSON em camelCase ---
// (Isso corrige o erro 'TypeError: ... reading 'nome'' no app.js)
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
// ========================================================================
// ========================================================================
// === CONSTRUÇÃO DO APLICATIVO (APP) ===
// ========================================================================
var app = builder.Build();

// ========================================================================
// === CONFIGURAÇÃO DO PIPELINE HTTP (MIDDLEWARES) ===
// ========================================================================

// --- Middleware de Tratamento de Erro (Deve ser o primeiro) ---
app.UseMiddleware<ErrorHandlingMiddleware>();

// --- Swagger (Apenas em Desenvolvimento) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Interface gráfica do Swagger
}

// --- Arquivos Estáticos (Frontend) ---
app.UseDefaultFiles(); // Procura por index.html na wwwroot
app.UseStaticFiles();  // Serve arquivos da wwwroot (css, js)

// --- Redirecionamento HTTPS ---
// app.UseHttpsRedirection(); // Comentado temporariamente

// --- MUDANÇA CORS (3 de 3): Mandamos o app USAR a política ---
// (Deve vir ANTES de UseAuthentication/UseAuthorization)
app.UseCors(MyAllowSpecificOrigins);

// --- Autenticação e Autorização (Ordem Importante!) ---
app.UseAuthentication(); // 1º: Identifica o usuário (lê o token)
app.UseAuthorization();  // 2º: Verifica as permissões (checa o cargo/política)

// ========================================================================
// === DEFINIÇÃO DOS ENDPOINTS DA API (IMPLEMENTAÇÃO COMPLETA) ===
// ========================================================================

// --- Grupo de Endpoints de Peças ---
var pecasApi = app.MapGroup("/api/pecas");
// (Endpoints de Peças... MapPost, MapGet, MapPut, MapDelete)
pecasApi.MapPost("/", async ([FromBody] CriarPecaDto dto, IPecaService pecaService) =>
{ var peca = await pecaService.CriarPeca(dto); return Results.Created($"/api/pecas/{peca.Id}", peca); })
.RequireAuthorization("AdminOuFuncionario");
pecasApi.MapGet("/", async (IPecaService pecaService) =>
{ var pecas = await pecaService.ObterTodasPecas(); return Results.Ok(pecas); });
pecasApi.MapGet("/{id:guid}", async ([FromRoute] Guid id, IPecaService pecaService) =>
{ var peca = await pecaService.ObterPecaPorId(id); if (peca == null) { return Results.NotFound("Peça não encontrada."); } return Results.Ok(peca); });
pecasApi.MapPut("/{id:guid}", async ([FromRoute] Guid id, [FromBody] AtualizarPecaDto dto, IPecaService pecaService) =>
{ await pecaService.AtualizarPeca(id, dto); return Results.NoContent(); })
.RequireAuthorization("AdminOuFuncionario");
pecasApi.MapDelete("/{id:guid}", async ([FromRoute] Guid id, IPecaService pecaService) =>
{ await pecaService.DeletarPeca(id); return Results.NoContent(); })
.RequireAuthorization("AdminOuFuncionario");


// --- Grupo de Endpoints de Autenticação ---
var authApi = app.MapGroup("/api/auth");

// POST /api/auth/registrar (Registrar Usuário) - Público
authApi.MapPost("/registrar", async ([FromBody] RegistrarUsuarioDto dto, IAuthService authService) =>
{
    var sucesso = await authService.RegistrarAsync(dto);
    if (!sucesso) { return Results.BadRequest("Não foi possível registrar. O e-mail pode já estar em uso."); }
    return Results.Ok("Usuário registrado com sucesso.");
});

// POST /api/auth/login (Passo 1 do Login) - Público
authApi.MapPost("/login", async ([FromBody] LoginUsuarioDto dto, IAuthService authService) =>
{
    // Agora chama o LoginAsync que retorna o LoginPasso1ResponseDto
    var resposta = await authService.LoginAsync(dto);
    if (resposta == null)
    {
        return Results.Unauthorized(); // Senha ou usuário errado
    }
    // Retorna 200 OK com o DTO (que diz "A2FRequerido = true" e o código simulado)
    return Results.Ok(resposta); 
});

// POST /api/auth/validar-a2f (Passo 2 do Login) - Público
authApi.MapPost("/validar-a2f", async ([FromBody] ValidarA2FDto dto, IAuthService authService) =>
{
    // Chama o novo serviço para validar o código
    var resposta = await authService.ValidarA2FAsync(dto);
    if (resposta == null)
    {
        // Código A2F errado ou expirado
        return Results.BadRequest("Código A2F inválido ou expirado.");
    }
    // Sucesso! Retorna o token JWT final.
    return Results.Ok(resposta); 
});


// --- Grupo de Endpoints de Ponto ---
var pontoApi = app.MapGroup("/api/ponto")
                 .RequireAuthorization("AdminOuFuncionario"); // Protege TODO o grupo
// (Endpoints de Ponto... MapPost /entrada, MapPost /saida)
pontoApi.MapPost("/entrada", async (IPontoService pontoService) =>
{ var registro = await pontoService.RegistrarEntradaAsync(); return Results.Ok(registro); });
pontoApi.MapPost("/saida", async (IPontoService pontoService) =>
{ var registro = await pontoService.RegistrarSaidaAsync(); return Results.Ok(registro); });


// --- Endpoint de Debug (Opcional, pode remover se não precisar mais) ---
app.MapPost("/api/debug/validate-token", ([FromBody] string token, IConfiguration config) =>
{
    // (Implementação completa do endpoint de debug)
    Console.WriteLine("\n--- INICIANDO VALIDAÇÃO MANUAL DO TOKEN ---");
    var tokenHandler = new JwtSecurityTokenHandler();
    var jwtKey = config["Jwt:Key"];
    var issuer = config["Jwt:Issuer"] ?? "Ecommerce.API";
    var audience = config["Jwt:Audience"] ?? "Ecommerce.App";
    if (string.IsNullOrEmpty(jwtKey)) { Console.WriteLine("ERRO FATAL: Chave JWT..."); return Results.Problem("..."); }
    var validationParameters = new TokenValidationParameters { /*...*/
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ValidateIssuerSigningKey = true, ValidIssuer = issuer, ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
    Console.WriteLine($"[Debug] Usando Chave: '{jwtKey}'");
    Console.WriteLine($"[Debug] Usando Issuer: '{issuer}'");
    Console.WriteLine($"[Debug] Usando Audience: '{audience}'");
    Console.WriteLine($"[Debug] Token recebido: {token?.Substring(0, Math.Min(token?.Length ?? 0, 10))}...");
    try
    {
        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        Console.WriteLine("[Debug] VALIDAÇÃO MANUAL BEM SUCEDIDA!");
        var claims = principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
        Console.WriteLine($"[Debug] Claims: {string.Join(" | ", claims ?? Enumerable.Empty<string>())}");
        Console.WriteLine("--- FIM VALIDAÇÃO MANUAL ---\n");
        return Results.Ok(new { message = "Token válido!", claims = claims });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Debug] ERRO NA VALIDAÇÃO MANUAL: {ex.GetType().FullName}");
        Console.WriteLine($"[Debug] Mensagem: {ex.Message}");
        Console.WriteLine("--- FIM VALIDAÇÃO MANUAL ---\n");
        return Results.BadRequest(new { error = ex.Message });
    }
});

// ========================================================================
// === EXECUÇÃO DO APLICATIVO ===
// ========================================================================
app.Run(); // Inicia o servidor web