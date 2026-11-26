<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Inicio.aspx.cs" Inherits="wsReservacionesFull.Inicio" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="encabezado-pagina d-flex flex-wrap justify-content-between align-items-end gap-2 mb-4">
        <div>
            <h1>Hola, <asp:Label ID="lblBienvenida" runat="server"></asp:Label></h1>
            <p>Panel general de reservaciones · sesión iniciada el <asp:Label ID="lblInicioSesion" runat="server"></asp:Label></p>
        </div>
        <a href="Reservar.aspx" class="btn btn-verde"><i class="bi bi-calendar-plus me-1"></i>Nueva reserva</a>
    </div>

    <asp:Literal ID="litMensaje" runat="server"></asp:Literal>

    <!-- Indicadores -->
    <div class="row g-3 mb-4">
        <div class="col-sm-6 col-lg-3">
            <div class="tarjeta kpi"><div class="icono bg-verde"><i class="bi bi-calendar-check"></i></div>
                <div><div class="valor"><asp:Literal ID="litActivas" runat="server" Text="0"></asp:Literal></div><div class="etiqueta">Reservas activas</div></div></div>
        </div>
        <div class="col-sm-6 col-lg-3">
            <div class="tarjeta kpi"><div class="icono bg-azul"><i class="bi bi-flag"></i></div>
                <div><div class="valor"><asp:Literal ID="litConcluidas" runat="server" Text="0"></asp:Literal></div><div class="etiqueta">Concluidas</div></div></div>
        </div>
        <div class="col-sm-6 col-lg-3">
            <div class="tarjeta kpi"><div class="icono bg-secondary"><i class="bi bi-calendar-x"></i></div>
                <div><div class="valor"><asp:Literal ID="litCanceladas" runat="server" Text="0"></asp:Literal></div><div class="etiqueta">Canceladas</div></div></div>
        </div>
        <div class="col-sm-6 col-lg-3">
            <div class="tarjeta kpi"><div class="icono bg-ambar"><i class="bi bi-building"></i></div>
                <div><div class="valor"><asp:Literal ID="litEspacios" runat="server" Text="0"></asp:Literal></div><div class="etiqueta">Espacios disponibles</div></div></div>
        </div>
    </div>

    <div class="row g-4">
        <!-- Próximas reservas -->
        <div class="col-lg-7">
            <div class="tarjeta h-100">
                <div class="tarjeta-titulo"><i class="bi bi-clock-history me-1"></i>Próximas reservas activas</div>
                <asp:GridView ID="gvProximas" runat="server" AutoGenerateColumns="false" GridLines="None"
                    CssClass="table table-hover tabla-reservas mb-0" EmptyDataText="No hay reservas activas próximas.">
                    <Columns>
                        <asp:BoundField DataField="fecha_reserva" HeaderText="Fecha" />
                        <asp:TemplateField HeaderText="Horario">
                            <ItemTemplate><%# Eval("hora_inicio") %> - <%# Eval("hora_fin") %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="espacio" HeaderText="Espacio" />
                        <asp:BoundField DataField="nombre_completo" HeaderText="Responsable" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- Proceso -->
        <div class="col-lg-5">
            <div class="tarjeta h-100">
                <div class="tarjeta-titulo"><i class="bi bi-diagram-3 me-1"></i>Proceso de reservación</div>
                <ol class="list-group list-group-numbered list-group-flush">
                    <li class="list-group-item"><strong>Inicio de sesión</strong><br /><small class="text-muted">Validación de usuario · notificación PUSH</small></li>
                    <li class="list-group-item"><strong>Selección de espacio y horario</strong><br /><small class="text-muted">Espacio, fecha, hora inicio y fin · notificación PUSH</small></li>
                    <li class="list-group-item"><strong>Validación de disponibilidad</strong><br /><small class="text-muted">Se evita el cruce con reservas activas · notificación PUSH</small></li>
                    <li class="list-group-item"><strong>Registro de la reserva</strong><br /><small class="text-muted">Se guarda en MySQL · notificación PUSH</small></li>
                </ol>
                <div class="d-flex gap-2 mt-3">
                    <a href="Disponibilidad.aspx" class="btn btn-outline-primary btn-sm"><i class="bi bi-calendar2-week me-1"></i>Ver disponibilidad</a>
                    <a href="Notificaciones.aspx" class="btn btn-outline-secondary btn-sm"><i class="bi bi-broadcast me-1"></i>Consola PUSH</a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
