<%@ Page Title="Notificaciones" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Notificaciones.aspx.cs" Inherits="wsReservacionesFull.Notificaciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ScriptManager: permite llamar métodos C# (PageMethods) desde JavaScript sin postback --%>
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>

    <div class="encabezado-pagina mb-4">
        <h1><i class="bi bi-broadcast me-2"></i>Consola de notificaciones PUSH</h1>
        <p>Monitoreo en línea (Server-Sent Events) de todos los procesos ejecutados en la Web API.</p>
    </div>

    <div class="row g-4 mb-4">
        <!-- Estado del canal -->
        <div class="col-lg-5">
            <div class="tarjeta h-100">
                <div class="tarjeta-titulo">Estado del canal SSE</div>
                <div class="mb-3"><span id="estadoConexion" class="estado-pill estado-conectando">● CONECTANDO...</span></div>

                <table class="table table-sm mb-3">
                    <tr><td class="text-muted">Endpoint</td><td><code id="urlStream"></code></td></tr>
                    <tr><td class="text-muted">Clientes conectados</td><td><strong id="clientesConectados">-</strong></td></tr>
                    <tr><td class="text-muted">Último heartbeat</td><td id="ultimoHeartbeat">-</td></tr>
                    <tr><td class="text-muted">Notificaciones recibidas</td><td><strong id="totalRecibidas">0</strong></td></tr>
                </table>

                <button type="button" class="btn btn-outline-primary" onclick="consultarEstado();">
                    <i class="bi bi-arrow-repeat me-1"></i>Consultar estado (GET /api/estado)
                </button>
                <div id="resultadoEstado" class="mt-3"></div>
            </div>
        </div>

        <!-- Envío manual -->
        <div class="col-lg-7">
            <div class="tarjeta h-100">
                <div class="tarjeta-titulo">Enviar notificación (POST /api/enviar)</div>
                <div class="row g-2 mb-2">
                    <div class="col-md-4">
                        <label class="form-label">Tipo</label>
                        <select id="txtTipo" class="form-select">
                            <option value="AVISO">AVISO</option>
                            <option value="INFO">INFO</option>
                            <option value="MANTENIMIENTO">MANTENIMIENTO</option>
                            <option value="SISTEMA">SISTEMA</option>
                        </select>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Nivel</label>
                        <select id="txtNivel" class="form-select">
                            <option value="info">info</option>
                            <option value="success">success</option>
                            <option value="warning">warning</option>
                            <option value="danger">danger</option>
                        </select>
                    </div>
                    <div class="col-md-5">
                        <label class="form-label">Título</label>
                        <input type="text" id="txtTitulo" class="form-control" maxlength="100" value="Aviso general" />
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label">Mensaje</label>
                    <textarea id="txtMensaje" class="form-control" rows="2" maxlength="300">El Auditorio Principal estará en mantenimiento el viernes.</textarea>
                </div>
                <button type="button" class="btn btn-primary" onclick="enviarNotificacion();">
                    <i class="bi bi-send me-1"></i>Enviar notificación
                </button>
                <div id="resultadoEnvio" class="mt-3"></div>
            </div>
        </div>
    </div>

    <!-- Monitor -->
    <div class="tarjeta">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <div class="tarjeta-titulo mb-0">Notificaciones recibidas en tiempo real</div>
            <div class="d-flex gap-2">
                <select id="filtroTipo" class="form-select form-select-sm" onchange="aplicarFiltro();">
                    <option value="">Todos los tipos</option>
                </select>
                <button type="button" class="btn btn-sm btn-outline-secondary" onclick="limpiarMonitor();">Limpiar</button>
            </div>
        </div>
        <div id="monitor" class="monitor">
            <div class="text-secondary small" id="monitorVacio">Esperando eventos del servidor... Abre otra pestaña e inicia sesión, valida una disponibilidad o registra una reserva.</div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript">
        // En esta página no se muestran toasts: el monitor ya muestra todo.
        window.PUSH_SIN_TOAST = true;

        var tiposVistos = {};
        var total = 0;

        document.getElementById("urlStream").innerText = (window.API_PUBLICA || "") + "/api/stream";

        function esc(t) { var d = document.createElement("div"); d.innerText = t == null ? "" : String(t); return d.innerHTML; }

        // ---- Eventos que publica js/push.js ----
        document.addEventListener("push:estado", function (e) {
            var c = document.getElementById("estadoConexion");
            if (e.detail === "CONECTADO") { c.className = "estado-pill estado-conectado"; c.innerText = "● CONECTADO"; }
            else if (e.detail === "DESCONECTADO") { c.className = "estado-pill estado-desconectado"; c.innerText = "● DESCONECTADO"; }
            else { c.className = "estado-pill estado-conectando"; c.innerText = "● CONECTANDO..."; }
        });

        document.addEventListener("push:conexion", function (e) {
            agregarEvento({ tipo: "CONEXION", titulo: "Canal SSE", mensaje: e.detail.mensaje, usuario: "sistema", endpoint: "GET /api/stream", ban: 1, nivel: "sistema", fechaHora: new Date() });
            consultarEstado();
        });

        document.addEventListener("push:heartbeat", function (e) {
            var f = new Date(e.detail.fechaHora);
            document.getElementById("ultimoHeartbeat").innerText = isNaN(f) ? e.detail.fechaHora : f.toLocaleTimeString();
        });

        document.addEventListener("push:notificacion", function (e) { agregarEvento(e.detail); });

        // ---- Monitor ----
        function agregarEvento(n) {
            var vacio = document.getElementById("monitorVacio");
            if (vacio) vacio.remove();

            total++;
            document.getElementById("totalRecibidas").innerText = total;

            if (!tiposVistos[n.tipo]) {
                tiposVistos[n.tipo] = true;
                var op = document.createElement("option");
                op.value = n.tipo; op.innerText = n.tipo;
                document.getElementById("filtroTipo").appendChild(op);
            }

            var f = new Date(n.fechaHora);
            var div = document.createElement("div");
            div.className = "evento " + (n.nivel || "info");
            div.setAttribute("data-tipo", n.tipo);
            div.innerHTML =
                '<div class="d-flex justify-content-between gap-2"><span class="tipo">[' + esc(n.tipo) + '] ' + esc(n.titulo) + '</span>' +
                '<span class="meta">' + esc(isNaN(f) ? n.fechaHora : f.toLocaleString()) + '</span></div>' +
                '<div>' + esc(n.mensaje) + '</div>' +
                '<div class="meta">usuario: ' + esc(n.usuario) + ' · endpoint: ' + esc(n.endpoint) + ' · ban: ' + esc(n.ban) + '</div>';

            var monitor = document.getElementById("monitor");
            monitor.insertBefore(div, monitor.firstChild);
            aplicarFiltro();
        }

        function aplicarFiltro() {
            var tipo = document.getElementById("filtroTipo").value;
            var items = document.querySelectorAll("#monitor .evento");
            for (var i = 0; i < items.length; i++)
                items[i].style.display = (!tipo || items[i].getAttribute("data-tipo") === tipo) ? "" : "none";
        }

        function limpiarMonitor() {
            document.getElementById("monitor").innerHTML = '<div class="text-secondary small" id="monitorVacio">Monitor limpio. Esperando eventos...</div>';
        }

        function mostrarResultado(id, ok, texto) {
            document.getElementById(id).innerHTML =
                '<div class="alert alert-' + (ok ? 'success' : 'danger') + ' py-2 mb-0">' + esc(texto) + '</div>';
        }

        // ---- GET /api/estado mediante PageMethods ----
        function consultarEstado() {
            if (typeof PageMethods === "undefined") { mostrarResultado("resultadoEstado", false, "PageMethods no está disponible."); return; }
            PageMethods.ConsultarEstado(function (resultado) {
                try {
                    var r = JSON.parse(resultado);
                    if (r.statusExec) {
                        document.getElementById("clientesConectados").innerText = r.datos.clientesConectados;
                        mostrarResultado("resultadoEstado", true, r.msg + " Clientes conectados: " + r.datos.clientesConectados);
                    } else mostrarResultado("resultadoEstado", false, r.msg);
                } catch (x) { mostrarResultado("resultadoEstado", false, "Respuesta no válida."); }
            }, function (err) { mostrarResultado("resultadoEstado", false, "Error: " + (err && err.get_message ? err.get_message() : "")); });
        }

        // ---- POST /api/enviar mediante PageMethods ----
        function enviarNotificacion() {
            var tipo = document.getElementById("txtTipo").value;
            var nivel = document.getElementById("txtNivel").value;
            var titulo = document.getElementById("txtTitulo").value;
            var mensaje = document.getElementById("txtMensaje").value;
            PageMethods.EnviarNotificacion(tipo, nivel, titulo, mensaje, function (resultado) {
                try {
                    var r = JSON.parse(resultado);
                    mostrarResultado("resultadoEnvio", r.statusExec, r.msg + (r.statusExec ? " Clientes notificados: " + r.ban : ""));
                } catch (x) { mostrarResultado("resultadoEnvio", false, "Respuesta no válida."); }
            }, function (err) { mostrarResultado("resultadoEnvio", false, "Error: " + (err && err.get_message ? err.get_message() : "")); });
        }
    </script>
</asp:Content>
