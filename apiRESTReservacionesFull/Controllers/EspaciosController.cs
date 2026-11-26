using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apiRESTReservacionesFull.Models;

namespace apiRESTReservacionesFull.Controllers
{
    [RoutePrefix("api/espacios")]
    public class EspaciosController : BaseApiController
    {
        // GET api/espacios
        // Consulta los espacios activos.
        //   ban = número de espacios (200)
        //   ban = -1 error (500)
        [HttpGet]
        [Route("")]
        public HttpResponseMessage espacios()
        {
            const string endpoint = "GET /api/espacios";
            clsApiStatus objRespuesta = new clsApiStatus();

            try
            {
                clsEspacio objEspacio = new clsEspacio();
                DataSet ds = objEspacio.vwEspaciosActivos();

                objRespuesta.statusExec = true;
                objRespuesta.ban = ds.Tables[0].Rows.Count;
                objRespuesta.msg = "Consulta de espacios activos realizada exitosamente.";
                objRespuesta.datos.Add("vwEspaciosActivos", TablaAJson(ds.Tables[0]));

                clsNotificador.Publicar("CONSULTA_ESPACIOS", "Consulta de espacios",
                    "Se consultaron " + objRespuesta.ban + " espacios activos.",
                    UsuarioPeticion(), endpoint, objRespuesta.ban, "info");

                return Responder(HttpStatusCode.OK, objRespuesta);
            }
            catch (Exception ex)
            {
                return ErrorServidor("Fallo en la consulta de espacios.", ex, endpoint);
            }
        }
    }
}
