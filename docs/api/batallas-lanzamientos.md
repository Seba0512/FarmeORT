# Batallas y lanzamientos — alcance de Persona B

Contrato aprobado: tabla API local del equipo, letra 1.7/1.8 y distribución de trabajo del 01/10/2026. Base `/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. Identidad exclusivamente desde `Authorization: Bearer {token}`; POST además usa `Content-Type: application/json`.

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| GET `/batallas` | Query `torneoId` obligatorio; `estado`, `ronda`, `jugadorId`, `limit`, `offset` opcionales | `BatallaResponse[]` | 200, 400, 401, 403, 404 |
| GET `/batallas/{id}` | Id de batalla en ruta, obtenido del listado o de las rondas | `BatallaDetalleResponse` | 200, 401, 403, 404 |
| GET `/batallas/{id}/lanzamientos` | Id de batalla en ruta | `LanzamientoResponse[]` ordenado por turno | 200, 401, 403, 404 |
| POST `/batallas/{id}/lanzamientos` | Id de batalla en ruta; `LanzamientoRequest { skillId: int }` obligatorio y positivo | 201 `LanzamientoResultadoResponse`, Location del historial de lanzamientos de la batalla | 201, 400, 401, 403, 404, 409 |
| POST `/batallas/{id}/fallos` | Id de batalla en ruta; `FalloRequest { ganadorId: int, comentario: string }` | 201 `BatallaDetalleResponse`, Location `/api/v1/batallas/{id}` | 201, 400, 401, 403, 404, 409 |

Consulta: Jugador, Organizador y Administrador. El Jugador ve batallas de los torneos donde tiene inscripción, aun Eliminada o Cancelada (D-55), el Organizador solo las de sus torneos y el Administrador todas. No se sustituye la inscripción por jugar una batalla concreta ni por el jugadorId del filtro. POST de lanzamientos requiere Jugador; POST de fallos requiere Organizador asignado.

Paginación por offset: valor por defecto 20, tope 100; `limit < 1` usa 20, `limit > 100` usa 100 y `offset < 0` usa 0. Filtros opcionales fuera de rango se ignoran; tipos mal formados producen 400. `torneoId` ausente o no positivo produce 400.

La respuesta del lanzamiento contiene `turno: int`, `estado: string` y `jugadorTurnoId: int?`. Después del octavo lanzamiento el estado es `EnArbitraje` y no hay siguiente jugador. El historial identifica turno, jugador, skill y fecha/hora. Las consultas conservan el historial incluso después del fallo o de la cancelación del torneo.

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

No hay endpoint para crear batallas: las crea Persona C al generar rondas. Inscripciones, cálculo ELO y la migración `AgregaInscripcionesYBatallas` ya están integrados. Esta última creó Batallas y Lanzamientos y se conserva intacta. Los tres GET están implementados en `feature/batallas-consultas`; la colección necesita las rondas de C para preparar batallas mediante la API.

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

## Consultar batallas de un torneo

GET `/api/v1/batallas`, HTTP, sin body. Roles: Jugador, Organizador y Administrador. Headers: `Authorization: Bearer {token}`, `Accept: application/json`. La identidad y el rol se obtienen de la sesión, nunca de la query. Primero se comprueba que exista el torneo y que sea visible para ese usuario; después se aplican los filtros. Un torneo Cancelado conserva su historial para los usuarios autorizados (el ranking público mantiene la regla distinta de D-45).

| Query | Tipo y reglas |
|---|---|
| `torneoId` | int obligatorio, mayor que cero |
| `estado` | string opcional: `Pendiente`, `EnArbitraje`, `Finalizada` o `Cancelada`, por nombre exacto; otro texto, distinta capitalización o número se ignoran |
| `ronda` | int opcional positivo; cero/negativo se ignoran |
| `jugadorId` | int opcional positivo; busca en ambos participantes, cero/negativo se ignoran; no concede permiso |
| `limit` | int opcional, default 20, máximo 100; menor que 1 usa 20 |
| `offset` | int opcional, default 0; negativo usa 0 |

El orden es ronda ascendente y luego id ascendente, aplicado en la consulta SQL antes de offset/limit. No hay coincidencias: 200 con `[]`. La consulta es de solo lectura, sin guardado ni cambios de ELO, turnos o inscripciones.

| Respuesta | Motivo y cuerpo |
|---|---|
| 200 | `BatallaResponse[]` |
| 400 | torneoId ausente/no positivo o tipos de query mal formados: `VALIDATION_ERROR`; la validación defensiva del servicio produce `INVALID_DATA` |
| 401 | Token ausente/inválido: `INVALID_TOKEN` |
| 403 | Rol no admitido: `FORBIDDEN`; Jugador sin inscripción en el torneo: `PLAYER_NOT_IN_TOURNAMENT`; Organizador ajeno: `NOT_TOURNAMENT_ORGANIZER` |
| 404 | Torneo inexistente: `TOURNAMENT_NOT_FOUND` |
| 500 | Error inesperado: `INTERNAL_ERROR` con mensaje fijo |

`BatallaResponse`: `id: int`, `torneoId: int`, `ronda: int`, `jugador1` y `jugador2: JugadorBatallaResponse`, `estado: string`, `jugadorTurnoId: int?`, `ganadorId: int?`. Ejemplo: GET `/api/v1/batallas?torneoId=5&estado=Finalizada&ronda=1&jugadorId=7&limit=20&offset=0`.

## Consultar el detalle de una batalla

GET `/api/v1/batallas/{id}`, HTTP, sin body ni query. `id: int` en ruta, obtenido del listado o de las rondas. Roles y headers iguales al listado. Se carga la batalla y se comprueba el permiso sobre su torneo; un jugador inscripto puede consultar también batallas de otros participantes del mismo torneo.

| Respuesta | Motivo y cuerpo |
|---|---|
| 200 | `BatallaDetalleResponse` definido arriba; incluye historial, y ganador/comentario/cambios de ELO si hubo fallo |
| 401 | Token ausente/inválido: `INVALID_TOKEN` |
| 403 | `FORBIDDEN`, `PLAYER_NOT_IN_TOURNAMENT` o `NOT_TOURNAMENT_ORGANIZER`, con las mismas reglas del listado |
| 404 | Batalla inexistente: `BATTLE_NOT_FOUND` |
| 500 | Error inesperado: `INTERNAL_ERROR` con mensaje fijo |

Antes del fallo, ganador, comentario y cambios de ELO son nulos. Una batalla Cancelada sigue siendo consultable para los usuarios autorizados. Un segmento de ruta que no es entero no coincide con la ruta y ASP.NET responde 404 antes de los filtros.

## Consultar lanzamientos de una batalla

GET `/api/v1/batallas/{id}/lanzamientos`, HTTP, sin body ni query. `id: int` en ruta, obtenido del listado o de las rondas. Roles y headers iguales al listado. La autorización se comprueba sobre el torneo de la batalla antes de construir la respuesta.

| Respuesta | Motivo y cuerpo |
|---|---|
| 200 | `LanzamientoResponse[]`, ordenado por turno ascendente; `[]` si todavía no hubo lanzamientos |
| 401 | Token ausente/inválido: `INVALID_TOKEN` |
| 403 | `FORBIDDEN`, `PLAYER_NOT_IN_TOURNAMENT` o `NOT_TOURNAMENT_ORGANIZER` |
| 404 | Batalla inexistente: `BATTLE_NOT_FOUND` |
| 500 | Error inesperado: `INTERNAL_ERROR` con mensaje fijo |

La consulta no exige Pendiente ni pertenecer a esa pareja: conserva el historial para toda la inscripción del torneo. El POST de esta ruta mantiene su autorización exclusiva de Jugador y todas sus reglas previas.

## Fixtures y ejecución de Postman de Persona B

Carpetas `Batallas` → `Lanzamientos` → `Fallos`, agregadas antes de Ranking. Las carpetas anteriores y sus scripts se conservan íntegros. El salto existente de Inscripciones a Ranking se conserva hasta que C agregue Rondas; por ahora el Runner general no ejecuta B. C debe conectar el final de Rondas con `Batallas - listado organizador → 200` y, después de B, completar rondas siguientes/finalización antes de Ranking. No se inventa un endpoint para crear batallas.

C debe guardar o coordinar estas variables mediante `pm.collectionVariables.set`, a partir de respuestas HTTP:

| Variables | Fixture requerida |
|---|---|
| `bTorneoId`, `bOrganizadorToken`, `bOtroOrganizadorToken` | Torneo con dos obligatorias y su organizador; segundo organizador ajeno |
| `bBattleId`, `bJugador1Id/Token`, `bJugador2Id/Token` | Primera batalla Pendiente y sin lanzamientos; ids corresponden a jugador1/jugador2 de su detalle |
| `bBattleAbandonoId`, `bJugador3Id/Token`, `bJugador4Id/Token` | Otra batalla Pendiente del mismo torneo, con otros dos inscriptos Aceptados |
| `bJugadorSinInscripcionToken`, `adminToken` | Jugador ajeno al torneo y Administrador autenticados |
| `bSkillObligatoria1Id`, `bSkillObligatoria2Id` | Dos skills exigidas por el torneo y declaradas por los cuatro jugadores |
| `bSkillLibre1Id`, `bSkillLibreInactivaId`, `bSkillLibreExtraId` | Tres libres declaradas por los cuatro; la segunda se desactiva **después** de declararla |
| `bSkillNoDeclaradaId` | Skill existente que no declararon los participantes de la primera batalla |

La carpeta sigue jugadorTurnoId, sin asumir el resultado del sorteo. Los primeros cuatro lanzamientos usan las dos libres (ambos pueden lanzar la misma skill), los últimos cuatro las obligatorias. Comprueba repetición, turno del rival y la obligación de reservar turnos; luego prueba fallo EnArbitraje con comentario de 10 caracteres y abandono de la otra batalla con comentario de 500. Guarda ELO previos/finales para comprobar efectos y que repetir el fallo no los modifica. El perdedor conserva acceso a detalle e historial.

Se ejecutaron las carpetas B mediante Newman sobre controllers, filtros, servicios y repositorios reales, con SQLite y fixtures manuales privadas: 82 requests y 174 assertions, sin fallos. Esa prueba no reemplaza el Runner completo sobre SQL Server con las rondas de C. Los tests SQLite cubren rollback ante concurrencia; el servicio cubre ausencia de inscripción Aceptada. No se simulan carreras modificando datos inconsistentes desde la colección.
