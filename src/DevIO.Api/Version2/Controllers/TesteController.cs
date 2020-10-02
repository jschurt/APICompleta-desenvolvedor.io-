using DevIO.Api.Controllers;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Version2.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/eu")]
    public class TesteController : MainAPIController
    {

        public TesteController(INotificador notificador,
                               IUser user) : base(notificador, user)
        { }

        [HttpGet]
        public string Valor()
        {
            return "sou a V2";
        }

    }
}
