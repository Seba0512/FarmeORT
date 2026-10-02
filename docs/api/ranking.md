# Ranking

Contrato: letra (ranking), especificación de la API del equipo y D-61. Base `/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. Es público: no requiere `Authorization`.

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| GET `/ranking` | Query `cantidad: int` opcional (10 por defecto) y `torneoId: int` opcional | `JugadorRankingResponse[]` | 200, 400, 404 |

`JugadorRankingResponse`: `posicion: int`, `nickname: string`, `elo: int`, `batallasGanadas: int` y `batallasPerdidas: int`.

Reglas:
- Entran solo los usuarios con rol Jugador y sin baja lógica.
- Orden: ELO de mayor a menor; si empatan, va primero el de id menor (el que se registró antes). La posición es el lugar en la lista (1, 2, 3…), así que no se repite aunque el ELO sea igual.
- `cantidad` mayor a 100 usa 100 y menor a 1 usa 10, igual que la paginación (D-35). Un valor que no es un número (`cantidad=abc`) responde 400 `VALIDATION_ERROR`.
- `batallasGanadas` y `batallasPerdidas` valen 0 hasta que se puedan fallar batallas (D-61).
- Estados: hoy, sin el filtro `torneoId`, solo responde 200 o 400. Cuando entre el filtro, un torneo inexistente va a responder 404 `TOURNAMENT_NOT_FOUND`. No hay 401, 403 ni 409: el recurso es público.

Ejemplo: `GET /api/v1/ranking?cantidad=2` responde 200 con `[{"posicion":1,"nickname":"aurafarmer","elo":1200,"batallasGanadas":0,"batallasPerdidas":0},{"posicion":2,"nickname":"sixseven","elo":1200,"batallasGanadas":0,"batallasPerdidas":0}]`: los dos tienen el mismo ELO y va primero el que se registró antes.

Pendiente: el filtro opcional `torneoId`, que se agrega junto con las rondas.
