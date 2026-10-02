# Finalizar un torneo

Contrato: letra (finalizar torneo), especificación de la API del equipo, D-40, D-63 y D-64. Base `/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. La identidad sale solo de `Authorization: Bearer {token}`.

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| POST `/torneos/{torneoId}/finalizaciones` | Id del torneo en la ruta (sale del listado o del detalle). Sin body. | `FinalizacionResponse` | 200, 401, 403, 404, 409 |

Es una operación no CRUD modelada como sub-recurso sustantivado (`finalizaciones`), igual que la cancelación (`POST /torneos/{id}/cancelaciones`, que también responde 200 con la representación).

`FinalizacionResponse`: `torneoId: int`, `estado: string` (`Finalizado`) y `campeon` (`JugadorBatallaResponse`: `id`, `nickname`, `elo` actual, el mismo formato que los jugadores de una batalla).

Rol: el Organizador asignado al torneo o un Administrador (D-63). Otro rol recibe 403 `FORBIDDEN`; un Organizador que no es el asignado, 403 `NOT_TOURNAMENT_ORGANIZER` (D-40).

Precondiciones, en este orden:
1. El torneo existe.
2. Quien llama es el organizador asignado o un Administrador.
3. El torneo está En curso.
4. No tiene batallas sin resolver (Pendientes o En arbitraje).
5. Queda exactamente un jugador sin eliminar (con inscripción Aceptada): es el campeón.

Efectos, en una sola operación: el torneo pasa a `Finalizado` y guarda a su campeón (`Torneo.CampeonId`, 0..1). La inscripción del campeón no cambia; como el torneo ya no está vigente, deja de exigir sus skills obligatorias.

| Estado | errorCode | Cuándo |
|---|---|---|
| 401 | `INVALID_TOKEN` | Falta el token o no corresponde a una sesión activa |
| 403 | `FORBIDDEN` | Quien llama no es Organizador ni Administrador |
| 403 | `NOT_TOURNAMENT_ORGANIZER` | Es Organizador, pero no el asignado a este torneo |
| 404 | `TOURNAMENT_NOT_FOUND` | No existe el torneo |
| 409 | `TOURNAMENT_NOT_IN_PROGRESS` | El torneo está Abierto, Finalizado o Cancelado |
| 409 | `TOURNAMENT_HAS_UNRESOLVED_BATTLES` | Tiene batallas Pendientes o En arbitraje |
| 409 | `TOURNAMENT_WITHOUT_CHAMPION` | Queda más de un jugador sin eliminar: faltan rondas |

Ejemplo: `POST /api/v1/torneos/4/finalizaciones` después de fallar la final responde 200 con `{"torneoId":4,"estado":"Finalizado","campeon":{"id":7,"nickname":"aurafarmer","elo":1262}}`.
