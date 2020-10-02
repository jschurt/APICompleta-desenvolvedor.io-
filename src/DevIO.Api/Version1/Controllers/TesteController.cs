using DevIO.Api.Controllers;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace DevIO.Api.Version1.Controllers
{
    [ApiVersion("1.0", Deprecated = true)]
    [Route("api/v{version:apiVersion}/teste")]
    public class TesteController : MainAPIController
    {

        private readonly ILogger _logger;

        public TesteController(INotificador notificador,
                               IUser user,
                               ILogger<TesteController> logger) : base(notificador, user)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Valor() 
        {

            throw new DivideByZeroException();

            //Mensagens de desenvolvimento
            _logger.LogTrace("Log de Trace");
            _logger.LogDebug("Log de Debug");


            _logger.LogInformation("Log de Informacao");
            _logger.LogWarning("Log de Aviso");
            _logger.LogError("Log de Erro");
            _logger.LogCritical("Log de Problema Critico");

            return "sou a V1";
        } 

    }
    
}
