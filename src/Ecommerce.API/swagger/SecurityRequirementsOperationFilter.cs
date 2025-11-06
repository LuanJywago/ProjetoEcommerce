// Importações necessárias para o Filtro do Swagger
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ecommerce.API.Swagger
{
    // Esta classe vai "filtrar" as operações do Swagger
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 1. Verifica se o endpoint TEM alguma política de autorização (ex: ".RequireAuthorization(...)")
            var authAttributes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Distinct();

            // 2. Verifica se o "MapGroup" (ex: pecasApi) ou o endpoint em si tem a autorização
            var requiredRoles = context.ApiDescription
                .ActionDescriptor
                .EndpointMetadata
                .OfType<IAuthorizeData>();

            // 3. Se NÃO houver autorização (ex: GET /pecas ou POST /login), a gente para por aqui.
            if (!authAttributes.Any() && !requiredRoles.Any())
            {
                // Este endpoint é público, não faz nada.
                return;
            }

            // 4. Se CHEGOU AQUI, o endpoint é protegido!
            // Adicionamos a exigência de "Bearer" (Token) a ele

            // Garante que a lista de "Tags" (respostas) exista
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Não autorizado (Unauthorized)" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Acesso negado (Forbidden)" });

            // Define o esquema de segurança (o cadeado)
            var bearerScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Deve bater com o nome que demos em AddSecurityDefinition
                }
            };

            // Adiciona o cadeado a ESTE endpoint
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [bearerScheme] = new List<string>()
                }
            };
        }
    }
}