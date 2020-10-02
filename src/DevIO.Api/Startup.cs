using AutoMapper;
using DevIO.Api.Configuration;
using DevIO.Api.Configuration.Swagger;
using DevIO.Api.Extensions;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevIO.Api
{
    public class Startup
    {

        private IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<MeuDbContext>(options =>
            {
                options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddAutoMapper(typeof(Startup));

            //Extension Method que concentra informacoes do Identity (seguranca)
            services.AddIdentityConfiguration(_configuration);

            //Extension Method que concentra todas as configuracoes de servico da API
            services.WebApiConfig(_configuration);

            //Adicionando swagger, para documentacao da API
            //Pacote: Swashbucle.AspNetCore
            //AddSwaggerConfig e' um metodo de extensao que criamos na classe SwaggerConfig.
            services.AddSwaggerConfig();

            //ResolveDependencies e' um metodo de extensao que criamos na classe DependencyInjectionConfig.
            services.ResolveDependencies();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("Development");
            }
            else
            {
                app.UseCors("Production");
            }

            //Extension method para capturar exceptions nao tratadas (podemos criar um log por exemplo)
            app.UseMiddleware<ExceptionMiddleware>();

            //Extension Method que concentra configuracoes de middleware do swagger (para documentacao)
            app.UseSwaggerConfig(provider);

            //Precisa SEMPRE VIR ANTES que o UseMvcConfiguration
            app.UseAuthentication();

            //Extension Method que concentra configuracoes de middleware da API
            app.UseMvcConfiguration();

        }
    }
}
