using DevIO.Api.Controllers;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.Api.Version1.Controllers
{

    //Posso ter a mesma controller suportando mais de uma versao
    //[ApiVersion("2.0")]
    //[ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class AuthController : MainAPIController
    {

        //Responsavel pela autenticacao do usuario
        private readonly SignInManager<IdentityUser> _signInManager;

        //Responsavel por criar usuario
        private readonly UserManager<IdentityUser> _userManager;

        //Contem propriedades de configuracao do meu JWT
        private readonly MyAppSettings _myAppSettings;

        //Log da aplicacao
        private readonly ILogger _logger;


        public AuthController(INotificador notificador,
                                SignInManager<IdentityUser> signInManager,
                                UserManager<IdentityUser> userManager,
                                IOptions<MyAppSettings> myAppSettings,
                                IUser user,
                                ILogger<AuthController> logger) : base(notificador, user)
        {

            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _myAppSettings = myAppSettings.Value ?? throw new ArgumentNullException(nameof(myAppSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Registra (cadastra) novo usuario
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(RegisterUserViewModel model)
        {

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return CustomResponse(await GerarJwt(user.Email));
            }
            else
            {
                foreach (var error in result.Errors)
                    NotificarErro(error.Description);
            }

            return CustomResponse(model);

        } //Registrar

        /// <summary>
        /// Login usuario
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Se sucesso, retorna o token</returns>
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginUserViewModel model)
        {

            if (!ModelState.IsValid)
                return CustomResponse(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);

            if (result.Succeeded)
            {
                _logger.LogInformation(message: $"Usario {model.Email} logado com sucesso.");
                return CustomResponse(await GerarJwt(model.Email));
            }

            if (result.IsLockedOut)
            {
                NotificarErro("Usuario temporariamente bloqueado por tentativas invalidas.");
                return CustomResponse(model);
            }

            NotificarErro("Login error");
            return CustomResponse(model);

        } //Login

        /// <summary>
        /// Gera Jason Web Token (JWT)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<LoginResponseViewModel> GerarJwt(string email)
        {

            //Pegando dados do usuario (user, claims, roles)
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            //Alem das claims do usuario, quero adicionar as claims do token...
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            //Identificacao do token
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            //Not value before
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            //Data/hora quando o token foi emitido
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            //Roles tb sao reconhecidas como claims
            foreach (var userRole in roles)
            {
                claims.Add(new Claim("role", userRole));
            }

            //Convertendo para identity claims
            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            byte[] key = Encoding.ASCII.GetBytes(_myAppSettings.Secret);
            SymmetricSecurityKey ssKey = new SymmetricSecurityKey(key);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _myAppSettings.Emissor,
                Audience = _myAppSettings.ValidoEm,
                Expires = DateTime.UtcNow.AddHours(_myAppSettings.ExpiracaoHora),
                SigningCredentials = new SigningCredentials(ssKey, SecurityAlgorithms.HmacSha256Signature),
                Subject = identityClaims
            });

            var encodedHandler = tokenHandler.WriteToken(token);

            var response = new LoginResponseViewModel
            {
                AccessToken = encodedHandler,
                ExpiresIn = TimeSpan.FromHours(_myAppSettings.ExpiracaoHora).TotalSeconds,
                UserToken = new UserTokenViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value })
                }
            };

            return response;

        } //GerarJwt

        /// <summary>
        /// Gerando Unix Epoch Date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);


    } //class
} //namespace
