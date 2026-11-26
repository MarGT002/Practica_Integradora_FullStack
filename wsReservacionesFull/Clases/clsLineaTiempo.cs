using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;

namespace wsReservacionesFull.Clases
{
    /// <summary>
    /// Genera la "consulta visual de disponibilidad": una línea de tiempo del día
    /// (07:00 a 22:00) con las reservas del espacio y el horario solicitado.
    /// </summary>
    public static class clsLineaTiempo
    {
        private const double HoraInicioDia = 7;
        private const double HoraFinDia = 22;

        public static string Generar(DataTable reservas, string horaInicio, string horaFin, bool disponible)
        {
            StringBuilder sb = new StringBuilder();

            // Escala de horas
            sb.Append("<div class='escala mb-1'>");
            for (int h = (int)HoraInicioDia; h <= (int)HoraFinDia; h += 1)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<span style='left:{0:0.##}%'>{1:00}</span>", Posicion(h), h);
            }
            sb.Append("</div>");

            sb.Append("<div class='linea-tiempo'>");

            // Reservas del día
            if (reservas != null && reservas.Columns.Contains("hora_inicio"))
            {
                foreach (DataRow fila in reservas.Rows)
                {
                    string estado = Convert.ToString(fila["estado"]);
                    string clase = estado == "Activa" ? "activa" : estado == "Cancelada" ? "cancelada" : "concluida";
                    string ini = Convert.ToString(fila["hora_inicio"]);
                    string fin = Convert.ToString(fila["hora_fin"]);
                    string texto = ini + "-" + fin + " · " + Convert.ToString(fila["motivo"]);

                    sb.Append(Bloque(clase, ini, fin, texto + " (" + estado + ", " + Convert.ToString(fila["nombre_completo"]) + ")", texto));
                }
            }

            // Horario solicitado (marco punteado)
            if (!string.IsNullOrWhiteSpace(horaInicio) && !string.IsNullOrWhiteSpace(horaFin))
            {
                sb.Append(Bloque("solicitado" + (disponible ? " libre" : ""), horaInicio, horaFin,
                                 "Horario solicitado " + horaInicio + "-" + horaFin, ""));
            }

            sb.Append("</div>");

            // Simbología
            sb.Append("<div class='d-flex flex-wrap gap-3 small text-muted mt-2'>" +
                      "<span><span class='badge' style='background:#e5484d'>&nbsp;</span> Activa (ocupado)</span>" +
                      "<span><span class='badge' style='background:#94a3b8'>&nbsp;</span> Concluida</span>" +
                      "<span><span class='badge border' style='background:#e2e8f0'>&nbsp;</span> Cancelada (no bloquea)</span>" +
                      "<span><span class='badge border border-2 border-primary bg-white'>&nbsp;</span> Horario solicitado</span>" +
                      "<span><span class='badge' style='background:#eef7f6;border:1px solid #cdeae7'>&nbsp;</span> Libre</span>" +
                      "</div>");

            return sb.ToString();
        }

        private static string Bloque(string clase, string ini, string fin, string titulo, string texto)
        {
            double a = Horas(ini), b = Horas(fin);
            if (double.IsNaN(a) || double.IsNaN(b) || b <= a)
                return "";

            double izquierda = Math.Max(0, Posicion(a));
            double ancho = Math.Max(0.8, Math.Min(100, Posicion(b)) - izquierda);

            return string.Format(CultureInfo.InvariantCulture,
                "<div class='bloque {0}' style='left:{1:0.##}%;width:{2:0.##}%' title='{3}'>{4}</div>",
                clase, izquierda, ancho, HttpUtility.HtmlAttributeEncode(titulo), HttpUtility.HtmlEncode(texto));
        }

        private static double Posicion(double horas)
        {
            return (horas - HoraInicioDia) * 100.0 / (HoraFinDia - HoraInicioDia);
        }

        private static double Horas(string hhmm)
        {
            TimeSpan t;
            if (TimeSpan.TryParse(hhmm, CultureInfo.InvariantCulture, out t))
                return t.TotalHours;
            return double.NaN;
        }
    }
}
