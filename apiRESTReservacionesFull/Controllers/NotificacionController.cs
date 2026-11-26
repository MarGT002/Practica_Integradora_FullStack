using apiRESTReservacionesFull.Models;

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace apiRESTReservacionesFull.Controllers
{
    [RoutePrefix("api")]
    public class NotificacionController : BaseApiController
    {
        // GET
        // api/stream
        // Abre el canal PUSH/SSE.
        [HttpGet]
        [Route("stream")]
        public HttpResponseMessage stream()
        {
            // Registrar al cliente ANTES de devolver la respuesta.
            clsSuscriptorSse suscriptor = clsNotificacionSseBroker.Suscribir();

            // Obtener token de desconexión del cliente.
            //
            // ASP.NET activa este token cuando el navegador,
            // Postman, etc. cierra la conexión.
            CancellationToken tokenDesconexion = CancellationToken.None;


            if (HttpContext.Current != null && HttpContext.Current.Response != null)
            {
                tokenDesconexion = HttpContext.Current.Response.ClientDisconnectedToken;

                // Desactivar buffering de ASP.NET.
                HttpContext.Current.Response.BufferOutput = false;
            }

            HttpResponseMessage respuesta = Request.CreateResponse(HttpStatusCode.OK);

            // IMPORTANTE
            // Forzamos explícitamente el constructor asíncrono
            // Func<..., Task>

            Func<Stream, HttpContent, TransportContext, Task> funcionStream =
                async (streamSalida, contenido, contextoTransporte) =>
                {
                    try
                    {
                        // 1. EVENTO INICIAL
                        await EscribirEventoSse(streamSalida, "conexion",
                                                "{\"mensaje\":\"Canal SSE conectado correctamente\"}",
                                                tokenDesconexion).ConfigureAwait(false);

                        // 2. CICLO PERMANENTE
                        while (!tokenDesconexion.IsCancellationRequested)
                        {
                            string mensaje = null;

                            // Creamos token combinado:
                            //
                            // - desconexión cliente
                            // - timeout de 5 segundos
                            // El timeout genera heartbeat.
                            using (CancellationTokenSource tokenHeartbeat =
                                    CancellationTokenSource.CreateLinkedTokenSource(tokenDesconexion))
                            {
                                tokenHeartbeat.CancelAfter(TimeSpan.FromSeconds(5));

                                try
                                {
                                    mensaje = await suscriptor.EsperarMensajeAsync(tokenHeartbeat.Token)
                                                                                    .ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    // Si fue porque Postman/browser cerró:
                                    if (tokenDesconexion.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    // Si NO fue desconexión significa que
                                    // pasaron 5 segundos sin eventos.
                                    //
                                    // Enviamos heartbeat para:
                                    //
                                    // 1. mantener viva la conexión;
                                    // 2. detectar clientes desconectados.

                                    JObject heartbeat =  new JObject();

                                    heartbeat.Add("estado", "activo");
                                    heartbeat.Add("fechaHora", DateTime.Now);

                                    await EscribirEventoSse(streamSalida, "heartbeat", heartbeat.ToString(
                                                    Newtonsoft.Json.Formatting.None), tokenDesconexion)
                                                    .ConfigureAwait(false);

                                    continue;
                                }
                            }

                            // 3. EVENTO DE NEGOCIO
                            if (!String.IsNullOrEmpty(mensaje))
                            {
                                await EscribirEventoSse(streamSalida, "notificacion", mensaje, tokenDesconexion)
                                                    .ConfigureAwait(false);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Cliente cerró la conexión.
                    }
                    catch (IOException)
                    {
                        // El stream HTTP ya no está disponible.
                    }
                    catch (ObjectDisposedException)
                    {
                        // El cliente cerró el stream.
                    }
                    catch (HttpException)
                    {
                        // IIS detectó desconexión.
                    }
                    finally
                    {
                        // MUY IMPORTANTE:
                        //
                        // Quitar SIEMPRE el cliente del broker.

                        clsNotificacionSseBroker.Desuscribir(suscriptor.idSuscriptor);

                        try
                        {
                            streamSalida.Close();
                        }
                        catch
                        {
                            // Ya estaba cerrado.
                        }
                    }
                };

            // Al definir explícitamente funcionStream como Func<Task>
            // garantizamos la sobrecarga asincrónica.
            respuesta.Content = new PushStreamContent(funcionStream, new MediaTypeHeaderValue("text/event-stream"));

            // Headers necesarios para streaming.
            respuesta.Headers.CacheControl = new CacheControlHeaderValue(){ NoCache = true };

            respuesta.Headers.TryAddWithoutValidation("Pragma", "no-cache");

            return respuesta;
        }

        // MÉTODO PRIVADO
        //
        // Construye UN frame SSE completo.
        private async Task EscribirEventoSse(Stream stream, string nombreEvento, string datosJson,
                                             CancellationToken cancellationToken)
        {
            // SSE:
            // event: nombre\r\n
            // data: contenido\r\n
            // \r\n
            //
            // La línea vacía final es OBLIGATORIA para indicar
            // el final del evento.

            StringBuilder sb = new StringBuilder();

            sb.Append("event: ");
            sb.Append(nombreEvento);
            sb.Append("\r\n");
            sb.Append("data: ");
            sb.Append(datosJson.Replace("\r", "").Replace("\n", ""));
            sb.Append("\r\n\r\n");

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Escribir DIRECTAMENTE sobre el stream HTTP.
            // Ya no usamos StreamWriter.
            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);

            // Forzar envío inmediato.
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        // GET
        //
        // api/estado
        [HttpGet]
        [Route("estado")]
        public HttpResponseMessage estado()
        {
            clsApiStatus objRespuesta = new clsApiStatus();

            int clientes = clsNotificacionSseBroker.CantidadSuscriptores;
            objRespuesta.datos.Add("clientesConectados", clientes);
            objRespuesta.datos.Add("fechaHora", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            objRespuesta.statusExec = true;
            objRespuesta.msg = "Estado del canal PUSH SSE.";
            objRespuesta.ban = clientes;

            return Responder(HttpStatusCode.OK, objRespuesta);
        }

        // POST
        //
        // api/enviar
        [HttpPost]
        [Route("enviar")]
        public HttpResponseMessage enviar([FromBody] clsNotificacion modelo)
        {
            clsApiStatus objRespuesta = new clsApiStatus();

            try
            {
                if (modelo == null || String.IsNullOrWhiteSpace(modelo.titulo) ||
                    String.IsNullOrWhiteSpace(modelo.mensaje))
                {
                    objRespuesta.statusExec = false;
                    objRespuesta.msg = "Título y mensaje son obligatorios.";
                    objRespuesta.ban = -2;

                    return Responder(HttpStatusCode.BadRequest, objRespuesta);
                }

                // Valores por omisión
                if (String.IsNullOrWhiteSpace(modelo.tipo)) modelo.tipo = "INFO";
                if (String.IsNullOrWhiteSpace(modelo.nivel)) modelo.nivel = "info";
                if (String.IsNullOrWhiteSpace(modelo.usuario)) modelo.usuario = UsuarioPeticion();
                if (String.IsNullOrWhiteSpace(modelo.endpoint)) modelo.endpoint = "POST /api/enviar";

                // La fecha/hora siempre es la del servidor al momento del envío.
                modelo.fechaHora = DateTime.Now;

                int clientesNotificados = clsNotificacionSseBroker.Publicar(modelo);

                objRespuesta.statusExec = true;
                objRespuesta.msg = "Notificación PUSH enviada.";
                objRespuesta.ban = clientesNotificados;
                objRespuesta.datos.Add("clientesNotificados", clientesNotificados);

                return Responder(HttpStatusCode.OK, objRespuesta);
            }
            catch (Exception ex)
            {
                objRespuesta.statusExec = false;
                objRespuesta.msg = "Error al enviar notificación: " + ex.Message;
                objRespuesta.ban = -1;

                return Responder(HttpStatusCode.InternalServerError, objRespuesta);
            }
        }
    }
}
