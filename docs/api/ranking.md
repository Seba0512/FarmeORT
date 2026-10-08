# Ranking

Contrato: letra (ranking), especificación de la API del equipo, D-61, D-45, D-46 y D-72. Base `/api/v1`, HTTP, JSON camelCase, `Accept: application/json`. Es público: `Authorization` es opcional y, si se envía, se valida (D-72).

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| GET `/rankings` | Query `cantidad: int` opcional (10 por defecto) y `torneoId: int` opcional. `Authorization` opcional. | `JugadorRankingResponse[]` | 200, 400, 401, 404 |

`JugadorRankingResponse`: `posicion: int`, `nickname: string`, `elo: int`, `batallasGanadas: int` y `batallasPerdidas: int`.

Reglas:
- Entran solo los usuarios con rol Jugador y sin baja lógica.
- Orden: ELO de mayor a menor; si empatan, va primero el de id menor (el que se registró antes). La posición es el lugar en la lista (1, 2, 3…), así que no se repite aunque el ELO sea igual.
- `cantidad` mayor a 100 usa 100 y menor a 1 usa 10, igual que la paginación (D-35). Un valor que no es un número (`cantidad=abc`) responde 400 `VALIDATION_ERROR`.
- `batallasGanadas` y `batallasPerdidas` cuentan las batallas Finalizadas del jugador (D-46): ganadas, en las que es el ganador, y perdidas, en las que el ganador es el rival. Las Pendientes, En arbitraje y Canceladas no cuentan. Son siempre las globales: con `torneoId` también cuentan las batallas de todos los torneos (foro, debate 06; D-72).
- Con `torneoId` (D-45) entran solo los jugadores con inscripción Aceptada o Eliminada en ese torneo; las Canceladas no. El ELO es el actual (global) y el orden y la cantidad son los mismos que sin filtro.
- Estados: 200; 400 `VALIDATION_ERROR` si `cantidad` o `torneoId` no son números; 401 `INVALID_TOKEN` si se envía un token que no corresponde a una sesión activa; 404 `TOURNAMENT_NOT_FOUND` si el torneo no existe (también con 0 o un número negativo), o si está Cancelado y quien consulta es anónimo (D-36). Un usuario autenticado ve el ranking de un torneo Cancelado, como ve el torneo (foro, debate 11; D-72). No hay 403 ni 409: el recurso es público.

Ejemplo: `GET /api/v1/rankings?cantidad=2` responde 200 con `[{"posicion":1,"nickname":"aurafarmer","elo":1200,"batallasGanadas":0,"batallasPerdidas":0},{"posicion":2,"nickname":"sixseven","elo":1200,"batallasGanadas":0,"batallasPerdidas":0}]`: los dos tienen el mismo ELO y va primero el que se registró antes.

Ejemplo con filtro: `GET /api/v1/rankings?torneoId=4&cantidad=2` responde 200 con los dos participantes de mayor ELO de ese torneo; `GET /api/v1/rankings?torneoId=999999` responde 404 con `{"userMessage":"No existe un torneo con ese id.","errorCode":"TOURNAMENT_NOT_FOUND",...}`.

