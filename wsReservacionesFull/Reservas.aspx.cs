using System;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using wsReservacionesFull.Clases;

namespace wsReservacionesFull
{
    public partial class Reservas : clsPaginaProtegida
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["estado"]) &&
                    ddlEstado.Items.FindByValue(Request.QueryString["estado"]) != null)
                    ddlEstado.SelectedValue = Request.QueryString["estado"];

                RegisterAsyncTask(new PageAsyncTask(CargarInicialAsync));
            }
        }

        private async Task CargarInicialAsync()
        {
            string error = await CargarEspaciosAsync(ddlEspacio, "Todos los espacios");
            if (error != null)
            {
                litMensaje.Text = HtmlAlerta("danger", "plug", error);
                return;
            }
            await CargarReservasAsync();
        }

        protected async void btnFiltrar_Click(object sender, EventArgs e)
        {
            await CargarReservasAsync();
        }

        protected async void btnTodas_Click(object sender, EventArgs e)
        {
            ddlEstado.SelectedIndex = 0;
            ddlEspacio.SelectedIndex = 0;
            txtFecha.Text = "";
            chkMias.Checked = false;
            await CargarReservasAsync();
        }

        // GET /api/reservas con los filtros seleccionados
        private async Task CargarReservasAsync()
        {
            string ruta = "/api/reservas?estado=" + HttpUtility.UrlEncode(ddlEstado.SelectedValue) +
                          "&fecha=" + HttpUtility.UrlEncode(txtFecha.Text) +
                          "&id_espacio=" + (ddlEspacio.SelectedValue == "" ? "0" : ddlEspacio.SelectedValue) +
                          "&id_usuario=" + (chkMias.Checked ? IdUsuario : 0);

            clsRespuestaApi respuesta = await clsApiCliente.GetAsync(ruta, Usuario);

            if (!respuesta.Ok)
            {
                litMensaje.Text = HtmlAlerta("danger", "plug", respuesta.status.msg);
                gvReservas.DataSource = null;
                gvReservas.DataBind();
                return;
            }

            DataTable dt = clsApiCliente.ATabla(respuesta.status.datos, "vwReservas");
            gvReservas.DataSource = dt;
            gvReservas.DataBind();

            int activas = 0, canceladas = 0, concluidas = 0;
            if (dt.Columns.Contains("estado"))
            {
                activas = dt.Select("estado = 'Activa'").Length;
                canceladas = dt.Select("estado = 'Cancelada'").Length;
                concluidas = dt.Select("estado = 'Concluida'").Length;
            }

            litResumen.Text = "<div class='d-flex gap-2 flex-wrap'>" +
                "<span class='badge rounded-pill text-bg-light border'>Total: " + respuesta.status.ban + "</span>" +
                "<span class='badge rounded-pill badge-Activa'>Activas: " + activas + "</span>" +
                "<span class='badge rounded-pill badge-Concluida'>Concluidas: " + concluidas + "</span>" +
                "<span class='badge rounded-pill badge-Cancelada'>Canceladas: " + canceladas + "</span></div>";
        }

        // Solo se pueden cancelar reservas activas: las propias o cualquiera si es Administrador
        protected bool PuedeCancelar(object estado, object idUsuario)
        {
            if (Convert.ToString(estado) != "Activa")
                return false;

            return EsAdministrador || Convert.ToInt32(idUsuario) == IdUsuario;
        }

        // PUT /api/reservas/{id}/cancelar
        protected async void gvReservas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Cancelar")
                return;

            int idReserva = Convert.ToInt32(e.CommandArgument);
            clsRespuestaApi respuesta = await clsApiCliente.PutAsync("/api/reservas/" + idReserva + "/cancelar", Usuario);

            if (respuesta.status.statusExec && respuesta.status.ban == 0)
                litMensaje.Text = HtmlAlerta("success", "check-circle", "Reserva #" + idReserva + ": " + respuesta.status.msg);
            else
                litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", "Reserva #" + idReserva + ": " + respuesta.status.msg);

            await CargarReservasAsync();
        }
    }
}
