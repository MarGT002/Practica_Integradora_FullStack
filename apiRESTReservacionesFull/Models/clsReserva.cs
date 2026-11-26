using System;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Modelo del proceso principal: reservas de espacios.
    /// Los nombres de los atributos coinciden con las columnas de la BD.
    /// </summary>
    public class clsReserva
    {
        // Atributos (body del POST /api/reservas)
        public int id_reserva { get; set; }
        public int id_espacio { get; set; }
        public int id_usuario { get; set; }
        public string fecha_reserva { get; set; }   // yyyy-MM-dd
        public string hora_inicio { get; set; }     // HH:mm
        public string hora_fin { get; set; }        // HH:mm
        public string motivo { get; set; }
        public string estado { get; set; }

        public clsReserva() { }

        public clsReserva(int id_espacio, string fecha_reserva, string hora_inicio, string hora_fin)
        {
            this.id_espacio = id_espacio;
            this.fecha_reserva = fecha_reserva;
            this.hora_inicio = hora_inicio;
            this.hora_fin = hora_fin;
        }

        // -----------------------------------------------------------------
        // VALIDACIONES DE FORMATO (antes de ir a la BD)
        // Regresa null si todo está bien, o el mensaje de error.
        // -----------------------------------------------------------------
        public string ValidarFormato(bool validarRegistro)
        {
            if (id_espacio <= 0)
                return "Debes indicar un espacio (id_espacio).";

            if (validarRegistro && id_usuario <= 0)
                return "Debes indicar el usuario (id_usuario).";

            DateTime fecha;
            if (string.IsNullOrWhiteSpace(fecha_reserva) ||
                !DateTime.TryParseExact(fecha_reserva, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out fecha))
                return "La fecha debe tener el formato yyyy-MM-dd.";

            TimeSpan inicio, fin;
            if (!LeerHora(hora_inicio, out inicio) || !LeerHora(hora_fin, out fin))
                return "Las horas deben tener el formato HH:mm (24 horas).";

            if (validarRegistro && string.IsNullOrWhiteSpace(motivo))
                return "El motivo de la reserva es obligatorio.";

            if (validarRegistro && motivo.Length > 180)
                return "El motivo no debe exceder 180 caracteres.";

            return null;
        }

        // Validación de negocio: hora_fin > hora_inicio
        public bool HorarioValido()
        {
            TimeSpan inicio, fin;
            return LeerHora(hora_inicio, out inicio) && LeerHora(hora_fin, out fin) && fin > inicio;
        }

        private static bool LeerHora(string valor, out TimeSpan hora)
        {
            hora = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            string[] formatos = { @"hh\:mm", @"hh\:mm\:ss", @"h\:mm" };
            return TimeSpan.TryParseExact(valor.Trim(), formatos, CultureInfo.InvariantCulture, out hora)
                   && hora >= TimeSpan.Zero && hora < TimeSpan.FromDays(1);
        }

        // -----------------------------------------------------------------
        // MÉTODOS DE PROCESO (SP de MySQL)
        // -----------------------------------------------------------------

        // Valida si el espacio está libre (2 tablas: resultado y conflictos)
        public DataSet spValidarDisponibilidad()
        {
            return clsConexion.Ejecutar(
                "CALL spValidarDisponibilidad(@idEspacio, @fecha, @horaInicio, @horaFin);",
                "spValidarDisponibilidad",
                new MySqlParameter("@idEspacio", this.id_espacio),
                new MySqlParameter("@fecha", this.fecha_reserva),
                new MySqlParameter("@horaInicio", this.hora_inicio),
                new MySqlParameter("@horaFin", this.hora_fin));
        }

        // Registra la reserva si no hay conflicto
        public DataSet spInsReserva()
        {
            return clsConexion.Ejecutar(
                "CALL spInsReserva(@idEspacio, @idUsuario, @fecha, @horaInicio, @horaFin, @motivo);",
                "spInsReserva",
                new MySqlParameter("@idEspacio", this.id_espacio),
                new MySqlParameter("@idUsuario", this.id_usuario),
                new MySqlParameter("@fecha", this.fecha_reserva),
                new MySqlParameter("@horaInicio", this.hora_inicio),
                new MySqlParameter("@horaFin", this.hora_fin),
                new MySqlParameter("@motivo", this.motivo.Trim()));
        }

        // Consulta de reservas con filtros opcionales
        public DataSet spConsultarReservas(string filtroEstado, string filtroFecha, int filtroEspacio, int filtroUsuario)
        {
            return clsConexion.Ejecutar(
                "CALL spConsultarReservas(@estado, @fecha, @idEspacio, @idUsuario);",
                "vwReservas",
                new MySqlParameter("@estado", filtroEstado ?? ""),
                new MySqlParameter("@fecha", filtroFecha ?? ""),
                new MySqlParameter("@idEspacio", filtroEspacio),
                new MySqlParameter("@idUsuario", filtroUsuario));
        }

        // Cancela una reserva activa
        public DataSet spCancelarReserva()
        {
            return clsConexion.Ejecutar(
                "CALL spCancelarReserva(@idReserva);",
                "spCancelarReserva",
                new MySqlParameter("@idReserva", this.id_reserva));
        }
    }
}
