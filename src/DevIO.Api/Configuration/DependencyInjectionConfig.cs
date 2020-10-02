using DevIO.Api.Configuration.Swagger;
using DevIO.Api.Extensions;
using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using DevIO.Business.Services;
using DevIO.Data.Context;
using DevIO.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DevIO.Api.Configuration
{
    public static class DependencyInjectionConfig
    {

        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {

            services.AddScoped<MeuDbContext>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            services.AddScoped<IFornecedorRepository, FornecedorRepository>();
            services.AddScoped<IFornecedorService, FornecedorService>();
            
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IProdutoRepository, ProdutoRepository>();
            services.AddScoped<IProdutoService, ProdutoService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUser, AspNetUser>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;

        } //ResolveDependencies

    } //class
} //namespace
