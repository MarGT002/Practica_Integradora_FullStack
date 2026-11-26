using System;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Atajo para que cada endpoint PULL publique su notificación PUSH
    /// en el canal SSE al terminar de ejecutarse.
    /// </summary>
    public static class clsNotificador
    {
        public static int Publicar(string tipo, string titulo, string mensaje,
                                   string usuario, string endpoint, int ban, string nivel)
        {
            try
            {
                clsNotificacion notificacion = new clsNotificacion();
                notificacion.tipo = tipo;
                notificacion.titulo = titulo;
                notificacion.mensaje = mensaje;
                notificacion.usuario = string.IsNullOrWhiteSpace(usuario) ? "anónimo" : usuario;
                notificacion.endpoint = endpoint;
                notificacion.ban = ban;
                notificacion.nivel = nivel;
                notificacion.fechaHora = DateTime.Now;

                return clsNotificacionSseBroker.Publicar(notificacion);
            }
            catch
            {
                // Una falla en la notificación nunca debe romper el endpoint principal.
                return 0;
            }
        }
    }
}
