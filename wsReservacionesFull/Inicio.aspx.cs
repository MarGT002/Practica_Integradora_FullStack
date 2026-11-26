using System;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using wsReservacionesFull.Clases;

namespace wsReservacionesFull
{
    public partial class Inicio : clsPaginaProtegida
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblBienvenida.Text = HttpUtility.HtmlEncode(NombreCompleto);
            lblInicioSesion.Text = HttpUtility.HtmlEncode(Convert.ToString(Session["inicio_sesion"]));

            if (!IsPostBack)
                RegisterAsyncTask(new PageAsyncTask(CargarDatosAsync));
        }

        private async Task CargarDatosAsync()
        {
            // GET /api/reservas (todas)  y  GET /api/espacios
            Task<clsRespuestaApi> tareaReservas = clsApiCliente.GetAsync("/api/reservas", Usuario);
            Task<clsRespuestaApi> tareaEspacios = clsApiCliente.GetAsync("/api/espacios", Usuario);
            await Task.WhenAll(tareaReservas, tareaEspacios);

            clsRespuestaApi reservas = tareaReservas.Result;
            clsRespuestaApi espacios = tareaEspacios.Result;

            if (!reservas.Ok || !espacios.Ok)
            {
                litMensaje.Text = HtmlAlerta("danger", "plug", !reservas.Ok ? reservas.status.msg : espacios.status.msg);
                return;
            }

            DataTable dt = clsApiCliente.ATabla(reservas.status.datos, "vwReservas");
            litEspacios.Text = espacios.status.ban.ToString();

            if (dt.Columns.Contains("estado"))
            {
                litActivas.Text = dt.Select("estado = 'Activa'").Length.ToString();
                litConcluidas.Text = dt.Select("estado = 'Concluida'").Length.ToString();
                litCanceladas.Text = dt.Select("estado = 'Cancelada'").Length.ToString();

                // Próximas 6 reservas activas (orden ascendente por fecha y hora)
                DataView vista = new DataView(dt, "estado = 'Activa'", "fecha_reserva ASC, hora_inicio ASC", DataViewRowState.CurrentRows);
                DataTable proximas = vista.ToTable();
                while (proximas.Rows.Count > 6)
                    proximas.Rows.RemoveAt(proximas.Rows.Count - 1);

                gvProximas.DataSource = proximas;
            }

            gvProximas.DataBind();
        }
    }
}
