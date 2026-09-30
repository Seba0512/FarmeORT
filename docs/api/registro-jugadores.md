# Registro público de jugadores — punto 1.2

Contrato tomado de la tabla API acordada por el equipo y del punto 1.2 de la letra oficial.

| Campo | Contrato |
|---|---|
| Endpoint | `POST /api/v1/jugadores` |
| Acceso | Público, sin token ni rol previo |
| Headers | `Content-Type: application/json`, `Accept: application/json` |
| Route / query | Sin parámetros |
| Request | `RegistroJugadorRequest` |
| Éxito | `201 Created`, `UsuarioResponse`, `Location: /api/v1/usuarios/{id}` |

`Location` identifica al usuario creado mediante el recurso existente. Su consulta requiere
Administrador según el contrato de Usuarios; el registro no agrega un GET público ni inicia sesión.
El cliente obtiene los datos del jugador directamente en la respuesta y puede autenticarse mediante
`POST /api/v1/sesiones`.

## Entrada

Todos los campos son obligatorios. El JSON usa camelCase.

| Campo | Tipo | Validación |
|---|---|---|
| nombre | string | No vacío ni espacios; máximo 50 según la decisión D-19 vigente |
| apellido | string | Entre 3 y 25 caracteres |
| email | string | Formato válido, único entre todos los usuarios |
| nickname | string | Alfanumérico ASCII sin espacios, entre 3 y 20, único |
| contrasena | string | Entre 15 y 25, mayúscula, minúscula, número y símbolo |
| fechaNacimiento | fecha ISO `yyyy-MM-dd` | Requerida, al menos 13 años cumplidos en la fecha local |

```json
{
  "nombre": "Ana",
  "apellido": "Pérez",
  "email": "ana@farmeort.test",
  "nickname": "anaperez",
  "contrasena": "ClaveSegura123!!",
  "fechaNacimiento": "2000-01-15"
}
```

El request no contiene `rol`, `elo`, un id ni skills. Aunque un cliente envíe `rol` o `elo` como
propiedades adicionales, no se usan: el servidor fija Jugador y 1200. No se crean skills declaradas.

## Respuesta

`UsuarioResponse` contiene `id`, `nombre`, `apellido`, `email`, `nickname`, `fechaNacimiento`,
`rol: "Jugador"` y `elo: 1200`. El identificador se genera en la base. No contiene contraseña,
hash ni token. La contraseña se guarda con el hasheador PBKDF2 ya utilizado por el alta administrativa.

| HTTP | errorCode | Motivo |
|---|---|---|
| 201 | — | Jugador guardado, con representación y Location |
| 400 | VALIDATION_ERROR | Campos requeridos, formato o largo inválido, fecha omitida/null, JSON mal formado |
| 400 | INVALID_DATA | Regla del dominio, edad menor a 13 o fecha futura; también validación al invocar el servicio directamente |
| 409 | EMAIL_ALREADY_EXISTS | Email ocupado, incluso si la otra alta se guarda después de la consulta previa |
| 409 | NICKNAME_ALREADY_EXISTS | Nickname ocupado, con la misma protección al guardar |
| 500 | INTERNAL_ERROR | Fallo inesperado, con mensaje fijo y sin detalles internos |

Los errores usan `ErrorResponse { userMessage, errorCode, moreInfo, detalles }`. No se exigen
autenticación ni autorización en este endpoint; no hay 401 o 403 propios del registro.

## Diseño y trazabilidad

`JugadoresController` llama `IServicioUsuarios.RegistrarJugador`. `ServicioUsuarios` convierte el
request público al alta existente fijando `Roles.Jugador`, y reutiliza contraseña, unicidad,
fecha local, creación del dominio y mapeo de respuesta. `RepositorioUsuarios.Agregar` realiza un solo
`SaveChanges`; si aparece un conflicto de unicidad al guardar, lo traduce a las excepciones ya mapeadas.
Los demás errores de base se propagan y el filtro global responde el 500 seguro.

No hay clases de usuario por rol, nuevas dependencias, nuevos registros DI ni migraciones.
El modelo todavía no incluye las entidades del repertorio del punto 1.4; este registro no crea
skills ni introduce un modelo anticipado de esa funcionalidad. Al implementarla deberá conservarse
el repertorio inicial vacío.

- Controller: `FarmeORT.WebAPI/Controllers/JugadoresController.cs`.
- Contrato: `FarmeORT.DTOs/Usuarios/RegistroJugadorRequest.cs`, `UsuarioResponse.cs`.
- Servicio: `FarmeORT.IServicios/Usuarios/IServicioUsuarios.cs`, `FarmeORT.Servicios/Usuarios/ServicioUsuarios.cs`.
- Dominio: `FarmeORT.Dominio/Usuarios/Usuario.cs`, `ReglaContrasena.cs`.
- Persistencia: `FarmeORT.Repositorios/Usuarios/RepositorioUsuarios.cs`, índices únicos existentes.
- Pruebas: `JugadoresControllerTests`, `RegistroJugadorRequestTests`, `ServicioRegistroJugadorTests`,
  `UsuarioTests` y `RepositorioUsuariosTests`.
- Postman: `Datos/FarmeORT.RegistroJugadores.postman_collection.json`.
- UML: `docs/uml/registro-jugadores.puml` y `registro-jugadores-contrato.puml`, con sus PNG renderizados.
- Evidencia TDD y uso de IA: `docs/registro-jugadores-validacion.md`.

## REST

Se crea un recurso mediante POST, con URI plural y versionada, representación JSON, 201 y Location.
La identidad y el rol del jugador nuevo los define el servidor; no dependen de estado de sesión en
memoria. La respuesta se mapea a un DTO y los errores se traducen en los filtros comunes, manteniendo
la separación entre HTTP, caso de uso y persistencia.
