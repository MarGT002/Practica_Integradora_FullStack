-- =====================================================================================
-- INGENIERÍA FULLSTACK · PRÁCTICA FINAL INTEGRADORA
-- SISTEMA DE RESERVACIONES DE ESPACIOS
-- Script MySQL: base de datos, tablas, llaves, índices, registros de prueba,
--               vistas y procedimientos almacenados (reglas de negocio).
--
-- Compatible con MySQL 8 (local y AWS RDS). Se puede ejecutar completo varias veces:
-- elimina y vuelve a crear la base de datos.
--
-- Contraseña de TODOS los usuarios de prueba:  isc2026
-- (se guarda cifrada con SHA2-256 en la columna password_hash)
--
-- Zona horaria: los SP calculan la hora local de México (UTC-6, sin horario de verano)
-- a partir de UTC_TIMESTAMP(), así funciona igual en tu PC que en RDS (que trabaja en UTC).
-- =====================================================================================

DROP DATABASE IF EXISTS reservaciones;
CREATE DATABASE IF NOT EXISTS reservaciones;
USE reservaciones;

-- -------------------------------------------------------------------------------------
-- (1) TABLAS
-- -------------------------------------------------------------------------------------

-- Usuarios: controla el acceso al sistema
CREATE TABLE usuarios (
    id_usuario            INT AUTO_INCREMENT PRIMARY KEY,
    nombre_completo       VARCHAR(120) NOT NULL,
    correo_institucional  VARCHAR(120) NOT NULL UNIQUE,
    usuario               VARCHAR(50)  NOT NULL UNIQUE,
    password_hash         VARCHAR(255) NOT NULL,
    rol                   ENUM('Administrador','Usuario') NOT NULL DEFAULT 'Usuario',
    activo                TINYINT(1)   NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Espacios: lugares disponibles para reservar
CREATE TABLE espacios (
    id_espacio  INT AUTO_INCREMENT PRIMARY KEY,
    nombre      VARCHAR(100) NOT NULL,
    ubicacion   VARCHAR(120) NOT NULL,
    capacidad   INT          NOT NULL,
    tipo        ENUM('Aula','Laboratorio','Sala','Auditorio','Otro') NOT NULL DEFAULT 'Aula',
    activo      TINYINT(1)   NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Reservas: proceso principal (fecha y horario)
CREATE TABLE reservas (
    id_reserva      INT AUTO_INCREMENT PRIMARY KEY,
    id_espacio      INT          NOT NULL,
    id_usuario      INT          NOT NULL,
    fecha_reserva   DATE         NOT NULL,
    hora_inicio     TIME         NOT NULL,
    hora_fin        TIME         NOT NULL,
    motivo          VARCHAR(180) NOT NULL,
    estado          ENUM('Activa','Cancelada','Concluida') NOT NULL DEFAULT 'Activa',
    fecha_registro  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_reservas_espacios
        FOREIGN KEY (id_espacio) REFERENCES espacios(id_espacio)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_reservas_usuarios
        FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_horario CHECK (hora_fin > hora_inicio),
    INDEX idx_disponibilidad (id_espacio, fecha_reserva, hora_inicio, hora_fin),
    INDEX idx_estado (estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- -------------------------------------------------------------------------------------
-- (2) REGISTROS DE PRUEBA
-- -------------------------------------------------------------------------------------

-- 5 usuarios (contraseña: isc2026)
INSERT INTO usuarios (nombre_completo, correo_institucional, usuario, password_hash, rol) VALUES
('Laura Méndez Ortiz',     'laura.mendez@pachuca.tecnm.mx',   'lmendez',  SHA2('isc2026', 256), 'Administrador'),
('Carlos Ramírez Soto',    'carlos.ramirez@pachuca.tecnm.mx', 'cramirez', SHA2('isc2026', 256), 'Usuario'),
('Ana Torres Gil',         'ana.torres@pachuca.tecnm.mx',     'atorres',  SHA2('isc2026', 256), 'Usuario'),
('Jorge Hernández Cruz',   'jorge.hernandez@pachuca.tecnm.mx','jhernandez',SHA2('isc2026', 256), 'Usuario'),
('Sofía Castillo Vega',    'sofia.castillo@pachuca.tecnm.mx', 'scastillo', SHA2('isc2026', 256), 'Usuario');

-- 5 espacios activos + 1 inactivo (para comprobar que solo se listan los activos)
INSERT INTO espacios (nombre, ubicacion, capacidad, tipo, activo) VALUES
('Aula 101',                    'Edificio A, planta baja',        35, 'Aula',        1),
('Laboratorio de Cómputo 1',    'Edificio K, primer piso',        30, 'Laboratorio', 1),
('Sala de Juntas Académica',    'Edificio de Gobierno',           12, 'Sala',        1),
('Auditorio Principal',         'Edificio Central',              200, 'Auditorio',   1),
('Laboratorio de Redes',        'Edificio K, segundo piso',       25, 'Laboratorio', 1),
('Aula 204 (mantenimiento)',    'Edificio B, segundo piso',       30, 'Aula',        0);

-- 15 reservas (activas, canceladas y concluidas).
-- Las reservas 6 y 7 son contiguas (8-10 y 10-12): NO se cruzan.
-- La reserva 13 se cruza con la 6 y 7, pero está Cancelada, por eso no bloquea.
INSERT INTO reservas (id_espacio, id_usuario, fecha_reserva, hora_inicio, hora_fin, motivo, estado) VALUES
(1, 2, '2026-11-12', '08:00', '10:00', 'Clase de Programación Web',            'Concluida'),
(2, 3, '2026-11-13', '10:00', '12:00', 'Práctica de Bases de Datos',           'Concluida'),
(4, 1, '2026-11-18', '16:00', '19:00', 'Conferencia de Ingeniería de Software', 'Concluida'),
(3, 4, '2026-11-19', '09:00', '10:30', 'Reunión de academia',                  'Concluida'),
(5, 5, '2026-11-20', '12:00', '14:00', 'Configuración de routers',             'Concluida'),
(1, 2, '2026-12-01', '08:00', '10:00', 'Asesoría de residencias',              'Activa'),
(1, 3, '2026-12-01', '10:00', '12:00', 'Examen de Cálculo Vectorial',          'Activa'),
(2, 4, '2026-12-01', '09:00', '11:00', 'Taller de Git y GitHub',               'Activa'),
(3, 1, '2026-12-02', '13:00', '14:30', 'Junta de coordinación',                'Activa'),
(4, 5, '2026-12-03', '17:00', '20:00', 'Ceremonia de clausura',                'Activa'),
(5, 2, '2026-12-04', '07:00', '09:00', 'Práctica de VLANs',                    'Activa'),
(2, 3, '2026-12-05', '15:00', '17:00', 'Curso de ASP.NET Web API',             'Activa'),
(1, 4, '2026-12-01', '09:00', '11:00', 'Tutoría grupal',                       'Cancelada'),
(3, 5, '2026-12-02', '13:00', '14:00', 'Entrevista a candidatos',              'Cancelada'),
(4, 2, '2026-12-08', '10:00', '13:00', 'Foro de proyectos integradores',       'Activa');


-- -------------------------------------------------------------------------------------
-- (3) VISTAS
-- -------------------------------------------------------------------------------------

-- Espacios disponibles para reservar (solo activos)
CREATE VIEW vwEspaciosActivos
AS
    SELECT  e.id_espacio,
            e.nombre,
            e.ubicacion,
            e.capacidad,
            e.tipo,
            CONCAT(e.nombre, ' - ', e.ubicacion, ' (', e.capacidad, ' personas)') AS descripcion
    FROM    espacios e
    WHERE   e.activo = 1;

-- Reporte de reservas con datos de espacio y usuario (fechas y horas ya formateadas)
CREATE VIEW vwReservas
AS
    SELECT  r.id_reserva,
            r.id_espacio,
            e.nombre                                    AS espacio,
            e.ubicacion,
            r.id_usuario,
            u.usuario,
            u.nombre_completo,
            DATE_FORMAT(r.fecha_reserva, '%Y-%m-%d')    AS fecha_reserva,
            TIME_FORMAT(r.hora_inicio, '%H:%i')         AS hora_inicio,
            TIME_FORMAT(r.hora_fin, '%H:%i')            AS hora_fin,
            r.motivo,
            r.estado,
            DATE_FORMAT(r.fecha_registro, '%Y-%m-%d %H:%i') AS fecha_registro
    FROM    reservas r
            INNER JOIN espacios e ON e.id_espacio = r.id_espacio
            INNER JOIN usuarios u ON u.id_usuario = r.id_usuario;

-- Resumen de reservas por espacio y estado (indicadores)
CREATE VIEW vwResumenReservas
AS
    SELECT  e.id_espacio,
            e.nombre AS espacio,
            SUM(CASE WHEN r.estado = 'Activa'    THEN 1 ELSE 0 END) AS activas,
            SUM(CASE WHEN r.estado = 'Cancelada' THEN 1 ELSE 0 END) AS canceladas,
            SUM(CASE WHEN r.estado = 'Concluida' THEN 1 ELSE 0 END) AS concluidas,
            COUNT(r.id_reserva)                                     AS total
    FROM    espacios e
            LEFT JOIN reservas r ON r.id_espacio = e.id_espacio
    GROUP BY e.id_espacio, e.nombre;


-- -------------------------------------------------------------------------------------
-- (4) PROCEDIMIENTOS ALMACENADOS (REGLAS DE NEGOCIO)
-- -------------------------------------------------------------------------------------

-- ---------------------------------------------------------------
-- spValidarAcceso: valida usuario y contraseña de un usuario activo
--   ban = 1  acceso correcto (+ datos del usuario)
--   ban = 0  acceso denegado
-- ---------------------------------------------------------------
DELIMITER $$
CREATE PROCEDURE spValidarAcceso
(
    IN p_usuario   VARCHAR(50),
    IN p_password  VARCHAR(100)
)
BEGIN
    IF EXISTS ( SELECT id_usuario
                FROM   usuarios
                WHERE  usuario = p_usuario
                AND    password_hash = SHA2(p_password, 256)
                AND    activo = 1 ) THEN
        SELECT  1 AS ban,
                id_usuario,
                nombre_completo,
                correo_institucional,
                usuario,
                rol
        FROM    usuarios
        WHERE   usuario = p_usuario
        AND     password_hash = SHA2(p_password, 256)
        AND     activo = 1;
    ELSE
        SELECT 0 AS ban;
    END IF;
END $$
DELIMITER ;


-- ---------------------------------------------------------------
-- spValidarDisponibilidad: revisa si un espacio está libre en una fecha y rango de horas.
-- Dos horarios se cruzan cuando:  inicio_existente < fin_nuevo  Y  fin_existente > inicio_nuevo
-- Solo cuentan las reservas en estado 'Activa'.
--   ban = 1  disponible
--   ban = 0  conflicto (2do resultado: reservas que se cruzan)
--   ban = 2  horario inválido (hora_fin debe ser mayor que hora_inicio)
--   ban = 3  el espacio no existe o no está activo
--   ban = 5  la fecha ya pasó
-- ---------------------------------------------------------------
DELIMITER $$
CREATE PROCEDURE spValidarDisponibilidad
(
    IN p_id_espacio   INT,
    IN p_fecha        DATE,
    IN p_hora_inicio  TIME,
    IN p_hora_fin     TIME
)
BEGIN
    DECLARE v_hoy DATE;
    SET v_hoy = DATE(DATE_SUB(UTC_TIMESTAMP(), INTERVAL 6 HOUR));

    IF p_hora_fin <= p_hora_inicio THEN
        SELECT 2 AS ban, 'La hora de fin debe ser mayor que la hora de inicio.' AS mensaje;
    ELSEIF p_fecha < v_hoy THEN
        SELECT 5 AS ban, 'No se puede reservar en una fecha que ya pasó.' AS mensaje;
    ELSEIF NOT EXISTS (SELECT id_espacio FROM espacios
                       WHERE id_espacio = p_id_espacio AND activo = 1) THEN
        SELECT 3 AS ban, 'El espacio no existe o no está activo.' AS mensaje;
    ELSEIF EXISTS (SELECT id_reserva FROM reservas
                   WHERE  id_espacio    = p_id_espacio
                   AND    fecha_reserva = p_fecha
                   AND    estado        = 'Activa'
                   AND    hora_inicio   < p_hora_fin
                   AND    hora_fin      > p_hora_inicio) THEN
        SELECT 0 AS ban, 'El espacio NO está disponible: el horario se cruza con otra reserva activa.' AS mensaje;

        SELECT  id_reserva, espacio, fecha_reserva, hora_inicio, hora_fin, nombre_completo, motivo
        FROM    vwReservas
        WHERE   id_espacio    = p_id_espacio
        AND     fecha_reserva = DATE_FORMAT(p_fecha, '%Y-%m-%d')
        AND     estado        = 'Activa'
        AND     hora_inicio   < TIME_FORMAT(p_hora_fin, '%H:%i')
        AND     hora_fin      > TIME_FORMAT(p_hora_inicio, '%H:%i')
        ORDER BY hora_inicio;
    ELSE
        SELECT 1 AS ban, 'El espacio está disponible en el horario solicitado.' AS mensaje;
    END IF;
END $$
DELIMITER ;


-- ---------------------------------------------------------------
-- spInsReserva: registra una reserva solo si pasa todas las validaciones.
-- Usa una transacción con SELECT ... FOR UPDATE para que dos peticiones
-- simultáneas no puedan registrar el mismo horario.
--   ban = 0  reserva registrada (+ id_reserva)
--   ban = 1  conflicto de horario con una reserva activa
--   ban = 2  horario inválido
--   ban = 3  el espacio no existe o no está activo
--   ban = 4  el usuario no existe o no está activo
--   ban = 5  la fecha ya pasó
-- ---------------------------------------------------------------
DELIMITER $$
CREATE PROCEDURE spInsReserva
(
    IN p_id_espacio   INT,
    IN p_id_usuario   INT,
    IN p_fecha        DATE,
    IN p_hora_inicio  TIME,
    IN p_hora_fin     TIME,
    IN p_motivo       VARCHAR(180)
)
BEGIN
    DECLARE v_hoy        DATE;
    DECLARE v_conflictos INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    SET v_hoy = DATE(DATE_SUB(UTC_TIMESTAMP(), INTERVAL 6 HOUR));

    IF p_hora_fin <= p_hora_inicio THEN
        SELECT 2 AS ban, 'La hora de fin debe ser mayor que la hora de inicio.' AS mensaje, 0 AS id_reserva;
    ELSEIF p_fecha < v_hoy THEN
        SELECT 5 AS ban, 'No se puede reservar en una fecha que ya pasó.' AS mensaje, 0 AS id_reserva;
    ELSEIF NOT EXISTS (SELECT id_espacio FROM espacios
                       WHERE id_espacio = p_id_espacio AND activo = 1) THEN
        SELECT 3 AS ban, 'El espacio no existe o no está activo.' AS mensaje, 0 AS id_reserva;
    ELSEIF NOT EXISTS (SELECT id_usuario FROM usuarios
                       WHERE id_usuario = p_id_usuario AND activo = 1) THEN
        SELECT 4 AS ban, 'El usuario no existe o no está activo.' AS mensaje, 0 AS id_reserva;
    ELSE
        START TRANSACTION;

        SELECT  COUNT(*) INTO v_conflictos
        FROM    reservas
        WHERE   id_espacio    = p_id_espacio
        AND     fecha_reserva = p_fecha
        AND     estado        = 'Activa'
        AND     hora_inicio   < p_hora_fin
        AND     hora_fin      > p_hora_inicio
        FOR UPDATE;

        IF v_conflictos > 0 THEN
            ROLLBACK;
            SELECT 1 AS ban, 'Conflicto: el horario se cruza con otra reserva activa del mismo espacio.' AS mensaje, 0 AS id_reserva;
        ELSE
            INSERT INTO reservas (id_espacio, id_usuario, fecha_reserva, hora_inicio, hora_fin, motivo, estado)
            VALUES (p_id_espacio, p_id_usuario, p_fecha, p_hora_inicio, p_hora_fin, p_motivo, 'Activa');

            SELECT 0 AS ban, 'Reserva registrada correctamente.' AS mensaje, LAST_INSERT_ID() AS id_reserva;
            COMMIT;
        END IF;
    END IF;
END $$
DELIMITER ;


-- ---------------------------------------------------------------
-- spConsultarReservas: primero marca como 'Concluida' las reservas activas cuyo
-- horario ya terminó y después regresa el listado con filtros opcionales
-- (NULL, '' o 0 = sin filtro).
-- ---------------------------------------------------------------
DELIMITER $$
CREATE PROCEDURE spConsultarReservas
(
    IN p_estado      VARCHAR(20),
    IN p_fecha       VARCHAR(10),
    IN p_id_espacio  INT,
    IN p_id_usuario  INT
)
BEGIN
    UPDATE  reservas
    SET     estado = 'Concluida'
    WHERE   estado = 'Activa'
    AND     TIMESTAMP(fecha_reserva, hora_fin) < DATE_SUB(UTC_TIMESTAMP(), INTERVAL 6 HOUR);

    SELECT  *
    FROM    vwReservas
    WHERE   (p_estado     IS NULL OR p_estado = ''  OR estado        = p_estado)
    AND     (p_fecha      IS NULL OR p_fecha  = ''  OR fecha_reserva = p_fecha)
    AND     (p_id_espacio IS NULL OR p_id_espacio = 0 OR id_espacio  = p_id_espacio)
    AND     (p_id_usuario IS NULL OR p_id_usuario = 0 OR id_usuario  = p_id_usuario)
    ORDER BY fecha_reserva DESC, hora_inicio;
END $$
DELIMITER ;


-- ---------------------------------------------------------------
-- spCancelarReserva: cambia una reserva activa a 'Cancelada'
--   ban = 0  cancelada
--   ban = 1  la reserva no existe
--   ban = 2  la reserva no está activa
-- ---------------------------------------------------------------
DELIMITER $$
CREATE PROCEDURE spCancelarReserva
(
    IN p_id_reserva INT
)
BEGIN
    IF NOT EXISTS (SELECT id_reserva FROM reservas WHERE id_reserva = p_id_reserva) THEN
        SELECT 1 AS ban, 'La reserva no existe.' AS mensaje;
    ELSEIF NOT EXISTS (SELECT id_reserva FROM reservas
                       WHERE id_reserva = p_id_reserva AND estado = 'Activa') THEN
        SELECT 2 AS ban, 'Solo se pueden cancelar reservas activas.' AS mensaje;
    ELSE
        UPDATE reservas SET estado = 'Cancelada' WHERE id_reserva = p_id_reserva;
        SELECT 0 AS ban, 'Reserva cancelada correctamente.' AS mensaje;
    END IF;
END $$
DELIMITER ;


-- =====================================================================================
-- (5) SECCIÓN DE PRUEBAS (puedes ejecutarlas una por una para tus evidencias)
-- =====================================================================================
SELECT * FROM usuarios;
SELECT * FROM vwEspaciosActivos;
SELECT * FROM vwReservas;
SELECT * FROM vwResumenReservas;

CALL spValidarAcceso('lmendez', 'isc2026');                                  -- ban 1
CALL spValidarAcceso('lmendez', 'otra');                                     -- ban 0

CALL spValidarDisponibilidad(1, '2026-12-01', '09:00', '10:30');             -- ban 0 (se cruza con 6 y 7)
CALL spValidarDisponibilidad(1, '2026-12-01', '12:00', '13:00');             -- ban 1 (libre)
CALL spValidarDisponibilidad(1, '2026-12-01', '12:00', '11:00');             -- ban 2 (horario inválido)
CALL spValidarDisponibilidad(6, '2026-12-01', '12:00', '13:00');             -- ban 3 (espacio inactivo)

CALL spInsReserva(1, 2, '2026-12-01', '12:00', '13:00', 'Prueba de registro'); -- ban 0
CALL spInsReserva(1, 3, '2026-12-01', '12:30', '14:00', 'Prueba de cruce');    -- ban 1
CALL spInsReserva(1, 99, '2026-12-01', '15:00', '16:00', 'Usuario inexistente'); -- ban 4

CALL spConsultarReservas('', '', 0, 0);
CALL spConsultarReservas('Activa', '2026-12-01', 1, 0);
CALL spCancelarReserva(16);                                                  -- ban 0 (cancela la de prueba)
CALL spCancelarReserva(16);                                                  -- ban 2 (ya no está activa)
CALL spCancelarReserva(999);                                                 -- ban 1 (no existe)
