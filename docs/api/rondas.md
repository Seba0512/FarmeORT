# Rondas

Contrato: letra (rondas y emparejamiento), especificación de la API del equipo, D-40, D-62 y D-63. Base `/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. La identidad sale solo de `Authorization: Bearer {token}`.

Una ronda no es un recurso guardado aparte: es el grupo de batallas del torneo con el mismo número de ronda (D-62). El detalle del torneo (`GET /torneos/{id}`) trae todas las rondas en `rondas`: es el cuadro completo.

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| POST `/torneos/{torneoId}/rondas` | Id del torneo en la ruta (sale del listado o del detalle). Sin body. | 201 `RondaResponse`, `Location: /api/v1/torneos/{torneoId}/rondas/{numero}` | 201, 401, 403, 404, 409 |
| GET `/torneos/{torneoId}/rondas` | Id del torneo en la ruta. `Authorization` opcional. | `RondaResponse[]`, ordenadas por número | 200, 401, 404 |
| GET `/torneos/{torneoId}/rondas/{numero}` | Id del torneo y número de ronda en la ruta. `Authorization` opcional. | `RondaResponse` | 200, 401, 404 |

`RondaResponse`: `numero: int` y `batallas: BatallaResponse[]`, ordenadas por id. `BatallaResponse` es el mismo resumen de batalla que devuelve `GET /batallas` (persona B): `id`, `torneoId`, `ronda`, `jugador1` y `jugador2` (`id`, `nickname`, `elo` actual), `estado` (`Pendiente`, `EnArbitraje`, `Finalizada` o `Cancelada`), `jugadorTurnoId` (`null` si ya no hay turnos) y `ganadorId` (`null` hasta que la batalla se falle).

## Generar una ronda

Rol: el Organizador asignado al torneo o un Administrador (D-63). Otro rol recibe 403 `FORBIDDEN`; un Organizador que no es el asignado, 403 `NOT_TOURNAMENT_ORGANIZER` (D-40).

Primera ronda:
- El torneo tiene que estar Abierto y tener tantas inscripciones Aceptadas como su cupo.
- Empareja a los jugadores aceptados con la estrategia por ELO: de mayor a menor ELO, con desempate por id, el primero contra el último, el segundo contra el penúltimo, y así (D-62).
- Crea una batalla Pendiente por par, en la ronda 1. Empieza el jugador de mayor ELO; si empatan, decide el sorteo.
- El torneo pasa a En curso. Las batallas y el cambio de estado se guardan juntos (un solo `SaveChanges`).

Rondas siguientes (torneo En curso):
- Todas las batallas de la última ronda tienen que estar resueltas: Finalizadas (falladas) o Canceladas.
- Se empareja, con la misma estrategia, a los jugadores con inscripción todavía Aceptada: el fallo de una batalla deja Eliminada la inscripción del perdedor.
- Crea las batallas en la ronda siguiente (la última más uno). El torneo sigue En curso.
- Un torneo En curso que todavía no tiene batallas (por ejemplo, cargado a mano con los scripts de `Datos`) responde 409 `TOURNAMENT_NOT_OPEN`: sería una primera ronda, que exige el torneo Abierto y el cupo completo.
- Si queda un solo jugador sin eliminar, ya hay campeón: no se genera otra ronda y corresponde finalizar el torneo (`POST /torneos/{torneoId}/finalizaciones`).

| Estado | errorCode | Cuándo |
|---|---|---|
| 401 | `INVALID_TOKEN` | Falta el token o no corresponde a una sesión activa |
| 403 | `FORBIDDEN` | Quien llama no es Organizador ni Administrador |
| 403 | `NOT_TOURNAMENT_ORGANIZER` | Es Organizador, pero no el asignado a este torneo |
| 404 | `TOURNAMENT_NOT_FOUND` | No existe el torneo |
| 409 | `TOURNAMENT_NOT_OPEN` | El torneo está Finalizado o Cancelado, o está En curso sin batallas |
| 409 | `TOURNAMENT_NOT_FULL` | Primera ronda con menos inscripciones Aceptadas que el cupo |
| 409 | `ROUND_NOT_RESOLVED` | La última ronda todavía tiene batallas Pendientes o En arbitraje |
| 409 | `TOURNAMENT_READY_TO_FINISH` | Queda un solo jugador sin eliminar: corresponde finalizar el torneo |

## Consultar rondas

Rol: todos, incluido el anónimo. Un token enviado pero inválido responde 401 (D-36).

| Estado | errorCode | Cuándo |
|---|---|---|
| 401 | `INVALID_TOKEN` | Se envió un token que no corresponde a una sesión activa |
| 404 | `TOURNAMENT_NOT_FOUND` | No existe el torneo, o es un torneo Cancelado y quien consulta es anónimo |
| 404 | `ROUND_NOT_FOUND` | El torneo no tiene una ronda con ese número |

Ejemplo: `POST /api/v1/torneos/4/rondas` con un torneo Abierto de cupo 4 y cuatro aceptados responde 201 con `Location: /api/v1/torneos/4/rondas/1` y `{"numero":1,"batallas":[{"id":10,"torneoId":4,"ronda":1,"jugador1":{"id":7,"nickname":"aurafarmer","elo":1250},"jugador2":{"id":9,"nickname":"sixseven","elo":1180},"estado":"Pendiente","jugadorTurnoId":7,"ganadorId":null},...]}`.
