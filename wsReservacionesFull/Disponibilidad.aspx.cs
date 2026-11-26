using System;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using wsReservacionesFull.Clases;

namespace wsReservacionesFull
{
    public partial class Disponibilidad : clsPaginaProtegida
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Today.ToString("yyyy-MM-dd");
                RegisterAsyncTask(new PageAsyncTask(CargarAsync));
            }
        }

        private async Task CargarAsync()
        {
            string error = await CargarEspaciosAsync(ddlEspacio, "-- Selecciona un espacio --");
            if (error != null)
                litMensaje.Text = HtmlAlerta("danger", "plug", error);
        }

        protected async void btnConsultar_Click(object sender, EventArgs e)
        {
            string idEspacio = ddlEspacio.SelectedValue;
            string fecha = txtFecha.Text;
            string horaInicio = txtHoraInicio.Text;
            string horaFin = txtHoraFin.Text;

            pnlResultado.Visible = false;

            if (idEspacio == "" || fecha == "")
            {
                litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", "Selecciona un espacio y una fecha.");
                return;
            }

            bool validarHorario = horaInicio != "" || horaFin != "";
            if (validarHorario && (horaInicio == "" || horaFin == ""))
            {
                litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", "Para validar un horario indica la hora de inicio y la hora de fin.");
                return;
            }

            // 1) Reservas de ese espacio en esa fecha: GET /api/reservas?fecha=...&id_espacio=...
            clsRespuestaApi reservas = await clsApiCliente.GetAsync(
                "/api/reservas?fecha=" + HttpUtility.UrlEncode(fecha) + "&id_espacio=" + HttpUtility.UrlEncode(idEspacio), Usuario);

            if (!reservas.Ok)
            {
                litMensaje.Text = HtmlAlerta("danger", "plug", reservas.status.msg);
                return;
            }

            DataTable dt = clsApiCliente.ATabla(reservas.status.datos, "vwReservas");
            bool disponible = false;
            litMensaje.Text = "";

            // 2) Validación del horario: GET /api/disponibilidad
            if (validarHorario)
            {
                clsRespuestaApi resultado = await clsApiCliente.GetAsync(
                    "/api/disponibilidad?id_espacio=" + HttpUtility.UrlEncode(idEspacio) +
                    "&fecha=" + HttpUtility.UrlEncode(fecha) +
                    "&hora_inicio=" + HttpUtility.UrlEncode(horaInicio) +
                    "&hora_fin=" + HttpUtility.UrlEncode(horaFin), Usuario);

                disponible = resultado.status.ban == 1 && resultado.status.statusExec;

                if (disponible)
                    litMensaje.Text = HtmlAlerta("success", "check-circle", resultado.status.msg + " (" + horaInicio + " a " + horaFin + ")");
                else if (resultado.status.ban == 0)
                    litMensaje.Text = HtmlAlerta("danger", "x-octagon", resultado.status.msg);
                else
                    litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", resultado.status.msg);
            }

            lblEspacio.Text = HttpUtility.HtmlEncode(ddlEspacio.SelectedItem.Text);
            lblFecha.Text = HttpUtility.HtmlEncode(fecha);
            litLineaTiempo.Text = clsLineaTiempo.Generar(dt, horaInicio, horaFin, disponible);

            DataView vista = new DataView(dt);
            if (dt.Columns.Contains("hora_inicio"))
                vista.Sort = "hora_inicio ASC";
            gvDia.DataSource = vista;
            gvDia.DataBind();

            hlReservar.Visible = disponible;
            hlReservar.NavigateUrl = "~/Reservar.aspx?id_espacio=" + HttpUtility.UrlEncode(idEspacio) +
                                     "&fecha=" + HttpUtility.UrlEncode(fecha) +
                                     "&hi=" + HttpUtility.UrlEncode(horaInicio) +
                                     "&hf=" + HttpUtility.UrlEncode(horaFin);
            pnlResultado.Visible = true;
        }
    }
}
