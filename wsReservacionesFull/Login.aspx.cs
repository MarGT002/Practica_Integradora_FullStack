using System;
using System.Web;
using System.Web.UI;
using wsReservacionesFull.Clases;

namespace wsReservacionesFull
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            // Si ya hay sesión, ir directo al inicio
            if (Session["usuario"] != null)
            {
                Response.Redirect("~/Inicio.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (Request.QueryString["salir"] == "1")
                Mostrar("success", "check-circle", "Sesión cerrada correctamente.");
            else if (Request.QueryString["expirada"] == "1")
                Mostrar("warning", "exclamation-triangle", "Debes iniciar sesión para entrar a esa página.");
        }

        protected async void btnAcceder_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Text;

            if (usuario.Length == 0 || password.Length == 0)
            {
                Mostrar("warning", "exclamation-triangle", "Escribe tu usuario y contraseña.");
                return;
            }

            // POST /api/acceso/login
            clsRespuestaApi respuesta = await clsApiCliente.PostAsync("/api/acceso/login",
                new { usuario = usuario, password = password }, usuario);

            if (respuesta.status.ban == 1)
            {
                // Datos de sesión
                Session["id_usuario"] = (int)respuesta.status.datos["id_usuario"];
                Session["usuario"] = (string)respuesta.status.datos["usuario"];
                Session["nombre_completo"] = (string)respuesta.status.datos["nombre_completo"];
                Session["correo"] = (string)respuesta.status.datos["correo_institucional"];
                Session["rol"] = (string)respuesta.status.datos["rol"];
                Session["inicio_sesion"] = (string)respuesta.status.datos["inicio_sesion"];

                Response.Redirect("~/Inicio.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (respuesta.status.ban == 0)
                Mostrar("danger", "x-circle", "Acceso denegado: usuario o contraseña incorrectos.");
            else
                Mostrar("danger", "plug", respuesta.status.msg);
        }

        private void Mostrar(string tipo, string icono, string texto)
        {
            litMensaje.Text = "<div class='alert alert-" + tipo + " d-flex gap-2 align-items-start'>" +
                              "<i class='bi bi-" + icono + "'></i><div>" + HttpUtility.HtmlEncode(texto) + "</div></div>";
        }
    }
}
