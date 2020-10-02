using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DevIO.Api.Extensions
{

    /// <summary>
    /// Middle customizada para log de exceptions nao tratadas (utilizada em startup)
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(httpContext, ex);
            }
        }

        private void HandleExceptionAsync(HttpContext context, Exception exception)
        {
            //exception.Ship(context);
            _logger.LogError($"Meu erro: {exception.Message}");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}