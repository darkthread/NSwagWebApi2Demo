using Microsoft.Owin;
using NSwag.AspNet.Owin;
using Owin;
using System.Web.Http;
using NSwag;
using NSwag.SwaggerGeneration.Processors.Security;

[assembly: OwinStartup(typeof(WebApiDemo.Startup))]

namespace WebApiDemo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            app.UseSwaggerUi3(typeof(Startup).Assembly, settings =>
            {
                //針對RPC-Style WebAPI，指定路由包含Action名稱
                settings.GeneratorSettings.DefaultUrlTemplate = 
                    "api/{controller}/{action}/{id?}";
                //可加入客製化調整邏輯
                settings.PostProcess = document =>
                {
                    document.Info.Title = "WebAPI 範例";
                };
            });
            app.UseWebApi(config);
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();
        }
    }
}
