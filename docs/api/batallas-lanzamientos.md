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

Errores comunes: 400 `VALIDATION_ERROR` en el borde o `INVALID_DATA` en dominio; 401 `INVALID_TOKEN`; 403 `FORBIDDEN`; 404 por batalla, torneo o skill inexistente. Los conflictos 409 del lanzamiento deben distinguir batalla no Pendiente, jugador ajeno, turno incorrecto, skill fuera del repertorio, repetición del mismo jugador y obligación pendiente. La inactivación posterior de una skill declarada no impide lanzarla. Los códigos específicos se incorporan junto con cada ciclo TDD.

No hay endpoint para crear batallas: las crea Persona C al generar rondas. La regla de obligatorias se integra cuando esté disponible en `Torneo` sin modificar ese archivo desde esta rama. Fallos pertenece a `feature/fallos`, después de incorporar `Inscripcion` e `ICalculadorElo` a develop.
