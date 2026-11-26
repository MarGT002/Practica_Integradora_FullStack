using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using wsReservacionesFull.Clases;

namespace wsReservacionesFull
{
    public partial class Reservar : clsPaginaProtegida
    {
        // Estado de los pasos del proceso: "" (pendiente), "ok" o "error"
        private string Paso(int numero)
        {
            return Convert.ToString(ViewState["paso" + numero]);
        }

        private void Paso(int numero, string estado)
        {
            ViewState["paso" + numero] = estado;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            txtSolicitante.Text = NombreCompleto + " (" + Usuario + ")";

            if (!IsPostBack)
            {
                // Valores que llegan desde Disponibilidad.aspx ("Reservar este horario")
                txtFecha.Text = string.IsNullOrEmpty(Request.QueryString["fecha"])
                                ? DateTime.Today.ToString("yyyy-MM-dd") : Request.QueryString["fecha"];
                txtHoraInicio.Text = Request.QueryString["hi"] ?? "";
                txtHoraFin.Text = Request.QueryString["hf"] ?? "";

                RegisterAsyncTask(new PageAsyncTask(CargarAsync));
            }
        }

        private async Task CargarAsync()
        {
            string error = await CargarEspaciosAsync(ddlEspacio, "-- Selecciona un espacio --");
            if (error != null)
            {
                litMensaje.Text = HtmlAlerta("danger", "plug", error);
                return;
            }

            string espacio = Request.QueryString["id_espacio"];
            if (!string.IsNullOrEmpty(espacio) && ddlEspacio.Items.FindByValue(espacio) != null)
                ddlEspacio.SelectedValue = espacio;
        }

        // ---------------------------------------------------------------
        // PASO 2 y 3: selección de espacio/horario + validación de disponibilidad
        // ---------------------------------------------------------------
        protected async void btnValidar_Click(object sender, EventArgs e)
        {
            Paso(4, "");
            string error = ValidarCampos(false);
            if (error != null)
            {
                litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", error);
                return;
            }

            // Paso 2: notificación PUSH de la selección (POST /api/enviar)
            await clsApiCliente.PostAsync("/api/enviar", new
            {
                tipo = "SELECCION",
                titulo = "Selección de espacio y horario",
                mensaje = NombreCompleto + " seleccionó " + ddlEspacio.SelectedItem.Text + " el " + txtFecha.Text +
                          " de " + txtHoraInicio.Text + " a " + txtHoraFin.Text + ".",
                usuario = Usuario,
                endpoint = "Sitio · Reservar.aspx",
                ban = 0,
                nivel = "info"
            }, Usuario);
            Paso(2, "ok");

            // Paso 3: GET /api/disponibilidad (la API envía su propia notificación PUSH)
            clsRespuestaApi resultado = await clsApiCliente.GetAsync(
                "/api/disponibilidad?id_espacio=" + HttpUtility.UrlEncode(ddlEspacio.SelectedValue) +
                "&fecha=" + HttpUtility.UrlEncode(txtFecha.Text) +
                "&hora_inicio=" + HttpUtility.UrlEncode(txtHoraInicio.Text) +
                "&hora_fin=" + HttpUtility.UrlEncode(txtHoraFin.Text), Usuario);

            bool disponible = resultado.status.statusExec && resultado.status.ban == 1;
            Paso(3, disponible ? "ok" : "error");

            if (disponible)
                litMensaje.Text = HtmlAlerta("success", "check-circle", resultado.status.msg + " Ya puedes registrar la reserva.");
            else if (resultado.status.ban == 0)
                litMensaje.Text = HtmlAlerta("danger", "x-octagon", resultado.status.msg);
            else
                litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", resultado.status.msg);

            DataTable conflictos = clsApiCliente.ATabla(resultado.status.datos, "conflictos");
            pnlConflictos.Visible = conflictos.Rows.Count > 0;
            gvConflictos.DataSource = conflictos;
            gvConflictos.DataBind();

            await MostrarOcupacionAsync(disponible);
        }

        // ---------------------------------------------------------------
        // PASO 4: registro de la reserva (POST /api/reservas)
        // ---------------------------------------------------------------
        protected async void btnRegistrar_Click(object sender, EventArgs e)
        {
            string error = ValidarCampos(true);
            if (error != null)
            {
                litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", error);
                return;
            }

            clsRespuestaApi resultado = await clsApiCliente.PostAsync("/api/reservas", new
            {
                id_espacio = int.Parse(ddlEspacio.SelectedValue),
                id_usuario = IdUsuario,
                fecha_reserva = txtFecha.Text,
                hora_inicio = txtHoraInicio.Text,
                hora_fin = txtHoraFin.Text,
                motivo = txtMotivo.Text.Trim()
            }, Usuario);

            pnlConflictos.Visible = false;

            if (resultado.status.statusExec && resultado.status.ban == 0)
            {
                Paso(2, "ok");
                Paso(3, "ok");
                Paso(4, "ok");
                litMensaje.Text = HtmlAlerta("success", "check2-circle",
                    "Reserva #" + resultado.status.datos["id_reserva"] + " registrada correctamente en MySQL.") +
                    "<p class='mb-3'><a class='btn btn-sm btn-outline-primary' href='Reservas.aspx'><i class='bi bi-list-check me-1'></i>Ver listado de reservas</a></p>";
                txtMotivo.Text = "";
                await MostrarOcupacionAsync(false);
                return;
            }

            Paso(4, "error");
            if (resultado.status.ban == 1)
            {
                Paso(3, "error");
                litMensaje.Text = HtmlAlerta("danger", "x-octagon", resultado.status.msg);
                await MostrarOcupacionAsync(false);
            }
            else
            {
                litMensaje.Text = HtmlAlerta("warning", "exclamation-triangle", resultado.status.msg);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlEspacio.SelectedIndex = 0;
            txtFecha.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtHoraInicio.Text = "";
            txtHoraFin.Text = "";
            txtMotivo.Text = "";
            litMensaje.Text = "";
            pnlVisual.Visible = false;
            pnlConflictos.Visible = false;
            Paso(2, ""); Paso(3, ""); Paso(4, "");
        }

        // Línea de tiempo del espacio en la fecha seleccionada
        private async Task MostrarOcupacionAsync(bool disponible)
        {
            clsRespuestaApi reservas = await clsApiCliente.GetAsync(
                "/api/reservas?fecha=" + HttpUtility.UrlEncode(txtFecha.Text) +
                "&id_espacio=" + HttpUtility.UrlEncode(ddlEspacio.SelectedValue), Usuario);

            if (!reservas.Ok)
                return;

            DataTable dt = clsApiCliente.ATabla(reservas.status.datos, "vwReservas");
            litLineaTiempo.Text = clsLineaTiempo.Generar(dt, txtHoraInicio.Text, txtHoraFin.Text, disponible);
            pnlVisual.Visible = true;
            pnlAyuda.Visible = false;
        }

        private string ValidarCampos(bool registrar)
        {
            if (ddlEspacio.SelectedValue == "") return "Selecciona un espacio.";
            if (txtFecha.Text == "") return "Selecciona la fecha.";
            if (txtHoraInicio.Text == "" || txtHoraFin.Text == "") return "Indica la hora de inicio y la hora de fin.";
            if (string.CompareOrdinal(txtHoraFin.Text, txtHoraInicio.Text) <= 0)
                return "La hora de fin debe ser mayor que la hora de inicio.";
            if (registrar && txtMotivo.Text.Trim() == "") return "Escribe el motivo de la reserva.";
            if (registrar && txtMotivo.Text.Trim().Length > 180) return "El motivo no debe exceder 180 caracteres.";
            return null;
        }

        // Los pasos se dibujan al final (cuando ya terminaron los métodos asíncronos)
        protected override void OnPreRenderComplete(EventArgs e)
        {
            litPasos.Text = GenerarPasos();
            base.OnPreRenderComplete(e);
        }

        private string GenerarPasos()
        {
            string[,] pasos =
            {
                { "Inicio de sesión", "Usuario validado: " + Usuario },
                { "Selección", "Espacio y horario ini-fin" },
                { "Disponibilidad", "Sin cruces con reservas activas" },
                { "Registro", "Reserva guardada en MySQL" }
            };

            StringBuilder sb = new StringBuilder("<div class='pasos'>");
            for (int i = 0; i < 4; i++)
            {
                string estado = i == 0 ? "ok" : Paso(i + 1);
                string clase = estado == "ok" ? " hecho" : estado == "error" ? " error" : "";
                string icono = estado == "ok" ? "<i class='bi bi-check-lg'></i>" : estado == "error" ? "<i class='bi bi-x-lg'></i>" : (i + 1).ToString();

                sb.Append("<div class='paso" + clase + "'><div class='num'>" + icono + "</div>" +
                          "<div class='fw-semibold'>Paso " + (i + 1) + ": " + pasos[i, 0] + "</div>" +
                          "<small>" + HttpUtility.HtmlEncode(pasos[i, 1]) + " · <i class='bi bi-broadcast'></i> push</small></div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }
    }
}
