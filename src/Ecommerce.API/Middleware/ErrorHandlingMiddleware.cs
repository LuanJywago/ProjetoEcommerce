using System.Net;
using System.Text.Json;
using Ecommerce.Application.Exceptions; // Importa nossa exceção 400
using System; // Para Exception e Console
using Microsoft.AspNetCore.Http; // Para HttpContext
using System.Threading.Tasks; // Para Task

namespace Ecommerce.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        // Adicionamos o ILogger para logar o erro de forma mais "profissional"
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        // Modificamos o construtor para receber o ILogger
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Passamos o contexto e a exceção para o handler
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        // Modificamos o método para receber o ILogger
        private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ErrorHandlingMiddleware> logger)
        {
            var code = HttpStatusCode.InternalServerError; // 500 por padrão
            object result = new { error = "Ocorreu um erro inesperado no servidor." };

            switch (exception)
            {
                case ValidacaoException validacaoException:
                    code = HttpStatusCode.BadRequest; // 400
                    result = new { error = validacaoException.Erros };
                    break;
                case KeyNotFoundException:
                    code = HttpStatusCode.NotFound; // 404
                    result = new { error = "O recurso solicitado não foi encontrado." };
                    break;
                // NÃO precisamos de um 'default' aqui, pois o padrão já é 500
            }

            // --- A MUDANÇA IMPORTANTE ESTÁ AQUI ---
            // Se o erro NÃO for um dos que tratamos (400 ou 404),
            // logamos a exceção COMPLETA no console ANTES de enviar a resposta 500.
            if (code == HttpStatusCode.InternalServerError)
            {
                // Usamos o logger para registrar o erro
                // Isso vai imprimir a mensagem da exceção E o stack trace no seu terminal
                logger.LogError(exception, "Ocorreu uma exceção não tratada.");

                // Poderíamos até customizar a mensagem pro usuário em produção
                // result = new { error = "Ocorreu um erro interno. Tente novamente mais tarde." };
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}