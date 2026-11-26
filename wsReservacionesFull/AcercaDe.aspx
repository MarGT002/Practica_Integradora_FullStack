<%@ Page Title="Acerca de" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AcercaDe.aspx.cs" Inherits="wsReservacionesFull.AcercaDe" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="encabezado-pagina mb-4">
        <h1><i class="bi bi-info-circle me-2"></i>Acerca de</h1>
        <p>Datos académicos del equipo y del sistema.</p>
    </div>

    <div class="row g-4">
        <div class="col-lg-7">
            <div class="tarjeta mb-4">
                <div class="tarjeta-titulo"><i class="bi bi-mortarboard me-1"></i>Datos académicos</div>
                <table class="table mb-0">
                    <%-- ===== EDITA AQUÍ LOS DATOS DE TU EQUIPO ===== --%>
                    <tr><td class="text-muted" style="width:35%">Institución</td><td>Instituto Tecnológico de Pachuca</td></tr>
                    <tr><td class="text-muted">Carrera</td><td>Ingeniería en Sistemas Computacionales</td></tr>
                    <tr><td class="text-muted">Materia</td><td>Ingeniería Full Stack</td></tr>
                    <tr><td class="text-muted">Semestre</td><td>Noveno semestre</td></tr>
                    <tr><td class="text-muted">Docente</td><td>Luis Alejandro Santana Valadez</td></tr>
                    <tr><td class="text-muted">Práctica</td><td>Práctica Final Integradora · Evaluación del 3er. seguimiento</td></tr>
                    <tr><td class="text-muted">Periodo</td><td>Agosto - Diciembre 2026</td></tr>
                </table>
            </div>

            <div class="tarjeta">
                <div class="tarjeta-titulo"><i class="bi bi-people me-1"></i>Integrantes del equipo</div>
                <div class="row g-3 text-center">
                    <%-- ===== EDITA AQUÍ NOMBRES Y NÚMEROS DE CONTROL ===== --%>
                    <div class="col-sm-6 integrante">
                        <img src="imagenes/equipo.svg" alt="Integrante" class="mb-2" />
                        <div class="fw-semibold">Mario García Terrazas</div>
                        <small class="text-muted">No. de control: [00000000]</small>
                    </div>
                    <div class="col-sm-6 integrante">
                        <img src="imagenes/equipo.svg" alt="Integrante" class="mb-2" />
                        <div class="fw-semibold">José Efrén Quintanilla Morales</div>
                        <small class="text-muted">No. de control: [00000000]</small>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-5">
            <div class="tarjeta mb-4">
                <div class="tarjeta-titulo"><i class="bi bi-bullseye me-1"></i>Propósito del sistema</div>
                <p class="mb-0">
                    Registrar reservaciones de espacios académicos (aulas, laboratorios, salas y auditorios)
                    validando la disponibilidad para que <strong>nunca existan dos reservas activas del mismo
                    espacio, en la misma fecha, con horarios que se crucen</strong>, y monitorear en línea cada
                    proceso mediante notificaciones PUSH.
                </p>
            </div>

            <div class="tarjeta">
                <div class="tarjeta-titulo"><i class="bi bi-stack me-1"></i>Arquitectura</div>
                <ul class="list-group list-group-flush">
                    <li class="list-group-item"><i class="bi bi-database me-2 text-verde"></i><strong>Base de datos:</strong> MySQL en AWS RDS (tablas, vistas y procedimientos almacenados)</li>
                    <li class="list-group-item"><i class="bi bi-hdd-network me-2 text-verde"></i><strong>Backend:</strong> ASP.NET Web API (.NET Framework 4.8) · apiRESTReservacionesFull</li>
                    <li class="list-group-item"><i class="bi bi-window me-2 text-verde"></i><strong>Frontend:</strong> Sitio web ASP.NET con MasterPage y Bootstrap 5 · wsReservacionesFull</li>
                    <li class="list-group-item"><i class="bi bi-broadcast me-2 text-verde"></i><strong>PUSH:</strong> Server-Sent Events (GET /api/stream)</li>
                    <li class="list-group-item"><i class="bi bi-cloud me-2 text-verde"></i><strong>Publicación:</strong> IIS en AWS EC2 (Windows Server)</li>
                </ul>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
