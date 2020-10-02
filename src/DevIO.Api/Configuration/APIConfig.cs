using Microsoft.Extensions.Configuration;
using DevIO.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace DevIO.Api.Configuration
{
    /// <summary>
    /// Configuracoes da API
    /// </summary>
    public static class APIConfig
    {

        public static IServiceCollection WebApiConfig(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddControllers();

            //Aplicando versionamento da aplicacao.
            //Pacotes: Microsoft.AspNetCore.Versioning e Microsoft.AspNetCore.Versioning.ApiExplorer 
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            
            //Desabilitando formatacao e validacao de erros automaticos, que nao permite que eu faca a minha propria
            //validacao da ModelState. Da forma padrao, se existe um erro de modelstate (model invalida), minha action
            //sequer eh acionada, nao permitindo que eu trate o erro.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            //Adicionando policy CORS. Por default, eh restritiva. 
            //O CORS nao adiciona seguranca. Browsers implementam CORS mas outros aplicativos (por exemplo Postman) nao.
            services.AddCors(opt => {

                //Policy super permissiva, permitindo acesso de qualquer origem, qualquer metodo e qualquer header
                opt.AddPolicy("Development",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader(); ;
                    });

                //Policy exemplo com apenas algumas permissoes (o que nao estiver especificado sera restrito) 
                opt.AddPolicy("Production",
                    builder =>
                    {
                        builder
                            .WithMethods("GET", "POST")
                            .WithOrigins("http://desenvolvedor.io")
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            //.WithHeaders(HeaderNames.ContentType, "x-custom-header")
                            .AllowAnyHeader();
                    });


                //Policy padrao. Sera aplicada se nao for definido nenhum ambiente de execucao da aplicacao
                opt.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader(); ;
                    });


            });

            //Adicionando health checks (nativo .net core)
            //Eh possivel adicionar diversos health checks. (Mais info: github Xabaril AspNetCore.Diagnostics.HealthChecks)
            services.AddHealthChecks()
                .AddCheck(name: "Produtos", new SqlServerHealthCheckExtensions(configuration .GetConnectionString("DefaultConnection")))
                .AddSqlServer(connectionString: configuration.GetConnectionString("DefaultConnection"), name: "Banco SQL");

            //Adicionando health checks UI - Neste caso, estou utilizando a UI nativa. Eu posso criar a minha propria UI se eu quiser.
            //Pacote: AspNetCore.HealthChecks.UI
            services.AddHealthChecksUI()
                .AddInMemoryStorage();

            return services;

        } //WebApiConfig

        public static IApplicationBuilder UseMvcConfiguration(this IApplicationBuilder app)
        {

            //Se alguma chamada for feita via http, a aplicacao automaticamente redireciona para https
            app.UseHttpsRedirection();

            //Add Strict Transport Security Header (Seguranca). 
            //Este header informa ao browser que so conversa como https 
            //Porem, esta informacao apenas ocorre se a conexao ocorre via https. Iso eh, 
            //se a chamada a aplicacao for http, ela nao informara ao browser q conversa apenas https
            app.UseHsts();


            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //Adicionando middleware para health checks (ver tambem alteracoes em appsettings.json)
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/api/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/api/hc-ui";
                    options.ResourcesPath = "/api/hc-ui-resources";

                    options.UseRelativeApiPath = false;
                    options.UseRelativeResourcesPath = false;
                    options.UseRelativeWebhookPath = false;
                });

            });


            return app;
        } //UseMvcConfiguration


    }
}
