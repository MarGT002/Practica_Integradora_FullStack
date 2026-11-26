<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="wsReservacionesFull.Login" Async="true" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Acceso · Sistema de Reservaciones</title>
    <link rel="icon" type="image/svg+xml" href="~/imagenes/logo.svg" />
    <link href="~/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <link href="~/bootstrap/icons/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="~/css/sitio.css" rel="stylesheet" />
</head>
<body class="login-fondo">
    <form id="form1" runat="server" class="d-flex flex-grow-1 align-items-center justify-content-center p-3">
        <div class="login-caja row g-0 bg-white w-100">

            <!-- Imagen -->
            <div class="col-md-6 login-imagen d-none d-md-flex">
                <asp:Image ID="imgLogin" runat="server" ImageUrl="~/imagenes/login.svg" AlternateText="Edificio y calendario de reservaciones" />
            </div>

            <!-- Formulario -->
            <div class="col-md-6 p-4 p-lg-5">
                <div class="d-flex align-items-center gap-2 mb-4">
                    <asp:Image ID="imgLogo" runat="server" ImageUrl="~/imagenes/logo.svg" Width="48" Height="48" AlternateText="Logotipo" />
                    <div>
                        <h1 class="h4 mb-0 fw-bold" style="color:#0f2744">Sistema de Reservaciones</h1>
                        <small class="text-muted">Reserva de espacios académicos</small>
                    </div>
                </div>

                <h2 class="h5 fw-semibold mb-3">Iniciar sesión</h2>

                <asp:Literal ID="litMensaje" runat="server"></asp:Literal>

                <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnAcceder">
                    <div class="mb-3">
                        <label for="<%= txtUsuario.ClientID %>" class="form-label">Usuario</label>
                        <div class="input-group">
                            <span class="input-group-text"><i class="bi bi-person"></i></span>
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="50"
                                placeholder="Ej. lmendez" autocomplete="username"></asp:TextBox>
                        </div>
                    </div>

                    <div class="mb-4">
                        <label for="<%= txtPassword.ClientID %>" class="form-label">Contraseña</label>
                        <div class="input-group">
                            <span class="input-group-text"><i class="bi bi-lock"></i></span>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"
                                MaxLength="100" placeholder="Contraseña" autocomplete="current-password"></asp:TextBox>
                        </div>
                    </div>

                    <asp:Button ID="btnAcceder" runat="server" Text="Acceder" CssClass="btn btn-primary w-100 py-2"
                        OnClick="btnAcceder_Click" />
                </asp:Panel>

                <p class="text-muted small mt-4 mb-0">
                    <i class="bi bi-shield-lock me-1"></i>El acceso se valida con la Web API
                    (<code>POST /api/acceso/login</code>) y genera una notificación PUSH.
                </p>
            </div>
        </div>
    </form>

    <script src="<%= ResolveUrl("~/bootstrap/js/bootstrap.bundle.min.js") %>"></script>
</body>
</html>
