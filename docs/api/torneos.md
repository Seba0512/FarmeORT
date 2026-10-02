# Torneos — endpoints de la Persona A

Contrato: letra (gestión de torneos), especificación de la API del equipo, D-36, D-37 y D-42. Base
`/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. La identidad sale solo de
`Authorization: Bearer {token}`.

## Detalle de un torneo

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| GET `/torneos/{id}` | Id del torneo en la ruta (sale del listado). `Authorization` opcional. | `TorneoDetalleResponse` | 200, 401, 404 |

Rol: todos, incluido el anónimo. Un token enviado pero inválido responde 401 (D-36).

`TorneoDetalleResponse` tiene los campos de `TorneoResponse` y además (D-44):
- `skillsObligatorias`: `SkillResponse[]` (`id`, `codigo`, `nombre`, `descripcion`, `categoria`,
  `dificultad`, `activa`), el mismo formato que `GET /torneos/{id}/skills-obligatorias`.
- `jugadoresAceptados`: `JugadorAceptadoResponse[]` (`jugadorId`, `nickname`, `elo`), en orden de
  inscripción. Solo las inscripciones Aceptadas.
- `rondas`: `RondaResponse[]`, el cuadro completo (cada ronda con sus batallas, ambos jugadores y el ganador si lo hay), con el mismo formato que `GET /torneos/{id}/rondas` (D-62). Antes de la primera ronda viene vacío.

| Estado | errorCode | Cuándo |
|---|---|---|
| 401 | `INVALID_TOKEN` | Se envió un token que no corresponde a una sesión activa |
| 404 | `TOURNAMENT_NOT_FOUND` | No existe, o es un torneo Cancelado y quien consulta es anónimo |

Ejemplo: `GET /api/v1/torneos/4` responde 200 con
`{"id":4,"codigo":"COPAAURA",...,"inscripcionesAceptadas":1,"skillsObligatorias":[{"id":3,"codigo":"SIXSEVEN",...}],"jugadoresAceptados":[{"jugadorId":7,"nickname":"aurafarmer","elo":1200}]}`.

## Cancelar un torneo

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| POST `/torneos/{id}/cancelaciones` | Id del torneo en la ruta (sale del listado o del alta). Sin body. | `TorneoResponse` | 200, 401, 403, 404, 409 |

Rol: Administrador. Es una operación no CRUD modelada como sub-recurso sustantivado (`cancelaciones`).

`TorneoResponse`: `id`, `codigo`, `nombre`, `descripcion`, `estado`, `fechaInicio`, `fechaFin`, `cupo`,
`inscripcionesAceptadas`, `organizadorId` y `organizador` (nombre y apellido).

Efectos, en una sola operación atómica (D-42):
- El torneo pasa a `Cancelado`.
- Sus batallas `Pendiente` y `EnArbitraje` pasan a `Cancelada`. Las `Finalizada` no cambian.
- Sus inscripciones `Aceptada` pasan a `Cancelada`. Las `Eliminada` y las ya `Cancelada` no cambian.
- La respuesta muestra `estado: "Cancelado"` e `inscripcionesAceptadas: 0`.

Si algo falla, no se guarda ninguno de los cambios.

| Estado | errorCode | Cuándo |
|---|---|---|
| 401 | `INVALID_TOKEN` | Falta el token o no corresponde a una sesión activa |
| 403 | `FORBIDDEN` | El usuario no es Administrador |
| 404 | `TOURNAMENT_NOT_FOUND` | No existe un torneo con ese id |
| 409 | `TOURNAMENT_NOT_CANCELLABLE` | El torneo está Finalizado o ya Cancelado |
| 409 | `BATTLE_CHANGED` | Otra operación cambió una batalla del torneo mientras se cancelaba; se puede reintentar |

Ejemplo: `POST /api/v1/torneos/4/cancelaciones` sobre un torneo Abierto con dos inscriptos responde 200 con
`{"id":4,"codigo":"COPAAURA",...,"estado":"Cancelado","inscripcionesAceptadas":0,...}`, y
`GET /api/v1/inscripciones?torneoId=4` muestra las dos inscripciones `Cancelada`.
