using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Representa UNA conexión SSE.
    /// Cada navegador/Postman conectado a /stream
    /// obtiene su propio objeto clsSuscriptorSse.
    /// </summary>
    public class clsSuscriptorSse
    {
        // ID único de esta conexión.
        public Guid idSuscriptor { get; private set; }

        // ---------------------------------------------------------
        // Cola individual de mensajes.
        // Si llega una notificación mientras el consumidor está
        // ocupado, ésta permanece almacenada hasta ser procesada.
        // ---------------------------------------------------------
        private readonly ConcurrentQueue<string> colaMensajes;

        // ---------------------------------------------------------
        // Señal que despierta EsperarMensajeAsync cuando
        // se agrega información a la cola.
        // ---------------------------------------------------------
        private readonly SemaphoreSlim senalMensaje;

        // 1 = activo
        // 0 = desconectado
        private int estadoActivo;

        public bool activo
        {
            get
            {
                return Volatile.Read(ref estadoActivo) == 1;
            }
        }

        // CONSTRUCTOR
        public clsSuscriptorSse()
        {
            idSuscriptor = Guid.NewGuid();
            colaMensajes = new ConcurrentQueue<string>();
            senalMensaje = new SemaphoreSlim(0);
            estadoActivo = 1;
        }

        // AGREGA UN MENSAJE A ESTE CLIENTE
        public bool AgregarMensaje(string mensaje)
        {
            if (!activo)
                return false;

            colaMensajes.Enqueue(mensaje);

            try
            {
                senalMensaje.Release();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ESPERA ASÍNCRONAMENTE UN MENSAJE
        //
        // CancellationToken permite cancelar la espera cuando:
        // 1. El cliente se desconecta.
        // 2. Se alcanza el tiempo del heartbeat.
        public async Task<string> EsperarMensajeAsync(
            CancellationToken cancellationToken)
        {
            while (activo)
            {
                await senalMensaje.WaitAsync(cancellationToken).ConfigureAwait(false);
                string mensaje;

                if (colaMensajes.TryDequeue(out mensaje))
                {
                    return mensaje;
                }
            }

            throw new OperationCanceledException();
        }

        // MARCAR CONEXIÓN COMO FINALIZADA
        public void Desactivar()
        {
            Interlocked.Exchange(ref estadoActivo, 0);
        }
    }
}