# Batallas y lanzamientos — alcance de Persona B

Contrato aprobado: tabla API local del equipo, letra 1.7/1.8 y distribución de trabajo del 01/10/2026. Base `/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. Identidad exclusivamente desde `Authorization: Bearer {token}`; POST además usa `Content-Type: application/json`.

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| GET `/batallas` | Query `torneoId` obligatorio; `estado`, `ronda`, `jugadorId`, `limit`, `offset` opcionales | `BatallaResponse[]` | 200, 400, 401, 403, 404 |
| GET `/batallas/{id}` | Id de batalla en ruta, obtenido del listado o de las rondas | `BatallaDetalleResponse` | 200, 401, 403, 404 |
| GET `/batallas/{id}/lanzamientos` | Id de batalla en ruta | `LanzamientoResponse[]` ordenado por turno | 200, 401, 403, 404 |
| POST `/batallas/{id}/lanzamientos` | Id de batalla en ruta; `LanzamientoRequest { skillId: int }` obligatorio y positivo | 201 `LanzamientoResultadoResponse`, Location del historial de lanzamientos de la batalla | 201, 400, 401, 403, 404, 409 |

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

No hay endpoint para crear batallas: las crea Persona C al generar rondas. B incorporó las obligatorias de `Torneo` recibidas en develop `d2a5bd1` sin editar ese archivo. POST y sus reglas están implementados y probados; los tres GET, sus DTOs y la visibilidad esperan las inscripciones. La migración y las carpetas Postman de esta feature se generan al terminar, tras incorporar develop. Fallos pertenece a `feature/fallos`, después de incorporar `Inscripcion` e `ICalculadorElo` a develop.
