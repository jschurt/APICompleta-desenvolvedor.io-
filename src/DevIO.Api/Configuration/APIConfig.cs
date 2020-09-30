using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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

            //Adicionando suporte CORS
            services.AddCors(opt => {
                opt.AddPolicy("Development",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });


            return services;
        }

        public static IApplicationBuilder UseMvcConfiguration(this IApplicationBuilder app)
        {

            app.UseCors("Development");
            app.UseHttpsRedirection();
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
