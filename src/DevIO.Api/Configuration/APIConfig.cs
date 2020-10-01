using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace DevIO.Api.Configuration
{
    public static class APIConfig
    {

        public static IServiceCollection WebApiConfig(this IServiceCollection services)
        {

            services.AddControllers();

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


            return services;
        }

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

            return app;
        } //UseMvcConfiguration


    }
}
