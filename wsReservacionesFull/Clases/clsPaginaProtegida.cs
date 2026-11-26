using System;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace wsReservacionesFull.Clases
{
    /// <summary>
    /// Página base para TODAS las páginas que requieren sesión.
    /// Si no existe sesión activa, redirige a Login.aspx antes de ejecutar
    /// cualquier código de la página (evita el ingreso directo por URL).
    /// También evita que el navegador muestre páginas protegidas desde caché
    /// después de cerrar sesión (botón Atrás).
    /// </summary>
    public class clsPaginaProtegida : Page
    {
        protected override void OnInit(EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (Session["usuario"] == null)
            {
                // true = termina la ejecución de la página aquí mismo
                Response.Redirect("~/Login.aspx?expirada=1", true);
            }

            base.OnInit(e);
        }

        // Atajos a los datos de la sesión
        protected int IdUsuario
        {
            get { return Session["id_usuario"] == null ? 0 : Convert.ToInt32(Session["id_usuario"]); }
        }

        protected string Usuario
        {
            get { return Convert.ToString(Session["usuario"]); }
        }

        protected string NombreCompleto
        {
            get { return Convert.ToString(Session["nombre_completo"]); }
        }

        protected string Rol
        {
            get { return Convert.ToString(Session["rol"]); }
        }

        protected bool EsAdministrador
        {
            get { return Rol == "Administrador"; }
        }

        // HTML de una alerta Bootstrap (success, danger, warning, info)
        protected static string HtmlAlerta(string tipo, string icono, string texto)
        {
            return "<div class='alert alert-" + tipo + " d-flex align-items-start gap-2 shadow-sm' role='alert'>" +
                   "<i class='bi bi-" + icono + " fs-5'></i><div>" + HttpUtility.HtmlEncode(texto) + "</div></div>";
        }

        // Llena un DropDownList con los espacios activos (GET /api/espacios).
        // Regresa el mensaje de error o null si todo salió bien.
        protected async Task<string> CargarEspaciosAsync(DropDownList ddl, string textoInicial)
        {
            clsRespuestaApi respuesta = await clsApiCliente.GetAsync("/api/espacios", Usuario);

            ddl.Items.Clear();
            if (!respuesta.Ok)
            {
                ddl.Items.Add(new ListItem("(sin conexión con la API)", ""));
                return respuesta.status.msg;
            }

            DataTable dt = clsApiCliente.ATabla(respuesta.status.datos, "vwEspaciosActivos");
            ddl.DataSource = dt;
            ddl.DataTextField = "descripcion";
            ddl.DataValueField = "id_espacio";
            ddl.DataBind();
            ddl.Items.Insert(0, new ListItem(textoInicial, ""));
            return null;
        }

        // Script para mostrar un mensaje como alerta del navegador
        protected void Alerta(string mensaje)
        {
            string texto = HttpUtility.JavaScriptStringEncode(mensaje);
            ClientScript.RegisterStartupScript(GetType(), "alerta" + Guid.NewGuid().ToString("N"),
                "alert('" + texto + "');", true);
        }
    }
}
