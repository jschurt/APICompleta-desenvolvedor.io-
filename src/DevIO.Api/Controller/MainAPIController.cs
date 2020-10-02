using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevIO.Api.Controllers
{
    /// <summary>
    /// Classe abstrata, nao pode ser instanciada, apenas herdada
    /// </summary>
    [ApiController]
    public abstract class MainAPIController: ControllerBase
    {

        private readonly INotificador _notificador;
        public readonly IUser _appUser;

        protected Guid UsuarioId { get; set; }
        protected bool UsuarioAutenticado { get; set; }

        protected MainAPIController(INotificador notificador,
                                    IUser appUser)
        {
            _notificador = notificador ?? throw new System.ArgumentNullException(nameof(notificador));
            _appUser = appUser ?? throw new System.ArgumentNullException(nameof(appUser));

            if (_appUser.IsAuthenticated()) {
                UsuarioId = _appUser.GetUserId();
                UsuarioAutenticado = true;
            }

        }

        //validacao de notificacoes

        //validacao de ModelState

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao();
        } //OperacaoValida

        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            else
            {
                return BadRequest(new
                {
                    sucess = false,
                    errors = _notificador.ObterNotificacoes().Select(m => m.Mensagem)
                });
            }

        } //CustomResponse

        protected ActionResult CustomResponse(ModelStateDictionary modelState) 
        {

            if (!ModelState.IsValid)
                NotificarErroModelInvalida(modelState);

            return CustomResponse();

        } //CustomResponse

        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {
            var x = modelState.Values.Select(e => e.Errors);
            IEnumerable<ModelError> todosErros = modelState.Values.SelectMany(e => e.Errors);

            foreach (ModelError erro in todosErros)
            {
                var errorMessage = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;

                NotificarErro(errorMessage);

            }


        } //NotificarErroModelInvalida

        protected void NotificarErro(string errorMsg)
        {

            Notificacao notificacaoErro = new Notificacao(errorMsg);

            _notificador.Handle(notificacaoErro);

        } //NotificarErro

        //validacao da operacao de negocios

    } //class

} //namespace
