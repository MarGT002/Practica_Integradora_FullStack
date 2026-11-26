<%@ Page Title="Disponibilidad" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Disponibilidad.aspx.cs" Inherits="wsReservacionesFull.Disponibilidad" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="encabezado-pagina mb-4">
        <h1><i class="bi bi-calendar2-week me-2"></i>Consulta de disponibilidad</h1>
        <p>Revisa visualmente la ocupación de un espacio en un día y valida un horario específico.</p>
    </div>

    <div class="tarjeta mb-4">
        <asp:Panel ID="pnlFiltros" runat="server" DefaultButton="btnConsultar" CssClass="row g-3 align-items-end">
            <div class="col-lg-4">
                <label class="form-label">Espacio</label>
                <asp:DropDownList ID="ddlEspacio" runat="server" CssClass="form-select"></asp:DropDownList>
            </div>
            <div class="col-sm-4 col-lg-2">
                <label class="form-label">Fecha</label>
                <asp:TextBox ID="txtFecha" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="col-sm-4 col-lg-2">
                <label class="form-label">Hora inicio <small class="text-muted">(opcional)</small></label>
                <asp:TextBox ID="txtHoraInicio" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="col-sm-4 col-lg-2">
                <label class="form-label">Hora fin <small class="text-muted">(opcional)</small></label>
                <asp:TextBox ID="txtHoraFin" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="col-lg-2 d-grid">
                <asp:Button ID="btnConsultar" runat="server" Text="Consultar" CssClass="btn btn-primary" OnClick="btnConsultar_Click" />
            </div>
        </asp:Panel>
    </div>

    <asp:Literal ID="litMensaje" runat="server"></asp:Literal>

    <asp:Panel ID="pnlResultado" runat="server" Visible="false">
        <div class="tarjeta mb-4">
            <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
                <div class="tarjeta-titulo mb-0">
                    <i class="bi bi-bar-chart-steps me-1"></i>Ocupación de
                    <asp:Label ID="lblEspacio" runat="server"></asp:Label> ·
                    <asp:Label ID="lblFecha" runat="server"></asp:Label>
                </div>
                <asp:HyperLink ID="hlReservar" runat="server" CssClass="btn btn-verde btn-sm" Visible="false">
                    <i class="bi bi-calendar-plus me-1"></i>Reservar este horario
                </asp:HyperLink>
            </div>

            <asp:Literal ID="litLineaTiempo" runat="server"></asp:Literal>
        </div>

        <div class="tarjeta">
            <div class="tarjeta-titulo"><i class="bi bi-list-ul me-1"></i>Reservas del día en este espacio</div>
            <asp:GridView ID="gvDia" runat="server" AutoGenerateColumns="false" GridLines="None"
                CssClass="table table-hover tabla-reservas mb-0" EmptyDataText="No hay reservas registradas para este día: el espacio está libre.">
                <Columns>
                    <asp:BoundField DataField="id_reserva" HeaderText="#" />
                    <asp:TemplateField HeaderText="Horario">
                        <ItemTemplate><%# Eval("hora_inicio") %> - <%# Eval("hora_fin") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="motivo" HeaderText="Motivo" />
                    <asp:BoundField DataField="nombre_completo" HeaderText="Responsable" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate><span class='badge badge-<%# Eval("estado") %>'><%# Eval("estado") %></span></ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
