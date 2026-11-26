using System.Data;
using MySql.Data.MySqlClient;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Modelo para el control de acceso (login).
    /// </summary>
    public class clsAcceso
    {
        // Atributos (body del POST /api/acceso/login)
        public string usuario { get; set; }
        public string password { get; set; }

        public clsAcceso() { }

        public clsAcceso(string usuario, string password)
        {
            this.usuario = usuario;
            this.password = password;
        }

        // Ejecuta spValidarAcceso
        public DataSet spValidarAcceso()
        {
            return clsConexion.Ejecutar(
                "CALL spValidarAcceso(@usuario, @password);",
                "spValidarAcceso",
                new MySqlParameter("@usuario", this.usuario),
                new MySqlParameter("@password", this.password));
        }
    }
}
