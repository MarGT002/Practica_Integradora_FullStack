using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apiRESTReservacionesFull.Models;

namespace apiRESTReservacionesFull.Controllers
{
    [RoutePrefix("api/acceso")]
    public class AccesoController : BaseApiController
    {
        // POST api/acceso/login
        // Body: { "usuario": "lmendez", "password": "isc2026" }
        //   ban =  1  acceso correcto (200)
        //   ban =  0  acceso denegado (401)
        //   ban = -2  datos incompletos (400)
        //   ban = -1  error de servidor / BD (500)
        [HttpPost]
        [Route("login")]
        public HttpResponseMessage login([FromBody] clsAcceso modelo)
        {
            const string endpoint = "POST /api/acceso/login";

            if (modelo == null || string.IsNullOrWhiteSpace(modelo.usuario) ||
                string.IsNullOrWhiteSpace(modelo.password))
            {
                return DatosInvalidos("Usuario y contraseña son obligatorios.", endpoint);
            }

            clsApiStatus objRespuesta = new clsApiStatus();

            try
            {
                clsAcceso objAcceso = new clsAcceso(modelo.usuario.Trim(), modelo.password);
                DataSet ds = objAcceso.spValidarAcceso();
                DataRow fila = ds.Tables[0].Rows[0];

                objRespuesta.statusExec = true;
                objRespuesta.ban = Convert.ToInt32(fila["ban"]);

                if (objRespuesta.ban == 1)
                {
                    objRespuesta.msg = "Usuario validado exitosamente.";
                    objRespuesta.datos.Add("id_usuario", Convert.ToInt32(fila["id_usuario"]));
                    objRespuesta.datos.Add("nombre_completo", fila["nombre_completo"].ToString());
                    objRespuesta.datos.Add("correo_institucional", fila["correo_institucional"].ToString());
                    objRespuesta.datos.Add("usuario", fila["usuario"].ToString());
                    objRespuesta.datos.Add("rol", fila["rol"].ToString());
                    objRespuesta.datos.Add("inicio_sesion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    clsNotificador.Publicar("LOGIN", "Inicio de sesión",
                        fila["nombre_completo"] + " inició sesión (" + fila["rol"] + ").",
                        modelo.usuario, endpoint, 1, "success");

                    return Responder(HttpStatusCode.OK, objRespuesta);
                }

                objRespuesta.msg = "Acceso denegado: usuario o contraseña incorrectos.";
                objRespuesta.datos.Add("msgData", objRespuesta.msg);

                clsNotificador.Publicar("LOGIN_FALLIDO", "Intento de acceso fallido",
                    "Intento de acceso con el usuario '" + modelo.usuario + "'.",
                    modelo.usuario, endpoint, 0, "warning");

                return Responder(HttpStatusCode.Unauthorized, objRespuesta);
            }
            catch (Exception ex)
            {
                return ErrorServidor("Error de conexión con el servicio de datos.", ex, endpoint);
            }
        }
    }
}
