using Newtonsoft.Json.Linq;

namespace wsReservacionesFull.Clases
{
    /// <summary>
    /// Copia del objeto de respuesta estándar de la Web API.
    /// </summary>
    public class clsApiStatus
    {
        public bool statusExec { get; set; }
        public string msg { get; set; }
        public int ban { get; set; }
        public JObject datos { get; set; }
    }
}
