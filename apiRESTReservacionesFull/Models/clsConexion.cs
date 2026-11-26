using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace apiRESTReservacionesFull.Models
{
    /// <summary>
    /// Centraliza la conexión a MySQL.
    /// La cadena que se usa se elige en Web.config con la llave "conexionActiva":
    ///   bdReservaciones     -> MySQL local
    ///   bdReservacionesAws  -> MySQL en AWS RDS
    /// </summary>
    public static class clsConexion
    {
        public static string CadenaConexion
        {
            get
            {
                string nombre = ConfigurationManager.AppSettings["conexionActiva"];

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    nombre = "bdReservaciones";
                }

                return ConfigurationManager.ConnectionStrings[nombre].ConnectionString;
            }
        }

        /// <summary>
        /// Ejecuta una instrucción SQL (CALL a un SP o SELECT a una vista) con parámetros
        /// y regresa el DataSet con todas las tablas de resultado.
        /// Los parámetros evitan inyección SQL.
        /// </summary>
        public static DataSet Ejecutar(string cadSql, string nombreTabla, params MySqlParameter[] parametros)
        {
            using (MySqlConnection cnn = new MySqlConnection(CadenaConexion))
            using (MySqlCommand cmd = new MySqlCommand(cadSql, cnn))
            {
                cmd.CommandType = CommandType.Text;

                if (parametros != null)
                {
                    cmd.Parameters.AddRange(parametros);
                }

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds, nombreTabla);
                return ds;
            }
        }
    }
}
