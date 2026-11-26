using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using apiRESTReservacionesFull.Models;

namespace apiRESTReservacionesFull.Controllers
{
    /// <summary>
    /// Funciones comunes de los controladores:
    /// respuesta con código HTTP + clsApiStatus, usuario que hace la petición
    /// y conversión de DataTable a JSON.
    /// </summary>
    public abstract class BaseApiController : ApiController
    {
        // Respuesta HTTP con el código indicado y el objeto clsApiStatus en el body
        protected HttpResponseMessage Responder(HttpStatusCode codigo, clsApiStatus objRespuesta)
        {
            return Request.CreateResponse(codigo, objRespuesta);
        }

        // Usuario que ejecuta la petición (header opcional "X-Usuario").
        // El sitio web lo envía con el usuario de la sesión; en Postman se agrega en Headers.
        protected string UsuarioPeticion()
        {
            IEnumerable<string> valores;

            if (Request != null && Request.Headers.TryGetValues("X-Usuario", out valores))
            {
                string usuario = valores.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(usuario))
                    return usuario.Trim();
            }

            return "anónimo";
        }

        // Convierte una DataTable en JArray (lista JSON)
        protected static JArray TablaAJson(DataTable tabla)
        {
            if (tabla == null)
                return new JArray();

            return JArray.Parse(JsonConvert.SerializeObject(tabla));
        }

        // Respuesta estándar de error técnico (BD caída, etc.)
        protected HttpResponseMessage ErrorServidor(string mensaje, System.Exception ex, string endpoint)
        {
            clsApiStatus objRespuesta = new clsApiStatus();
            objRespuesta.statusExec = false;
            objRespuesta.ban = -1;
            objRespuesta.msg = mensaje;
            objRespuesta.datos.Add("msgData", ex.Message);

            clsNotificador.Publicar("ERROR", "Error en " + endpoint, mensaje,
                                    UsuarioPeticion(), endpoint, -1, "danger");

            return Responder(HttpStatusCode.InternalServerError, objRespuesta);
        }

        // Respuesta estándar de datos incompletos o con formato inválido
        protected HttpResponseMessage DatosInvalidos(string mensaje, string endpoint)
        {
            clsApiStatus objRespuesta = new clsApiStatus();
            objRespuesta.statusExec = true;
            objRespuesta.ban = -2;
            objRespuesta.msg = mensaje;
            objRespuesta.datos.Add("msgData", mensaje);

            clsNotificador.Publicar("DATOS_INVALIDOS", "Petición rechazada", mensaje,
                                    UsuarioPeticion(), endpoint, -2, "warning");

            return Responder(HttpStatusCode.BadRequest, objRespuesta);
        }
    }
}
