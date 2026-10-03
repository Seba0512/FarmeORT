# Errores generales de la API

Todas las respuestas de error usan el mismo cuerpo, en JSON y camelCase (D-07, D-65):

```json
{ "userMessage": "Mensaje para el usuario.", "errorCode": "CODIGO", "moreInfo": null, "detalles": null }
```

Este archivo lista los errores que no dependen de una regla de negocio. Los 404 y 409 de cada recurso (por
ejemplo, `TOURNAMENT_NOT_FOUND` o `SKILL_INACTIVE`) están en la especificación de cada uno.

| Estado | errorCode | Cuándo | Lo arma |
|---|---|---|---|
| 400 | `VALIDATION_ERROR` | Un dato no cumple el formato o el rango (data annotations), el cuerpo falta o no es un JSON legible, o un valor de la query no se puede convertir (`limit=abc`) | `RespuestaValidacion` |
| 400 | `INVALID_DATA` | Un dato no cumple una regla de formato o rango que vuelven a validar el dominio o el servicio, además del borde. El mensaje dice cuál | `FiltroExcepciones` |
| 401 | `INVALID_TOKEN` | Falta el token o no corresponde a una sesión activa ("La sesión no es válida.") | `AutenticacionAttribute` |
| 401 | `INVALID_CREDENTIALS` | Login con email o contraseña incorrectos, sin decir cuál de los dos falló | `FiltroExcepciones` |
| 403 | `FORBIDDEN` | El rol no tiene permiso para la operación | `AutorizacionAttribute` |
| 404 | `NOT_FOUND` | La ruta no existe, o un id de la ruta no es un número (`/usuarios/abc`) | Manejador global |
| 405 | `METHOD_NOT_ALLOWED` | La ruta existe, pero no admite ese verbo | Manejador global |
| 415 | `UNSUPPORTED_MEDIA_TYPE` | El cuerpo no se envía como `application/json` | Manejador global |
| 500 | `INTERNAL_ERROR` | Un error inesperado, también fuera de los controllers (por ejemplo, la base cae durante la autenticación). Mensaje fijo, sin detalles internos | `FiltroExcepciones` y manejador global |
| El mismo | `REQUEST_NOT_PROCESSED` | Cualquier otro error que el framework responda sin cuerpo | Manejador global |

`detalles` en un `VALIDATION_ERROR`:
- Un elemento por error, con la forma `campo: mensaje`. El campo usa el nombre del JSON, por ejemplo
  `fechaNacimiento: La fecha de nacimiento es obligatoria.`.
- Sin cuerpo: un solo detalle, `El cuerpo de la solicitud es obligatorio.`.
- Con un JSON que no se puede leer: `El cuerpo de la solicitud no es un JSON válido.`. Si se leyó, pero un dato no
  tiene el tipo esperado: `campo: El valor no tiene un formato válido.`. Nunca se muestra el mensaje técnico del
  parser.
