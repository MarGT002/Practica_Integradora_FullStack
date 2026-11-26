using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apiRESTReservacionesFull.Models;

namespace apiRESTReservacionesFull.Controllers
{
    /// <summary>
    /// Proceso principal: disponibilidad, registro, consulta y cancelación de reservas.
    /// </summary>
    [RoutePrefix("api")]
    public class ReservasController : BaseApiController
    {
        // -------------------------------------------------------------------------
        // GET api/disponibilidad?id_espacio=1&fecha=2026-12-10&hora_inicio=08:00&hora_fin=10:00
        //   ban =  1  disponible (200)
        //   ban =  0  conflicto de horario (409) + lista de reservas que se cruzan
        //   ban =  2  hora_fin no es mayor que hora_inicio (400)
        //   ban =  3  espacio inexistente o inactivo (404)
        //   ban =  5  fecha pasada (400)
        //   ban = -2  datos incompletos / formato inválido (400)
        //   ban = -1  error de servidor (500)
        // -------------------------------------------------------------------------
        [HttpGet]
        [Route("disponibilidad")]
        public HttpResponseMessage disponibilidad(int id_espacio = 0, string fecha = null,
                                                  string hora_inicio = null, string hora_fin = null)
        {
            const string endpoint = "GET /api/disponibilidad";

            clsReserva objReserva = new clsReserva(id_espacio, fecha, hora_inicio, hora_fin);

            string error = objReserva.ValidarFormato(false);
            if (error != null)
                return DatosInvalidos(error, endpoint);

            clsApiStatus objRespuesta = new clsApiStatus();

            if (!objReserva.HorarioValido())
            {
                objRespuesta.statusExec = true;
                objRespuesta.ban = 2;
                objRespuesta.msg = "La hora de fin debe ser mayor que la hora de inicio.";

                clsNotificador.Publicar("DISPONIBILIDAD", "Horario inválido", objRespuesta.msg,
                    UsuarioPeticion(), endpoint, 2, "warning");

                return Responder(HttpStatusCode.BadRequest, objRespuesta);
            }

            try
            {
                DataSet ds = objReserva.spValidarDisponibilidad();
                DataRow fila = ds.Tables[0].Rows[0];

                objRespuesta.statusExec = true;
                objRespuesta.ban = Convert.ToInt32(fila["ban"]);
                objRespuesta.msg = fila["mensaje"].ToString();

                objRespuesta.datos.Add("id_espacio", id_espacio);
                objRespuesta.datos.Add("fecha", fecha);
                objRespuesta.datos.Add("hora_inicio", hora_inicio);
                objRespuesta.datos.Add("hora_fin", hora_fin);
                objRespuesta.datos.Add("disponible", objRespuesta.ban == 1);
                objRespuesta.datos.Add("conflictos", TablaAJson(ds.Tables.Count > 1 ? ds.Tables[1] : null));

                string detalle = "Espacio " + id_espacio + ", " + fecha + " de " + hora_inicio + " a " + hora_fin + ": ";
                HttpStatusCode codigo;
                string nivel;
                string tipo = "DISPONIBILIDAD";

                switch (objRespuesta.ban)
                {
                    case 1: codigo = HttpStatusCode.OK; nivel = "success"; break;
                    case 0: codigo = HttpStatusCode.Conflict; nivel = "danger"; tipo = "CONFLICTO"; break;
                    case 3: codigo = HttpStatusCode.NotFound; nivel = "warning"; break;
                    default: codigo = HttpStatusCode.BadRequest; nivel = "warning"; break;
                }

                clsNotificador.Publicar(tipo, "Validación de disponibilidad", detalle + objRespuesta.msg,
                    UsuarioPeticion(), endpoint, objRespuesta.ban, nivel);

                return Responder(codigo, objRespuesta);
            }
            catch (Exception ex)
            {
                return ErrorServidor("Fallo al validar la disponibilidad.", ex, endpoint);
            }
        }

        // -------------------------------------------------------------------------
        // POST api/reservas
        // Body: { "id_espacio":1, "id_usuario":2, "fecha_reserva":"2026-12-10",
        //         "hora_inicio":"08:00", "hora_fin":"10:00", "motivo":"Clase" }
        //   ban =  0  reserva registrada (201) + id_reserva
        //   ban =  1  conflicto de horario (409)
        //   ban =  2  horario inválido (400)
        //   ban =  3  espacio inexistente o inactivo (404)
        //   ban =  4  usuario inexistente o inactivo (404)
        //   ban =  5  fecha pasada (400)
        //   ban = -2  datos incompletos / formato inválido (400)
        //   ban = -1  error de servidor (500)
        // -------------------------------------------------------------------------
        [HttpPost]
        [Route("reservas")]
        public HttpResponseMessage registrar([FromBody] clsReserva modelo)
        {
            const string endpoint = "POST /api/reservas";

            if (modelo == null)
                return DatosInvalidos("No se recibieron datos de la reserva.", endpoint);

            string error = modelo.ValidarFormato(true);
            if (error != null)
                return DatosInvalidos(error, endpoint);

            clsApiStatus objRespuesta = new clsApiStatus();

            if (!modelo.HorarioValido())
            {
                objRespuesta.statusExec = true;
                objRespuesta.ban = 2;
                objRespuesta.msg = "La hora de fin debe ser mayor que la hora de inicio.";

                clsNotificador.Publicar("RESERVA_RECHAZADA", "Reserva rechazada", objRespuesta.msg,
                    UsuarioPeticion(), endpoint, 2, "warning");

                return Responder(HttpStatusCode.BadRequest, objRespuesta);
            }

            try
            {
                DataSet ds = modelo.spInsReserva();
                DataRow fila = ds.Tables[0].Rows[0];

                objRespuesta.statusExec = true;
                objRespuesta.ban = Convert.ToInt32(fila["ban"]);
                objRespuesta.msg = fila["mensaje"].ToString();

                int idReserva = Convert.ToInt32(fila["id_reserva"]);
                objRespuesta.datos.Add("id_reserva", idReserva);
                objRespuesta.datos.Add("id_espacio", modelo.id_espacio);
                objRespuesta.datos.Add("id_usuario", modelo.id_usuario);
                objRespuesta.datos.Add("fecha_reserva", modelo.fecha_reserva);
                objRespuesta.datos.Add("hora_inicio", modelo.hora_inicio);
                objRespuesta.datos.Add("hora_fin", modelo.hora_fin);
                objRespuesta.datos.Add("motivo", modelo.motivo);

                string detalle = "Espacio " + modelo.id_espacio + ", " + modelo.fecha_reserva +
                                 " de " + modelo.hora_inicio + " a " + modelo.hora_fin + ": ";
                HttpStatusCode codigo;

                switch (objRespuesta.ban)
                {
                    case 0:
                        codigo = HttpStatusCode.Created;
                        clsNotificador.Publicar("RESERVA_REGISTRADA", "Nueva reserva #" + idReserva,
                            detalle + "\"" + modelo.motivo + "\"", UsuarioPeticion(), endpoint, 0, "success");
                        break;
                    case 1:
                        codigo = HttpStatusCode.Conflict;
                        clsNotificador.Publicar("CONFLICTO", "Reserva rechazada por cruce de horario",
                            detalle + objRespuesta.msg, UsuarioPeticion(), endpoint, 1, "danger");
                        break;
                    default:
                        codigo = (objRespuesta.ban == 3 || objRespuesta.ban == 4)
                                 ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest;
                        clsNotificador.Publicar("RESERVA_RECHAZADA", "Reserva rechazada",
                            detalle + objRespuesta.msg, UsuarioPeticion(), endpoint, objRespuesta.ban, "warning");
                        break;
                }

                return Responder(codigo, objRespuesta);
            }
            catch (Exception ex)
            {
                return ErrorServidor("Fallo al registrar la reserva.", ex, endpoint);
            }
        }

        // -------------------------------------------------------------------------
        // GET api/reservas?estado=Activa&fecha=2026-12-01&id_espacio=1&id_usuario=0
        // Todos los filtros son opcionales.
        //   ban = número de reservas encontradas (200)
        // -------------------------------------------------------------------------
        [HttpGet]
        [Route("reservas")]
        public HttpResponseMessage consultar(string estado = "", string fecha = "",
                                             int id_espacio = 0, int id_usuario = 0)
        {
            const string endpoint = "GET /api/reservas";

            if (!string.IsNullOrWhiteSpace(estado) &&
                estado != "Activa" && estado != "Cancelada" && estado != "Concluida")
            {
                return DatosInvalidos("El estado debe ser Activa, Cancelada o Concluida.", endpoint);
            }

            clsApiStatus objRespuesta = new clsApiStatus();

            try
            {
                clsReserva objReserva = new clsReserva();
                DataSet ds = objReserva.spConsultarReservas(estado, fecha, id_espacio, id_usuario);

                objRespuesta.statusExec = true;
                objRespuesta.ban = ds.Tables[0].Rows.Count;
                objRespuesta.msg = "Consulta de reservas realizada exitosamente.";
                objRespuesta.datos.Add("vwReservas", TablaAJson(ds.Tables[0]));

                string filtros = (string.IsNullOrWhiteSpace(estado) ? "todas" : estado) +
                                 (string.IsNullOrWhiteSpace(fecha) ? "" : ", fecha " + fecha) +
                                 (id_espacio > 0 ? ", espacio " + id_espacio : "");

                clsNotificador.Publicar("CONSULTA_RESERVAS", "Consulta de reservas",
                    "Se consultaron " + objRespuesta.ban + " reservas (" + filtros + ").",
                    UsuarioPeticion(), endpoint, objRespuesta.ban, "info");

                return Responder(HttpStatusCode.OK, objRespuesta);
            }
            catch (Exception ex)
            {
                return ErrorServidor("Fallo en la consulta de reservas.", ex, endpoint);
            }
        }

        // -------------------------------------------------------------------------
        // PUT api/reservas/5/cancelar
        //   ban = 0 cancelada (200) | 1 no existe (404) | 2 no está activa (409)
        // -------------------------------------------------------------------------
        [HttpPut]
        [Route("reservas/{id:int}/cancelar")]
        public HttpResponseMessage cancelar(int id)
        {
            const string endpoint = "PUT /api/reservas/{id}/cancelar";
            clsApiStatus objRespuesta = new clsApiStatus();

            try
            {
                clsReserva objReserva = new clsReserva();
                objReserva.id_reserva = id;
                DataSet ds = objReserva.spCancelarReserva();
                DataRow fila = ds.Tables[0].Rows[0];

                objRespuesta.statusExec = true;
                objRespuesta.ban = Convert.ToInt32(fila["ban"]);
                objRespuesta.msg = fila["mensaje"].ToString();
                objRespuesta.datos.Add("id_reserva", id);

                HttpStatusCode codigo = objRespuesta.ban == 0 ? HttpStatusCode.OK
                                      : objRespuesta.ban == 1 ? HttpStatusCode.NotFound
                                      : HttpStatusCode.Conflict;

                clsNotificador.Publicar(objRespuesta.ban == 0 ? "RESERVA_CANCELADA" : "CANCELACION_RECHAZADA",
                    "Cancelación de la reserva #" + id, objRespuesta.msg,
                    UsuarioPeticion(), endpoint, objRespuesta.ban, objRespuesta.ban == 0 ? "secondary" : "warning");

                return Responder(codigo, objRespuesta);
            }
            catch (Exception ex)
            {
                return ErrorServidor("Fallo al cancelar la reserva.", ex, endpoint);
            }
        }
    }
}
