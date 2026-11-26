using Newtonsoft.Json.Linq;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Objeto estándar de respuesta de TODOS los endpoints.
    /// </summary>
    public class clsApiStatus
    {
        // Estado de ejecución del endpoint (true = se ejecutó sin errores técnicos)
        public bool statusExec { get; set; }

        // Descripción del resultado
        public string msg { get; set; }

        // Código (bandera) de ejecución del endpoint
        public int ban { get; set; }

        // Objeto Json para envío de datos
        public JObject datos { get; set; }

        public clsApiStatus()
        {
            datos = new JObject();
        }
    }
}
