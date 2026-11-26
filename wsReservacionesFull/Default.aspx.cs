using System;
using System.Web.UI;

namespace wsReservacionesFull
{
    // Página de entrada: envía al inicio (si no hay sesión, Inicio.aspx manda al login)
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Inicio.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
