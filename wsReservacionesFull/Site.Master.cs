using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.UI;
using wsReservacionesFull.Clases;

namespace wsReservacionesFull
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Datos de la sesión en el encabezado
            lblNombre.Text = HttpUtility.HtmlEncode(Convert.ToString(Session["nombre_completo"]));
            lblRol.Text = HttpUtility.HtmlEncode(Convert.ToString(Session["rol"]));
        }

        // URL pública de la API para JavaScript (EventSource del canal PUSH)
        protected string UrlApiPublica
        {
            get
            {
                string url = ConfigurationManager.AppSettings["urlApiPublica"] ?? "";
                return HttpUtility.JavaScriptStringEncode(url.TrimEnd('/'));
            }
        }

        // Marca como "active" la opción del menú de la página actual
        protected string Activa(string pagina)
        {
            string actual = Path.GetFileName(Request.AppRelativeCurrentExecutionFilePath ?? "");
            return string.Equals(actual, pagina, StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }

        // Cierre de sesión
        protected void lnkSalir_Click(object sender, EventArgs e)
        {
            string usuario = Convert.ToString(Session["usuario"]);
            string nombre = Convert.ToString(Session["nombre_completo"]);

            // Notificación PUSH de cierre de sesión (si falla, igual se cierra la sesión)
            try
            {
                clsApiCliente.PostAsync("/api/enviar", new
                {
                    tipo = "LOGOUT",
                    titulo = "Cierre de sesión",
                    mensaje = nombre + " cerró sesión.",
                    usuario = usuario,
                    endpoint = "Sitio · Site.Master",
                    ban = 0,
                    nivel = "secondary"
                }, usuario).GetAwaiter().GetResult();
            }
            catch { }

            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx?salir=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
