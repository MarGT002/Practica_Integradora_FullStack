using System;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Representa una notificación que será enviada
    /// mediante Server-Sent Events (SSE).
    /// </summary>
    public class clsNotificacion
    {
        // Tipo de evento: LOGIN, DISPONIBILIDAD, RESERVA_REGISTRADA, CONFLICTO, etc.
        public string tipo { get; set; }
        public string titulo { get; set; }
        public string mensaje { get; set; }
        public string usuario { get; set; }

        // Endpoint que generó la notificación y bandera obtenida
        public string endpoint { get; set; }
        public int ban { get; set; }

        // Nivel visual para el frontend: success, info, warning, danger
        public string nivel { get; set; }

        public DateTime fechaHora { get; set; }

        public clsNotificacion()
        {
            fechaHora = DateTime.Now;
            nivel = "info";
        }
    }
}
