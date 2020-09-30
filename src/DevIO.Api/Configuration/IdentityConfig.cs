using DevIO.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text;
using Microsoft.IdentityModel.Tokens;
using DevIO.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DevIO.Api.Configuration
{
    public static class IdentityConfig
    {

        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            //Config context udentity
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            //Config identity (necessario instalar o pacote Microsoft.AspNetCore.Identity.UI)
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<IdentityMessagesPortugues>();

            #region === JWT =======================================================================

            //Carregando valores de appsettings.json e disponibilizando para a aplicacao
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<MyAppSettings>(appSettingsSection);

            //Lendo valores de configuracao carregados
            MyAppSettings appSettings = appSettingsSection.Get<MyAppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            //Indicando que autenticacao se dara via token
            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtConfig =>
            {
                //Disable just in development. Se true, forca que o cliente venha de https
                jwtConfig.RequireHttpsMetadata = false;
                jwtConfig.SaveToken = true;
                jwtConfig.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor
                };
            });

            #endregion

            return services;

        } //AddIdentityConfiguration

    } //IdentityConfig
} //namespace
