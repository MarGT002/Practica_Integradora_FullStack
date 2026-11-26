<%@ Page Title="Reservas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reservas.aspx.cs" Inherits="wsReservacionesFull.Reservas" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="encabezado-pagina d-flex flex-wrap justify-content-between align-items-end gap-2 mb-4">
        <div>
            <h1><i class="bi bi-list-check me-2"></i>Listado de reservas</h1>
            <p>Reservas activas, canceladas y concluidas (consumo de <code>GET /api/reservas</code>).</p>
        </div>
        <a href="Reservar.aspx" class="btn btn-verde"><i class="bi bi-calendar-plus me-1"></i>Nueva reserva</a>
    </div>

    <div class="tarjeta mb-4">
        <asp:Panel ID="pnlFiltros" runat="server" DefaultButton="btnFiltrar" CssClass="row g-3 align-items-end">
            <div class="col-sm-6 col-lg-2">
                <label class="form-label">Estado</label>
                <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                    <asp:ListItem Value="">Todas</asp:ListItem>
                    <asp:ListItem Value="Activa">Activas</asp:ListItem>
                    <asp:ListItem Value="Cancelada">Canceladas</asp:ListItem>
                    <asp:ListItem Value="Concluida">Concluidas</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-sm-6 col-lg-2">
                <label class="form-label">Fecha</label>
                <asp:TextBox ID="txtFecha" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="col-lg-4">
                <label class="form-label">Espacio</label>
                <asp:DropDownList ID="ddlEspacio" runat="server" CssClass="form-select"></asp:DropDownList>
            </div>
            <div class="col-sm-6 col-lg-2">
                <div class="mb-2">
                    <asp:CheckBox ID="chkMias" runat="server" Text=" Solo mis reservas" />
                </div>
            </div>
            <div class="col-sm-6 col-lg-2 d-flex gap-2">
                <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-primary flex-fill" OnClick="btnFiltrar_Click" />
                <asp:Button ID="btnTodas" runat="server" Text="Todas" CssClass="btn btn-light border" OnClick="btnTodas_Click" />
            </div>
        </asp:Panel>
    </div>

    <asp:Literal ID="litMensaje" runat="server"></asp:Literal>

    <div class="tarjeta">
        <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
            <div class="tarjeta-titulo mb-0"><i class="bi bi-table me-1"></i>Resultados</div>
            <asp:Literal ID="litResumen" runat="server"></asp:Literal>
        </div>

        <div class="table-responsive">
            <asp:GridView ID="gvReservas" runat="server" AutoGenerateColumns="false" GridLines="None"
                CssClass="table table-hover tabla-reservas mb-0" DataKeyNames="id_reserva"
                OnRowCommand="gvReservas_RowCommand" EmptyDataText="No se encontraron reservas con esos filtros.">
                <Columns>
                    <asp:BoundField DataField="id_reserva" HeaderText="#" />
                    <asp:BoundField DataField="espacio" HeaderText="Espacio" />
                    <asp:BoundField DataField="fecha_reserva" HeaderText="Fecha" />
                    <asp:TemplateField HeaderText="Horario">
                        <ItemTemplate><%# Eval("hora_inicio") %> - <%# Eval("hora_fin") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="nombre_completo" HeaderText="Responsable" />
                    <asp:BoundField DataField="motivo" HeaderText="Motivo" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate><span class='badge badge-<%# Eval("estado") %>'><%# Eval("estado") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Acción">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkCancelar" runat="server" CommandName="Cancelar"
                                CommandArgument='<%# Eval("id_reserva") %>'
                                Visible='<%# PuedeCancelar(Eval("estado"), Eval("id_usuario")) %>'
                                CssClass="btn btn-sm btn-outline-danger"
                                OnClientClick="return confirm('¿Cancelar esta reserva? El horario quedará libre.');">
                                <i class="bi bi-x-circle"></i> Cancelar
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
