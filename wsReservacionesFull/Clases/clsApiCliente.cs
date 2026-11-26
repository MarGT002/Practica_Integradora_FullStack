using System;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace wsReservacionesFull.Clases
{
    /// <summary>
    /// Resultado de una llamada a la Web API: código HTTP + clsApiStatus.
    /// </summary>
    public class clsRespuestaApi
    {
        public int codigoHttp { get; set; }
        public clsApiStatus status { get; set; }

        public bool Ok
        {
            get { return status != null && status.statusExec; }
        }
    }

    /// <summary>
    /// Cliente HTTP único para consumir la Web API apiRESTReservacionesFull.
    /// La URL base se configura en Web.config (llave "urlApi").
    /// Envía el usuario de la sesión en el header "X-Usuario" para que la API
    /// lo incluya en las notificaciones PUSH.
    /// </summary>
    public static class clsApiCliente
    {
        // Un solo HttpClient para todo el sitio (buena práctica en .NET Framework)
        private static readonly HttpClient cliente = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        public static string UrlApi
        {
            get { return (ConfigurationManager.AppSettings["urlApi"] ?? "").TrimEnd('/'); }
        }

        public static Task<clsRespuestaApi> GetAsync(string ruta, string usuario)
        {
            return EnviarAsync(new HttpRequestMessage(HttpMethod.Get, UrlApi + ruta), usuario);
        }

        public static Task<clsRespuestaApi> PostAsync(string ruta, object cuerpo, string usuario)
        {
            HttpRequestMessage peticion = new HttpRequestMessage(HttpMethod.Post, UrlApi + ruta);
            peticion.Content = new StringContent(JsonConvert.SerializeObject(cuerpo), Encoding.UTF8, "application/json");
            return EnviarAsync(peticion, usuario);
        }

        public static Task<clsRespuestaApi> PutAsync(string ruta, string usuario)
        {
            HttpRequestMessage peticion = new HttpRequestMessage(HttpMethod.Put, UrlApi + ruta);
            peticion.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            return EnviarAsync(peticion, usuario);
        }

        private static async Task<clsRespuestaApi> EnviarAsync(HttpRequestMessage peticion, string usuario)
        {
            clsRespuestaApi resultado = new clsRespuestaApi();

            try
            {
                if (!string.IsNullOrWhiteSpace(usuario))
                    peticion.Headers.TryAddWithoutValidation("X-Usuario", usuario);

                using (HttpResponseMessage respuesta = await cliente.SendAsync(peticion).ConfigureAwait(false))
                {
                    resultado.codigoHttp = (int)respuesta.StatusCode;
                    string cuerpo = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // La API regresa clsApiStatus también en los códigos 4xx/5xx
                    try
                    {
                        resultado.status = JsonConvert.DeserializeObject<clsApiStatus>(cuerpo);
                    }
                    catch (JsonException)
                    {
                        resultado.status = null;
                    }

                    if (resultado.status == null)
                    {
                        resultado.status = Error("La Web API respondió con HTTP " + resultado.codigoHttp + " sin un objeto clsApiStatus.");
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.codigoHttp = 0;
                resultado.status = Error("No fue posible conectar con la Web API (" + UrlApi + "): " + ex.Message);
            }

            if (resultado.status.datos == null)
                resultado.status.datos = new JObject();

            return resultado;
        }

        private static clsApiStatus Error(string mensaje)
        {
            return new clsApiStatus { statusExec = false, ban = -1, msg = mensaje, datos = new JObject() };
        }

        /// <summary>
        /// Convierte una lista JSON de datos (ej. datos["vwReservas"]) en DataTable para GridView/DropDownList.
        /// </summary>
        public static DataTable ATabla(JObject datos, string nombreLista)
        {
            if (datos == null || datos[nombreLista] == null)
                return new DataTable();

            JArray lista = datos[nombreLista] as JArray;
            if (lista == null || lista.Count == 0)
                return new DataTable();

            return JsonConvert.DeserializeObject<DataTable>(lista.ToString());
        }
    }
}
