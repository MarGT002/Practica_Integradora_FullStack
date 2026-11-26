using System.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;

namespace apiRESTReservacionesFull
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // -----------------------------------------------------
            // CORS
            // NuGet Package Manager -> Package Manager Console:
            //   Install-Package Microsoft.AspNet.WebApi.Cors -Version 5.3.0
            //
            // Los orígenes permitidos se configuran en Web.config
            // (llave "corsOrigenes", separados por coma). Se necesita
            // para que la consola PUSH del sitio (EventSource) pueda
            // conectarse al canal /api/stream cuando el sitio y la API
            // corren en puertos distintos (Visual Studio local).
            // -----------------------------------------------------
            string origenes = ConfigurationManager.AppSettings["corsOrigenes"];
            if (string.IsNullOrWhiteSpace(origenes))
            {
                origenes = "*";
            }

            EnableCorsAttribute cors = new EnableCorsAttribute(origenes, "*", "GET,POST,PUT,OPTIONS");
            config.EnableCors(cors);

            // Respuestas siempre en JSON (sin XML)
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Web API routes (rutas por atributo: [RoutePrefix] y [Route])
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
