using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Configuration.Swagger
{

    /// <summary>
    /// Extendendo opcao de configuracao swagger
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

        /// <summary>
        /// Pega todas as versoes da minha API e cria um doc para cada versao
        /// </summary>
        /// <param name="options"></param>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                OpenApiInfo apiInfo = CreateInfoForApiVersion(description);
                options.SwaggerDoc(description.GroupName, apiInfo);
            }
        }

        /// <summary>
        /// Criando documentacao minima para a minha API
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "My API",
                Version = description.ApiVersion.ToString(),
                Description = "Esta API faz parte do curso REST com ASP.NET Core WebAPI.",
                Contact = new OpenApiContact() { Name = "Julio Schurt", Email = "julio@email.com" },
                TermsOfService = new Uri("https://opensource.org/licenses/MIT"),
                License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
            };

            if (description.IsDeprecated)
            {
                info.Description += " (Esta versão está obsoleta!)";
            }

            return info;
        }
    } //class
} //namespace
