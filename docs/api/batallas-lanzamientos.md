# Batallas y lanzamientos — alcance de Persona B

Contrato aprobado: tabla API local del equipo, letra 1.7/1.8 y distribución de trabajo del 01/10/2026. Base `/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. Identidad exclusivamente desde `Authorization: Bearer {token}`; POST además usa `Content-Type: application/json`.

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| GET `/batallas` | Query `torneoId` obligatorio; `estado`, `ronda`, `jugadorId`, `limit`, `offset` opcionales | `BatallaResponse[]` | 200, 400, 401, 403, 404 |
| GET `/batallas/{id}` | Id de batalla en ruta, obtenido del listado o de las rondas | `BatallaDetalleResponse` | 200, 401, 403, 404 |
| GET `/batallas/{id}/lanzamientos` | Id de batalla en ruta | `LanzamientoResponse[]` ordenado por turno | 200, 401, 403, 404 |
| POST `/batallas/{id}/lanzamientos` | Id de batalla en ruta; `LanzamientoRequest { skillId: int }` obligatorio y positivo | 201 `LanzamientoResultadoResponse`, Location del historial de lanzamientos de la batalla | 201, 400, 401, 403, 404, 409 |
| POST `/batallas/{id}/fallos` | Id de batalla en ruta; `FalloRequest { ganadorId: int, comentario: string }` | 201 `BatallaDetalleResponse`, Location `/api/v1/batallas/{id}` | 201, 400, 401, 403, 404, 409 |

Consulta: Jugador, Organizador y Administrador. El Jugador ve batallas de los torneos donde participa, el Organizador solo las de sus torneos y el Administrador todas. Esta comprobación requiere las inscripciones de Persona A; no se sustituye por participar en una batalla concreta. POST requiere rol Jugador.

Paginación por offset: valor por defecto 20, tope 100; `limit < 1` usa 20, `limit > 100` usa 100 y `offset < 0` usa 0. Filtros opcionales fuera de rango se ignoran; tipos mal formados producen 400. `torneoId` ausente o no positivo produce 400.

La respuesta del lanzamiento contiene `turno: int`, `estado: string` y `jugadorTurnoId: int?`. Después del octavo lanzamiento el estado es `EnArbitraje` y no hay siguiente jugador. El historial identifica turno, jugador, skill y fecha/hora. Las fichas de consultas y sus DTOs se completarán al disponer de las inscripciones, antes de implementar su visibilidad.

Errores comunes: 400 `VALIDATION_ERROR` en el borde o `INVALID_DATA` en dominio; 401 `INVALID_TOKEN`; 403 `FORBIDDEN` por rol o `NOT_TOURNAMENT_ORGANIZER` por asignación (D-40); 404 `BATTLE_NOT_FOUND`, `TOURNAMENT_NOT_FOUND`, `USER_NOT_FOUND` o `SKILL_NOT_FOUND` según el recurso inexistente.

| Conflicto de lanzamiento (409) | errorCode |
|---|---|
| Batalla fuera de Pendiente | `BATTLE_NOT_PENDING` |
| Jugador ajeno a la batalla | `PLAYER_NOT_IN_BATTLE` |
| Turno del rival, o un pedido concurrente ya ocupó el turno | `NOT_PLAYER_TURN` |
| Skill no declarada por el jugador | `SKILL_NOT_IN_REPERTOIRE` |
| Skill ya lanzada por ese jugador | `SKILL_ALREADY_THROWN` |
| Pendientes obligatorias igualan los turnos propios restantes y se intenta una libre | `MANDATORY_SKILL_PENDING` |
| Otro pedido cambió el estado antes de guardar | `BATTLE_CHANGED` |

Una skill desactivada después de declararla se puede lanzar, incluso si es obligatoria. La repetición se comprueba por jugador: ambos pueden lanzar la misma skill. La fecha del lanzamiento la genera el servidor. El guardado del lanzamiento, el turno siguiente y el eventual paso a EnArbitraje es atómico. Un conflicto concurrente requiere consultar el estado actual antes de intentar otro lanzamiento.

Ejemplo de POST: `{"skillId":3}`. Respuesta de un primer turno: `{"turno":1,"estado":"Pendiente","jugadorTurnoId":8}`. Después del octavo: `{"turno":8,"estado":"EnArbitraje","jugadorTurnoId":null}`. `Location` apunta a `/api/v1/batallas/{id}/lanzamientos`, el historial de la batalla; no se inventa un GET de lanzamiento individual.

No hay endpoint para crear batallas: las crea Persona C al generar rondas. Inscripciones, cálculo ELO y la migración `AgregaInscripcionesYBatallas` ya están integrados. Esta última creó Batallas y Lanzamientos y se conserva intacta. Los tres GET se implementan en `feature/batallas-consultas`; la colección necesita las rondas de C para preparar batallas mediante la API.

## Fallar una batalla

POST `/api/v1/batallas/{id}/fallos`, HTTP, JSON. Solo Organizador autenticado y asignado al torneo. El id de ruta se obtiene de rondas o del listado de batallas. Headers: `Authorization: Bearer {token}`, `Content-Type: application/json` y `Accept: application/json`.

`FalloRequest` exige `ganadorId` entero positivo y `comentario` no vacío de 10 a 500 caracteres, inclusive. Un comentario compuesto solo por espacios se rechaza. El dominio vuelve a validar comentario y participación; no se recorta el texto ni se cambian sus límites.

La batalla debe estar Pendiente (abandono) o EnArbitraje. El ganador debe ser uno de sus dos jugadores. Antes de modificar entidades se comprueban las reglas y se obtiene la inscripción Aceptada del perdedor. `ICalculadorElo` recibe los ELO actuales **previos** de ganador y perdedor en una única llamada. Después se actualizan ambos usuarios, se registra el fallo, la batalla pasa a Finalizada y se elimina la inscripción del perdedor. Los repositorios comparten el contexto Scoped y un único `SaveChanges` guarda todos los efectos. Repetir el fallo no recalcula ELO y devuelve 409. Una carrera sobre el estado o el ELO devuelve `BATTLE_CHANGED` y revierte el guardado completo.

| Respuesta | Motivo y cuerpo |
|---|---|
| 201 | `BatallaDetalleResponse`; `Location: /api/v1/batallas/{id}` |
| 400 | Body ausente, mal formado, ganador faltante/no positivo o comentario inválido: `VALIDATION_ERROR` en el borde o `INVALID_DATA` al validar en dominio |
| 401 | Token ausente o inválido: `INVALID_TOKEN` |
| 403 | Rol distinto de Organizador: `FORBIDDEN`; organizador ajeno al torneo: `NOT_TOURNAMENT_ORGANIZER` (D-40) |
| 404 | Batalla inexistente: `BATTLE_NOT_FOUND` |
| 409 | Finalizada/Cancelada: `BATTLE_NOT_JUDGEABLE`; ganador ajeno: `PLAYER_NOT_IN_BATTLE`; perdedor sin inscripción Aceptada: `REGISTRATION_NOT_ACTIVE`; conflicto concurrente: `BATTLE_CHANGED` |
| 500 | Fallo inesperado: `INTERNAL_ERROR`, con mensaje fijo sin datos internos |

### Objetos de respuesta

`JugadorBatallaResponse`: `id: int`, `nickname: string`, `elo: int` (ELO actual).

`LanzamientoResponse`: `turno: int`, `jugadorId: int`, `nickname: string`, `skillId: int`, `skill: string` (nombre) y `fecha: DateTimeOffset` generada por el servidor. El historial se ordena por turno ascendente.

`BatallaDetalleResponse`: `id: int`, `torneoId: int`, `ronda: int`, `jugador1` y `jugador2` (`JugadorBatallaResponse`), `estado: string`, `jugadorTurnoId: int?`, `ganadorId: int?`, `comentario: string?`, `cambioEloJugador1: int?`, `cambioEloJugador2: int?` y `lanzamientos: LanzamientoResponse[]`. Los datos del fallo son nulos mientras no haya fallo. Los cambios de ELO corresponden a esa batalla, se guardan al fallarla y permanecen aunque el ELO actual cambie en otra ronda. Tras el fallo no hay siguiente turno.

Ejemplo de body: `{"ganadorId":7,"comentario":"Victoria por abandono."}`. Con dos ELO previos de 1200 y la política base, el ganador queda en 1216 y el perdedor en 1184. El detalle conserva los cambios +16/-16 y la inscripción del perdedor queda Eliminada.
