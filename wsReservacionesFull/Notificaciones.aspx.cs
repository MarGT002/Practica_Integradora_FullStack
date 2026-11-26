using System;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using wsReservacionesFull.Clases;

namespace wsReservacionesFull
{
    public partial class Notificaciones : clsPaginaProtegida
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // El canal SSE se abre desde JavaScript (js/push.js) con EventSource,
            // directamente contra la Web API. Aquí no se ejecutan endpoints.
        }

        // Usuario de la sesión (los WebMethod son estáticos)
        private static string UsuarioSesion()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null || ctx.Session["usuario"] == null)
                return null;
            return ctx.Session["usuario"].ToString();
        }

        private static string SinSesion()
        {
            return JsonConvert.SerializeObject(new clsApiStatus { statusExec = false, ban = -1, msg = "No existe una sesión activa." });
        }

        // GET /api/estado
        [WebMethod(EnableSession = true)]
        public static string ConsultarEstado()
        {
            string usuario = UsuarioSesion();
            if (usuario == null)
                return SinSesion();

            clsRespuestaApi r = clsApiCliente.GetAsync("/api/estado", usuario).GetAwaiter().GetResult();
            return JsonConvert.SerializeObject(r.status);
        }

        // POST /api/enviar
        [WebMethod(EnableSession = true)]
        public static string EnviarNotificacion(string tipo, string nivel, string titulo, string mensaje)
        {
            string usuario = UsuarioSesion();
            if (usuario == null)
                return SinSesion();

            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensaje))
                return JsonConvert.SerializeObject(new clsApiStatus { statusExec = false, ban = -2, msg = "Título y mensaje son obligatorios." });

            clsRespuestaApi r = clsApiCliente.PostAsync("/api/enviar", new
            {
                tipo = tipo,
                titulo = titulo,
                mensaje = mensaje,
                usuario = usuario,
                endpoint = "POST /api/enviar (consola)",
                ban = 0,
                nivel = nivel
            }, usuario).GetAwaiter().GetResult();

            return JsonConvert.SerializeObject(r.status);
        }
    }
}
