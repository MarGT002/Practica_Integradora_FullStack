using System.Data;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Modelo de espacios reservables.
    /// </summary>
    public class clsEspacio
    {
        public int id_espacio { get; set; }
        public string nombre { get; set; }
        public string ubicacion { get; set; }
        public int capacidad { get; set; }
        public string tipo { get; set; }

        // Consulta la vista de espacios activos
        public DataSet vwEspaciosActivos()
        {
            return clsConexion.Ejecutar(
                "SELECT * FROM vwEspaciosActivos ORDER BY nombre;",
                "vwEspaciosActivos");
        }
    }
}
