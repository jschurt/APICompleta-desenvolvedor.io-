using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

namespace DevIO.Api.Extensions
{

    /// <summary>
    /// Classe utilizada para trabalhar de uma forma mais facil com as Claims
    /// </summary>
    public class CustomAuthorization
    {
        public static bool ValidarClaimsUsuario(HttpContext context, string claimName, string claimValue)
        {
            return context.User.Identity.IsAuthenticated &&
                   context.User.Claims.Any(c => c.Type == claimName && c.Value.Contains(claimValue));
        }

    } //class

    public class ClaimsAuthorizeAttribute : TypeFilterAttribute
    {
        public ClaimsAuthorizeAttribute(string claimName, string claimValue) : base(typeof(RequisitoClaimFilter))
        {
            Arguments = new object[] { new Claim(claimName, claimValue) };
        }
    }

    public class RequisitoClaimFilter : IAuthorizationFilter
    {
        private readonly Claim _claim;

        public RequisitoClaimFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new StatusCodeResult(401);
                return;
            }

            if (!CustomAuthorization.ValidarClaimsUsuario(context.HttpContext, _claim.Type, _claim.Value))
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    
    } //class


    #region teste Filters ==================

    public class TestAuthorizeAttribute : TypeFilterAttribute
    {
        public TestAuthorizeAttribute() : base(typeof(testeFilter))
        {
        }
    }


    public class testeFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //pegando o token completo
            var token = context.HttpContext.GetTokenAsync("Bearer", "access_token");

            //pegando o usuario (email) do token
            var email = context.HttpContext.User.FindFirst("sub")?.Value;

            //pegando o id do usuario
            string userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //context.Result = new StatusCodeResult(401);

        }

    }

    #endregion

} //namespace