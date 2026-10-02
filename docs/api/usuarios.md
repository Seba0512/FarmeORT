# Usuarios: eliminación y cambio de rol (Persona A)

Contrato: letra (usuarios), especificación de la API del equipo, D-26, D-27, D-28 y D-43. Base `/api/v1`,
HTTP, JSON camelCase, `Accept: application/json`. Rol: Administrador. La identidad sale solo de
`Authorization: Bearer {token}`. El id de la ruta sale del listado o del alta de usuarios.

| Operación | Entrada | Respuesta | Estados |
|---|---|---|---|
| PUT `/usuarios/{id}` | Id en la ruta; `UsuarioAdminRequest` en el body (reemplaza todos los datos, D-28) | `UsuarioResponse` | 200, 400, 401, 403, 404, 409 |
| DELETE `/usuarios/{id}` | Id en la ruta. Sin body. | Sin contenido (baja lógica, D-27) | 204, 401, 403, 404, 409 |

Conflictos (409):

| errorCode | DELETE | PUT | Cuándo |
|---|---|---|---|
| `SELF_OPERATION_NOT_ALLOWED` | Sí | Sí | El administrador se elimina o se modifica a sí mismo |
| `EMAIL_ALREADY_EXISTS` / `NICKNAME_ALREADY_EXISTS` | No | Sí | El email o el nickname ya es de otro usuario |
| `USER_ORGANIZES_ACTIVE_TOURNAMENT` | Sí | Solo si cambia el rol | Organiza un torneo que no está Finalizado ni Cancelado |
| `USER_HAS_UNRESOLVED_BATTLES` | Sí | Solo si cambia el rol | Juega una batalla Pendiente o En arbitraje |
| `USER_HAS_ACTIVE_REGISTRATION` | Sí (supuesto, D-43) | Solo si cambia el rol | Tiene una inscripción Aceptada en un torneo que no está Finalizado ni Cancelado |

Si el `PUT` mantiene el rol, estas tres reglas no se controlan: se pueden corregir los datos de un
organizador con un torneo en curso. Las reglas se controlan en el orden de la tabla y se responde la
primera que falla.

Los demás errores no cambian: 400 `VALIDATION_ERROR` o `INVALID_DATA`, 401 `INVALID_TOKEN`, 403 `FORBIDDEN`,
404 `USER_NOT_FOUND`.

Ejemplo: `DELETE /api/v1/usuarios/12` sobre el organizador de un torneo Abierto responde 409 con
`{"userMessage":"El usuario organiza un torneo que no está Finalizado ni Cancelado.","errorCode":"USER_ORGANIZES_ACTIVE_TOURNAMENT",...}`.
