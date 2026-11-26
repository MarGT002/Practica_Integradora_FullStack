using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Broker central para las conexiones PUSH/SSE.
    /// Su función es:
    /// 1. Registrar conexiones.
    /// 2. Eliminar conexiones.
    /// 3. Publicar mensajes a todas las conexiones activas.
    /// </summary>
    public static class clsNotificacionSseBroker
    {
        // ---------------------------------------------------------
        // Diccionario thread-safe.
        // Key   = GUID de conexión
        // Value = cliente SSE
        // ---------------------------------------------------------
        private static readonly ConcurrentDictionary<Guid, clsSuscriptorSse> suscriptores =
                                                    new ConcurrentDictionary<Guid, clsSuscriptorSse>();
        // REGISTRAR CLIENTE
        public static clsSuscriptorSse Suscribir()
        {
            clsSuscriptorSse nuevo = new clsSuscriptorSse();

            suscriptores.TryAdd(nuevo.idSuscriptor, nuevo);

            return nuevo;
        }

        // ELIMINAR CLIENTE
        public static void Desuscribir(Guid idSuscriptor)
        {
            clsSuscriptorSse eliminado;

            if (suscriptores.TryRemove(idSuscriptor, out eliminado))
            {
                eliminado.Desactivar();
            }
        }

        // PUBLICAR NOTIFICACIÓN
        // Regresa exactamente cuántos clientes recibieron
        // la notificación en su cola.
        public static int Publicar(clsNotificacion notificacion)
        {
            string json = JsonConvert.SerializeObject(notificacion);

            int clientesNotificados = 0;

            // ToArray crea snapshot para evitar problemas
            // si alguna conexión desaparece mientras iteramos.
            foreach (clsSuscriptorSse suscriptor in suscriptores.Values.ToArray())
            {
                // Si ya no está activo, eliminarlo.
                if (!suscriptor.activo)
                {
                    Desuscribir(suscriptor.idSuscriptor);
                    continue;
                }

                bool agregado = suscriptor.AgregarMensaje(json);

                if (agregado)
                {
                    clientesNotificados++;
                }
                else
                {
                    // Si no pudo recibir información,
                    // eliminamos esa conexión.
                    Desuscribir(suscriptor.idSuscriptor);
                }
            }

            return clientesNotificados;
        }

        // NÚMERO REAL DE CLIENTES ACTIVOS
        public static int CantidadSuscriptores
        {
            get
            {
                // Primero limpiamos posibles conexiones marcadas
                // como inactivas.
                foreach (clsSuscriptorSse suscriptor in suscriptores.Values.ToArray())
                {
                    if (!suscriptor.activo)
                    {
                        Desuscribir(suscriptor.idSuscriptor);
                    }
                }

                return suscriptores.Count;
            }
        }
    }
}