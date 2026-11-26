# Sistema de Reservaciones · Práctica Final Integradora

**Ingeniería Full Stack · Ingeniería en Sistemas Computacionales · Instituto Tecnológico de Pachuca**

## Integrantes

| Nombre | No. de control |
|---|---|
| Mario García Terrazas | [No. de control] |
| José Efrén Quintanilla Morales | [No. de control] |

**Docente:** Luis Alejandro Santana Valadez

## Propósito del sistema

Sistema compacto para reservar espacios académicos (aulas, laboratorios, salas y auditorios). Antes de registrar una reserva, valida la disponibilidad para que **nunca existan dos reservas activas del mismo espacio, en la misma fecha, con horarios que se crucen**. Cada proceso ejecutado en la Web API genera una **notificación PUSH (Server-Sent Events)** que se muestra en línea en el sitio web.

Regla de cruce: dos horarios se cruzan cuando `inicio_existente < fin_nuevo` **y** `fin_existente > inicio_nuevo`. Solo cuentan las reservas en estado `Activa` (las canceladas no bloquean, y un horario que termina a las 10:00 no se cruza con uno que inicia a las 10:00).

## Estructura de la entrega

```
BD/reservaciones.sql                 Script MySQL: BD, tablas, PK/FK, índices, 5 usuarios, 6 espacios,
                                     15 reservas, 3 vistas y 5 procedimientos almacenados
apiRESTReservacionesFull/            Web API (ASP.NET Web API, .NET Framework 4.8)
wsReservacionesFull/                 Sitio web (ASP.NET Web Forms con MasterPage + Bootstrap 5)
Postman/Reservaciones.postman_collection.json   Pruebas locales y remotas (PULL / PUSH)
```

## Endpoints de la Web API

Todas las respuestas usan el objeto `clsApiStatus` (`statusExec`, `msg`, `ban`, `datos`) y además un código HTTP claro.

| Método | Endpoint | Propósito | Banderas (`ban`) / HTTP | Push |
|---|---|---|---|---|
| POST | `/api/acceso/login` | Validar usuario y generar datos de sesión | 1 = correcto (200) · 0 = denegado (401) | LOGIN / LOGIN_FALLIDO |
| GET | `/api/espacios` | Consultar espacios activos | n = número de espacios (200) | CONSULTA_ESPACIOS |
| GET | `/api/disponibilidad?id_espacio=&fecha=&hora_inicio=&hora_fin=` | Validar disponibilidad | 1 = disponible (200) · 0 = conflicto (409) · 2 = horario inválido (400) · 3 = espacio inexistente (404) · 5 = fecha pasada (400) | DISPONIBILIDAD / CONFLICTO |
| POST | `/api/reservas` | Registrar reserva si no hay conflicto | 0 = registrada (201) · 1 = conflicto (409) · 2 = horario inválido (400) · 3 = espacio inexistente (404) · 4 = usuario inexistente (404) · 5 = fecha pasada (400) | RESERVA_REGISTRADA / CONFLICTO / RESERVA_RECHAZADA |
| GET | `/api/reservas?estado=&fecha=&id_espacio=&id_usuario=` | Consultar reservas (filtros opcionales) | n = número de reservas (200) | CONSULTA_RESERVAS |
| PUT | `/api/reservas/{id}/cancelar` | Cancelar una reserva activa (extra) | 0 = cancelada (200) · 1 = no existe (404) · 2 = no activa (409) | RESERVA_CANCELADA |
| GET | `/api/stream` | Abrir canal PUSH (SSE) | eventos `conexion`, `heartbeat`, `notificacion` | — |
| GET | `/api/estado` | Estado del canal (clientes conectados) | n = clientes (200) | — |
| POST | `/api/enviar` | Enviar una notificación PUSH | n = clientes notificados (200) | (la enviada) |

Banderas comunes: `-2` = datos incompletos o con formato inválido (400) · `-1` = error del servidor o de la base de datos (500).

## Sitio web

| Página | Descripción |
|---|---|
| `Login.aspx` | Inicio de sesión (consume `POST /api/acceso/login`) |
| `Site.Master` | Plantilla principal: menú, datos de sesión, cierre de sesión, indicador y toasts PUSH |
| `Inicio.aspx` | Indicadores y próximas reservas |
| `Reservar.aspx` | Proceso principal en 4 pasos: sesión → selección → disponibilidad → registro |
| `Disponibilidad.aspx` | Consulta visual (línea de tiempo del día) y validación de un horario |
| `Reservas.aspx` | Listado de reservas activas, canceladas y concluidas, con filtros y cancelación |
| `Notificaciones.aspx` | Consola PUSH: estado del canal, envío manual y monitor en tiempo real |
| `AcercaDe.aspx` | Datos académicos del equipo |

Todas las páginas, excepto Login, heredan de `clsPaginaProtegida`. Si no hay sesión, redirige a Login.aspx (evita el ingreso directo por URL) y no guarda las páginas en caché.

## Usuarios de prueba

Contraseña de todos: `isc2026`

| Usuario | Rol |
|---|---|
| lmendez | Administrador |
| cramirez, atorres, jhernandez, scastillo | Usuario |

## URLs publicadas (AWS)

- Sitio web: `http://[IP_ELASTICA]/wsReservaciones/`
- Web API: `http://[IP_ELASTICA]/apiReservaciones/api/espacios`
- Base de datos: AWS RDS MySQL `[endpoint].rds.amazonaws.com` · BD `reservaciones`

## Cómo correrlo en local (para el equipo)

1. **Base de datos:** ejecuta `BD/reservaciones.sql` completo en MySQL Workbench.
   Recomendado: crea un usuario para la práctica en vez de usar `root`:
   ```sql
   CREATE USER IF NOT EXISTS 'reservas'@'localhost' IDENTIFIED BY 'Reservas2026';
   GRANT ALL PRIVILEGES ON reservaciones.* TO 'reservas'@'localhost';
   FLUSH PRIVILEGES;
   ```
2. **API:** crea en Visual Studio el proyecto `apiRESTReservacionesFull` (ASP.NET .NET Framework 4.8, plantilla Vacío + Web API) y copia `Models`, `Controllers` y `App_Start/WebApiConfig.cs`.
   NuGet: `Install-Package MySql.Data` y `Install-Package Microsoft.AspNet.WebApi.Cors -Version 5.3.0`.
3. **Web.config de la API:** pega lo de `Web.config.AGREGAR.txt`. Ojo:
   - Solo debe existir **una** sección `<appSettings>` (si VS ya trae una vacía, bórrala).
   - La llave es `conexionActiva` con valor `bdReservaciones` (local).
   - Con MySql.Data reciente se usa `SslMode=Disabled` (no `None`).
   - Usuario/contraseña: `Uid=reservas;Pwd=Reservas2026;`
4. F5 y prueba `https://localhost:PUERTO/api/espacios` (debe regresar 5 espacios).
5. **Sitio:** crea el proyecto `wsReservacionesFull` (Vacío + Web Forms), NuGet `Install-Package Newtonsoft.Json`, referencias `System.Net.Http` y `System.Web.Extensions`, copia todo el contenido de `wsReservacionesFull/` e inclúyelo en el proyecto. En su `Web.config` pon `urlApi` y `urlApiPublica` con el puerto de la API.
6. Solución → Proyectos de inicio → Varios proyectos (ambos en Iniciar) → F5. Usuario de prueba: `lmendez` / `isc2026`.

Guía completa (incluye AWS): `GUIA_PASO_A_PASO.md`.
