<%@ Page Title="Reservar" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reservar.aspx.cs" Inherits="wsReservacionesFull.Reservar" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="encabezado-pagina mb-4">
        <h1><i class="bi bi-calendar-plus me-2"></i>Registrar reserva</h1>
        <p>Selecciona el espacio y el horario, valida la disponibilidad y registra tu reserva.</p>
    </div>

    <!-- Pasos del proceso principal -->
    <asp:Literal ID="litPasos" runat="server"></asp:Literal>

    <div class="row g-4 mt-1">
        <div class="col-lg-5">
            <div class="tarjeta">
                <div class="tarjeta-titulo"><i class="bi bi-pencil-square me-1"></i>Datos de la reserva</div>

                <div class="mb-3">
                    <label class="form-label">Solicitante</label>
                    <asp:TextBox ID="txtSolicitante" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label class="form-label">Espacio</label>
                    <asp:DropDownList ID="ddlEspacio" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>
                <div class="mb-3">
                    <label class="form-label">Fecha</label>
                    <asp:TextBox ID="txtFecha" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="row g-2 mb-3">
                    <div class="col-6">
                        <label class="form-label">Hora inicio</label>
                        <asp:TextBox ID="txtHoraInicio" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="col-6">
                        <label class="form-label">Hora fin</label>
                        <asp:TextBox ID="txtHoraFin" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="mb-4">
                    <label class="form-label">Motivo</label>
                    <asp:TextBox ID="txtMotivo" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"
                        MaxLength="180" placeholder="Ej. Clase de Programación Web"></asp:TextBox>
                </div>

                <div class="d-flex flex-wrap gap-2">
                    <asp:Button ID="btnValidar" runat="server" Text="1. Validar disponibilidad" CssClass="btn btn-outline-primary"
                        OnClick="btnValidar_Click" />
                    <asp:Button ID="btnRegistrar" runat="server" Text="2. Registrar reserva" CssClass="btn btn-verde"
                        OnClick="btnRegistrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" CssClass="btn btn-light border"
                        OnClick="btnLimpiar_Click" />
                </div>
            </div>
        </div>

        <div class="col-lg-7">
            <asp:Literal ID="litMensaje" runat="server"></asp:Literal>

            <asp:Panel ID="pnlVisual" runat="server" Visible="false" CssClass="tarjeta mb-4">
                <div class="tarjeta-titulo"><i class="bi bi-bar-chart-steps me-1"></i>Ocupación del espacio ese día</div>
                <asp:Literal ID="litLineaTiempo" runat="server"></asp:Literal>
            </asp:Panel>

            <asp:Panel ID="pnlConflictos" runat="server" Visible="false" CssClass="tarjeta">
                <div class="tarjeta-titulo text-danger"><i class="bi bi-exclamation-octagon me-1"></i>Reservas activas que se cruzan</div>
                <asp:GridView ID="gvConflictos" runat="server" AutoGenerateColumns="false" GridLines="None"
                    CssClass="table table-sm tabla-reservas mb-0">
                    <Columns>
                        <asp:BoundField DataField="id_reserva" HeaderText="#" />
                        <asp:TemplateField HeaderText="Horario">
                            <ItemTemplate><%# Eval("hora_inicio") %> - <%# Eval("hora_fin") %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="motivo" HeaderText="Motivo" />
                        <asp:BoundField DataField="nombre_completo" HeaderText="Responsable" />
                    </Columns>
                </asp:GridView>
            </asp:Panel>

            <asp:Panel ID="pnlAyuda" runat="server" CssClass="tarjeta">
                <div class="tarjeta-titulo"><i class="bi bi-lightbulb me-1"></i>Reglas de la reserva</div>
                <ul class="mb-0 text-muted">
                    <li>La hora de fin debe ser mayor que la hora de inicio.</li>
                    <li>No se permite registrar dos reservas <strong>activas</strong> del mismo espacio, misma fecha y horarios que se crucen.</li>
                    <li>Un horario que termina a las 10:00 y otro que inicia a las 10:00 <strong>no</strong> se cruzan.</li>
                    <li>Las reservas canceladas no bloquean el horario.</li>
                    <li>Cada paso genera una notificación PUSH visible en la consola de notificaciones.</li>
                </ul>
            </asp:Panel>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
