/* =========================================================
   CANAL PUSH (Server-Sent Events) COMPARTIDO POR TODAS LAS PÁGINAS
   - Abre UNA conexión EventSource hacia  <API>/api/stream
   - Muestra un toast por cada notificación recibida
   - Actualiza el indicador del menú (●)
   - Publica eventos del navegador para que la consola PUSH
     (Notificaciones.aspx) también los muestre:
        push:estado        detail = "CONECTADO" | "CONECTANDO" | "DESCONECTADO"
        push:conexion      detail = { mensaje }
        push:heartbeat     detail = { estado, fechaHora }
        push:notificacion  detail = { tipo, titulo, mensaje, usuario, endpoint, ban, nivel, fechaHora }
   La URL de la API se define en Site.Master (window.API_PUBLICA).
   ========================================================= */
(function () {
    "use strict";

    var canal = null;
    var colores = { success: "#12a39a", danger: "#e5484d", warning: "#d98e1f", info: "#1d4e89", secondary: "#64748b" };

    function emitir(nombre, detalle) {
        document.dispatchEvent(new CustomEvent(nombre, { detail: detalle }));
    }

    function marcarIndicador(estado) {
        var punto = document.getElementById("puntoPush");
        var texto = document.getElementById("textoPush");
        if (!punto || !texto) return;
        punto.className = "punto" + (estado === "CONECTADO" ? " on" : estado === "DESCONECTADO" ? " off" : "");
        texto.innerText = estado === "CONECTADO" ? "Push en línea" : estado === "DESCONECTADO" ? "Push desconectado" : "Conectando push...";
        emitir("push:estado", estado);
    }

    function escapar(texto) {
        var div = document.createElement("div");
        div.innerText = texto == null ? "" : String(texto);
        return div.innerHTML;
    }

    function hora(fecha) {
        var d = new Date(fecha);
        return isNaN(d.getTime()) ? "" : d.toLocaleTimeString();
    }

    function mostrarToast(n) {
        var contenedor = document.getElementById("contenedorToasts");
        if (!contenedor || !window.bootstrap) return;

        var color = colores[n.nivel] || colores.info;
        var html =
            '<div class="toast toast-push" role="alert" aria-live="polite" aria-atomic="true">' +
            '  <div class="toast-header" style="background:' + color + '">' +
            '    <i class="bi bi-broadcast me-2"></i>' +
            '    <strong class="me-auto">' + escapar(n.titulo) + '</strong>' +
            '    <small>' + escapar(hora(n.fechaHora)) + '</small>' +
            '    <button type="button" class="btn-close btn-close-white ms-2" data-bs-dismiss="toast" aria-label="Cerrar"></button>' +
            '  </div>' +
            '  <div class="toast-body">' + escapar(n.mensaje) +
            '    <div class="text-muted small mt-1">' + escapar(n.tipo) + ' · ' + escapar(n.usuario) + ' · ban ' + escapar(n.ban) + '</div>' +
            '  </div>' +
            '</div>';

        var temporal = document.createElement("div");
        temporal.innerHTML = html;
        var elemento = temporal.firstChild;
        contenedor.appendChild(elemento);

        var toast = new bootstrap.Toast(elemento, { delay: 5000 });
        elemento.addEventListener("hidden.bs.toast", function () { elemento.remove(); });
        toast.show();

        // Contador en el menú
        var contador = document.getElementById("contadorPush");
        if (contador) {
            contador.innerText = (parseInt(contador.innerText || "0", 10) + 1);
            contador.classList.remove("d-none");
        }
    }

    function conectar() {
        if (!window.API_PUBLICA || !window.EventSource) return;

        marcarIndicador("CONECTANDO");
        canal = new EventSource(window.API_PUBLICA.replace(/\/$/, "") + "/api/stream");

        canal.onopen = function () { marcarIndicador("CONECTADO"); };

        canal.addEventListener("conexion", function (e) {
            marcarIndicador("CONECTADO");
            try { emitir("push:conexion", JSON.parse(e.data)); } catch (x) { }
        });

        canal.addEventListener("heartbeat", function (e) {
            try { emitir("push:heartbeat", JSON.parse(e.data)); } catch (x) { }
        });

        canal.addEventListener("notificacion", function (e) {
            var datos;
            try { datos = JSON.parse(e.data); } catch (x) { return; }
            emitir("push:notificacion", datos);
            if (!window.PUSH_SIN_TOAST) mostrarToast(datos);
        });

        // EventSource reintenta solo mientras no se llame close()
        canal.onerror = function () {
            marcarIndicador(canal.readyState === EventSource.CONNECTING ? "CONECTANDO" : "DESCONECTADO");
        };
    }

    window.addEventListener("beforeunload", function () { if (canal) canal.close(); });
    document.addEventListener("DOMContentLoaded", conectar);
})();
