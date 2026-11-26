# Guía paso a paso · Práctica Integradora (Sistema de Reservaciones)

Esta guía es para ti, no para la entrega (no la incluyas en el ZIP). Sigue las fases en orden y marca cada casilla al terminar.

**Lo que ya tienes:** EC2 con Windows Server + IIS con sus componentes, .NET Hosting Bundle y MySQL Connector/NET.
**Lo que falta y se hace aquí mismo:** abrir el puerto 80, crear RDS, la IP elástica, publicar y probar. Esos mismos pasos sirven para terminar la práctica del Censo: cuando estés en la fase 6 puedes publicar también `apiRESTCensoFull`.

---

## Fase 1 · Base de datos local (15 min)

- [ ] Abre `BD/reservaciones.sql` en MySQL Workbench o en VS Code (extensión Database Client de Weijan Chen) y ejecútalo completo.
- [ ] Verifica con `SELECT * FROM vwReservas;` (deben salir 16 filas: 15 de prueba más la de la sección de pruebas, que queda cancelada).

> Si **no tienes MySQL local**, salta directo a la fase 5 (RDS), ejecuta el script allá y en el Web.config de la API pon `conexionActiva = bdReservacionesAws`.

---

## Fase 2 · Web API `apiRESTReservacionesFull` (30 min)

1. Visual Studio → **Crear proyecto** → *Aplicación web ASP.NET (.NET Framework)*.
   - Nombre: **`apiRESTReservacionesFull`** (exacto, porque los `namespace` del código usan ese nombre).
   - Framework: **.NET Framework 4.8**.
   - Plantilla: **Vacío** y marca la casilla **Web API**.
2. Paquetes NuGet (Herramientas → Administrador de paquetes NuGet → Consola):
   ```
   Install-Package MySql.Data
   Install-Package Microsoft.AspNet.WebApi.Cors -Version 5.3.0
   ```
   (Si en `apiRESTCtrlUsuarioFull` agregaste MySql.Data de otra forma, usa la misma, pero verifica que en sus Propiedades diga **Copia local = True**.)
3. Copia los archivos de esta carpeta al proyecto:
   - `Models/*.cs` → carpeta `Models`
   - `Controllers/*.cs` → carpeta `Controllers`
   - `App_Start/WebApiConfig.cs` → **reemplaza** el que creó Visual Studio
   - En el Explorador de soluciones: botón **Mostrar todos los archivos** → clic derecho en cada archivo nuevo → **Incluir en el proyecto**.
4. `Web.config`: copia las secciones de `Web.config.AGREGAR.txt` (connectionStrings y appSettings) y pon tu contraseña de MySQL local.
5. `Web.Release.config`: reemplaza su contenido por el de este proyecto. Así, al publicar, la API usa RDS automáticamente.
6. **Ejecuta** (F5) y anota el puerto: `https://localhost:XXXXX`. Prueba en el navegador `https://localhost:XXXXX/api/espacios`.

---

## Fase 3 · Sitio web `wsReservacionesFull` (30 min)

1. Clic derecho en la solución → **Agregar → Nuevo proyecto** → *Aplicación web ASP.NET (.NET Framework)*.
   - Nombre: **`wsReservacionesFull`**, .NET Framework 4.8, plantilla **Vacío** con la casilla **Web Forms**.
2. NuGet: `Install-Package Newtonsoft.Json` (en el proyecto del sitio).
   Referencias: clic derecho en Referencias → Agregar referencia → Ensamblados → marca **System.Net.Http** (y **System.Web.Extensions** si no aparece ya; la usa el ScriptManager de Notificaciones.aspx).
3. Copia al proyecto **todo** el contenido de la carpeta `wsReservacionesFull` (páginas `.aspx` con sus `.cs` y `.designer.cs`, `Site.Master`, carpetas `Clases`, `css`, `js`, `imagenes`, `bootstrap`). Luego, con **Mostrar todos los archivos**, selecciónalos → **Incluir en el proyecto**.
4. `Web.config`: agrega las secciones de `Web.config.AGREGAR.txt` y cambia `urlApi` y `urlApiPublica` por el puerto de la API de la fase 2.
5. `Web.Release.config`: reemplaza su contenido por el de este proyecto.
6. Clic derecho en `Login.aspx` → **Establecer como página de inicio**.
7. Clic derecho en la solución → **Proyectos de inicio** → **Varios proyectos** → ambos en *Iniciar*. F5.
   - La primera vez, abre en el navegador la URL de la API para aceptar el certificado de localhost; si no, el indicador PUSH queda en "desconectado".

---

## Fase 4 · Pruebas locales (20 min)

- [ ] Postman → **Import** → `Postman/Reservaciones.postman_collection.json`.
- [ ] En las variables de la colección cambia `baseLocal` por `https://localhost:XXXXX` (puerto de la API).
- [ ] Postman → Settings → General → desactiva **SSL certificate verification** (solo para localhost).
- [ ] Abre `07a Abrir canal PUSH` en una pestaña y dale **Send** (se queda escuchando).
- [ ] Ejecuta la carpeta **LOCAL** con el **Runner** (desmarca 07a). Todas las pruebas deben salir en verde.
- [ ] Toma capturas: petición, body, respuesta y la pestaña del stream mostrando las notificaciones.
- [ ] Prueba el sitio con **dos navegadores** (o uno normal y otro en incógnito) con usuarios distintos: lo que haga uno aparece como notificación en el otro y en `Notificaciones.aspx`.

---

## Fase 5 · AWS RDS (30 min, incluye la espera)

1. Inicia el Learner Lab → consola AWS → **Aurora and RDS → Create database → Easy create**.
   MySQL · Free tier/Sandbox · identificador `dbfullstack` · usuario `admin` · contraseña `FullStack_26A.` (las del profe).
2. Cuando esté *Available*: **Modify → Additional configuration → Publicly accessible** → Apply immediately.
3. Grupo de seguridad de RDS → **Edit inbound rules**, agrega **dos** reglas MySQL/Aurora (3306):
   - Origen **Mi IP** (para VS Code y para probar desde tu laptop).
   - Origen **el grupo de seguridad de tu EC2** (escribe `sg-` y elígelo). **Sin esta regla, la API publicada en EC2 no podrá conectarse a RDS.**
4. Desde VS Code conéctate con el punto de enlace y ejecuta `BD/reservaciones.sql` completo.
5. En el Web.config de la API, pega el punto de enlace en `bdReservacionesAws`. Para probar RDS desde tu PC, cambia temporalmente `conexionActiva` a `bdReservacionesAws` y vuelve a correr la carpeta LOCAL de Postman.

---

## Fase 6 · Publicar en EC2 (40 min)

1. En Visual Studio, para **cada proyecto**: clic derecho → **Publicar** → **Carpeta** → configuración **Release** → Publicar.
   - Revisa que en la carpeta publicada de la API exista `bin\MySql.Data.dll`.
2. Comprime cada carpeta publicada y pásalas a la instancia (copiar/pegar por Escritorio remoto o arrastrar).
3. En la instancia descomprime en:
   - `C:\inetpub\wwwroot\apiReservaciones`
   - `C:\inetpub\wwwroot\wsReservaciones`
4. **IIS Manager**:
   - *Application Pools* → **Add**: `apiReservacionesPool` y `wsReservacionesPool`, ambos **.NET CLR v4.0** e **Integrated**.
   - *Sites → Default Web Site* → clic derecho → **Add Application**:
     - Alias `apiReservaciones`, ruta `C:\inetpub\wwwroot\apiReservaciones`, pool `apiReservacionesPool`.
     - Alias `wsReservaciones`, ruta `C:\inetpub\wwwroot\wsReservaciones`, pool `wsReservacionesPool`.
   - (Si ya copiaste las carpetas y aparecen como carpetas normales: clic derecho → **Convert to Application**.)
5. Dentro de la instancia, prueba en Edge: `http://localhost/apiReservaciones/api/espacios` y `http://localhost/wsReservaciones/`.

> Como el sitio y la API quedan en el mismo servidor y puerto, el canal PUSH usa la ruta relativa `/apiReservaciones` (la pone el Web.Release.config): no hace falta CORS ni escribir la IP en el código.

---

## Fase 7 · Abrir el puerto 80 y fijar la IP (15 min)

- [ ] **Nivel A (AWS):** EC2 → tu instancia → Seguridad → grupo de seguridad → Edit inbound rules → **HTTP 80, Anywhere-IPv4**.
- [ ] **Nivel B (Windows):** *Windows Defender Firewall with Advanced Security* → Inbound Rules → New Rule → Port → TCP 80 → Allow → Domain/Private/Public → nombre `HTTP 80`.
- [ ] **IP elástica:** EC2 → Elastic IPs → Allocate → Associate a tu instancia. Úsala en todo de aquí en adelante.

---

## Fase 8 · Pruebas remotas (20 min)

- [ ] En Postman cambia `baseAws` por `http://TU_IP_ELASTICA/apiReservaciones` y ejecuta la carpeta **REMOTO** con el Runner (y 07a a mano).
- [ ] En tu laptop abre `http://TU_IP_ELASTICA/wsReservaciones/`, inicia sesión y haz el flujo completo: Reservar → Validar → Registrar → intenta el mismo horario (conflicto) → revisa Reservas y Notificaciones.
- [ ] Capturas de todo. Exporta la colección (… → **Export**, v2.1) para que incluya tus URLs reales.

---

## Fase 9 · Entrega en Moodle

ZIP nombrado con los **números de control** de los integrantes, por ejemplo `20200001_20200002.zip`, con:

- [ ] Carpeta del proyecto `apiRESTReservacionesFull` (el código fuente, **sin** `packages`, `bin`, `obj` ni `.vs`).
- [ ] Carpeta del proyecto `wsReservacionesFull` (igual, sin `packages`, `bin`, `obj`).
- [ ] `BD/reservaciones.sql`.
- [ ] `Postman/…json` exportado con las pruebas locales y remotas.
- [ ] `README.md` con integrantes, URLs publicadas y números de control (edita los `[ ]`).
- [ ] Edita también `AcercaDe.aspx` con los nombres y números de control del equipo.

---

## Problemas comunes

| Síntoma | Causa y solución |
|---|---|
| `ban -1` "Unable to connect to any of the specified MySQL hosts" | RDS no es público o falta la regla 3306 (Mi IP / grupo de seguridad del EC2). En Learner Lab la IP de tu casa puede cambiar: actualiza la regla "Mi IP". |
| La API publicada responde 500 "Could not load file or assembly MySql.Data" | Falta `MySql.Data.dll` en `bin`: en la referencia, **Copia local = True**, y vuelve a publicar. |
| Indicador PUSH "desconectado" en local | `urlApiPublica` tiene otro puerto, o el navegador no ha aceptado el certificado de la API (abre la URL de la API una vez). |
| Las notificaciones llegan tarde o todas juntas en IIS | La compresión dinámica está guardando la respuesta en búfer: en IIS, en la aplicación `apiReservaciones` → *Compression* → desmarca *Enable dynamic content compression*. |
| `PUT .../cancelar` regresa 405 en IIS | Está instalado WebDAV; quítalo de los módulos de la aplicación de la API. |
| `ban 5` "fecha que ya pasó" | La regla no permite reservar en fechas anteriores a hoy (hora de México). Usa fechas futuras. |
| Después de reiniciar el lab no abre la IP | Sin IP elástica la IP pública cambia. Asocia una IP elástica (fase 7). |
| Postman local: "SSL Error" | Desactiva *SSL certificate verification* en Postman (solo local). |
