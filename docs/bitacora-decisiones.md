# Bitácora de decisiones — FarmeORT

Registro de las decisiones de diseño con impacto en mantenibilidad, testeabilidad o arquitectura.
Cada entrada indica el problema, las alternativas, la decisión, su justificación y la evidencia en el
repositorio. Al final hay una sección de pendientes detectados que todavía no se resolvieron.

---

### D-01 — Las interfaces de servicio reciben y devuelven DTOs

**Problema:** definir qué tipos cruzan la frontera entre WebAPI y la lógica de negocio, y dónde se
mapea DTO ↔ dominio.

**Alternativas consideradas:**
- IServicios depende de DTOs: el servicio recibe `*Request`, devuelve `*Response` y mapea en privado.
- IServicios depende de Dominio: el servicio devuelve entidades y el controller mapea con
  `Response.Desde(entidad)`.

**Decisión adoptada:** IServicios → DTOs. Ejemplo: `SesionResponse IniciarSesion(LoginRequest request)`.

**Justificación:** WebAPI no conoce el Dominio, lo que respeta el grafo de dependencias acordado
(`WebAPI → DTOs, IServicios`) y deja al controller como "recepcionista" sin lógica de mapeo. La
desventaja aceptada es que los servicios quedan acoplados al contrato HTTP.

**Conceptos del curso:** DIP, bajo acoplamiento, separación en capas.

**Evidencia:** `FarmeORT.IServicios/Sesiones/IServicioSesiones.cs`, `FarmeORT.WebAPI/Controllers/SesionesController.cs`.

**Participación de IA:** la IA planteó las dos alternativas con sus ventajas y desventajas; el equipo
eligió la primera.

---

### D-02 — Autenticación con token opaco GUID persistido y generador inyectable

**Problema:** cómo identificar al usuario en cada request y cómo testear un token que es aleatorio.

**Alternativas consideradas:**
- JWT firmado (no aprobado por el curso sin decisión documentada).
- Token opaco GUID guardado en la tabla `Sesiones`.
- Para el test: capturar la sesión guardada con un `Callback` de Moq, o inyectar un `IGeneradorTokens`.

**Decisión adoptada:** token opaco GUID persistido; la generación se abstrae en `IGeneradorTokens`
(implementado con `Guid.NewGuid()`).

**Justificación:** el token persistido permite invalidar la sesión (`DELETE /sesiones/actual`) y la
API sigue siendo stateless respecto de la memoria del servidor. Inyectar el generador vuelve
determinístico el test del servicio y permite `Setup` con valores exactos, sin `It.IsAny`.

**Conceptos del curso:** DIP, testeabilidad, REST stateless, dobles de prueba (stub).

**Evidencia:** `FarmeORT.Servicios/Sesiones/ServicioSesiones.cs`,
`FarmeORT.Servicios/Seguridad/GeneradorTokens.cs`, `ServicioSesionesTests`.

**Participación de IA:** la IA propuso inyectar el generador por el mismo criterio que el "azar
inyectable" de las batallas; el equipo lo aprobó.

---

### D-03 — Contraseñas hasheadas con PBKDF2 detrás de `IHasheadorContrasenas`

**Problema:** no guardar contraseñas en texto plano y poder testear el login sin depender del
algoritmo.

**Alternativas consideradas:** texto plano; librería externa (BCrypt); PBKDF2 de .NET
(`Rfc2898DeriveBytes.Pbkdf2`).

**Decisión adoptada:** PBKDF2 con SHA-256, 100.000 iteraciones y sal aleatoria de 16 bytes, guardado
como `sal.hash` en Base64 y comparado con `CryptographicOperations.FixedTimeEquals`. Un hash con
formato inválido devuelve `false` en lugar de lanzar excepción.

**Justificación:** no agrega dependencias (evita introducir tecnología no aprobada), la sal hace que
dos hashes de la misma contraseña sean distintos y la comparación en tiempo constante no revela
información por tiempo de respuesta. La interfaz permite usar un stub en `ServicioSesionesTests`.

**Conceptos del curso:** DIP, SRP, encapsulamiento.

**Evidencia:** `FarmeORT.Servicios/Seguridad/HasheadorContrasenas.cs`, `HasheadorContrasenasTests`.

**Participación de IA:** la IA propuso el algoritmo y los parámetros, y reorganizó los ciclos TDD para
no dejar un commit con contraseñas en texto plano; el equipo aprobó.

---

### D-04 — Respuesta de `POST /sesiones`

**Problema:** la especificación indica `SesionResponse + Location`, pero no existe un `GET` de sesiones.

**Alternativas consideradas:** devolver solo el token; devolver token, id y rol.

**Decisión adoptada:** `SesionResponse { token, usuarioId, rol }` con `Location: /api/v1/sesiones/actual`.

**Justificación:** el cliente conoce su rol sin una segunda llamada. `sesiones/actual` es la misma
identidad que usa `DELETE /sesiones/actual`.

**Evidencia:** `FarmeORT.DTOs/Sesiones/SesionResponse.cs`, `SesionesControllerTests`.

**Participación de IA:** la IA señaló el hueco en la especificación y propuso las opciones; el equipo
eligió la segunda.

---

### D-05 — Convención para la firma mínima de los commits RED

**Problema:** el `.editorconfig` obligatorio pone `CS9113` (parámetro sin leer) e `IDE0060` en
error, así que una firma mínima que solo lanza `NotImplementedException` no compila cuando la clase
recibe dependencias.

**Alternativas consideradas:** leer cada parámetro con un descarte (`_ = parametro;`) y lanzar
`NotImplementedException`; escribir una implementación falsa que falle por assertion.

**Decisión adoptada:** descarte explícito de cada parámetro sin usar más `NotImplementedException`.
Los descartes se eliminan en el commit GREEN.

**Justificación:** mantiene la convención de firma mínima de `AGENTS.md`, no suprime ninguna regla y el
RED no adelanta lógica del GREEN.

**Evidencia:** historial de commits `test [RED]` de la rama `feature/sesiones`.

**Participación de IA:** la IA detectó el conflicto con el `.editorconfig` y propuso las dos
opciones; el equipo eligió la primera. Pendiente: documentarla en `AGENTS.md`.

---

### D-06 — Registro de dependencias con métodos de extensión por assembly

**Problema:** las clases concretas son `internal`, así que `Program.cs` no puede registrarlas
directamente, y `AGENTS.md` pide que `Program.cs` delegue el registro.

**Alternativas consideradas:** hacer públicas las implementaciones; exponer un método de extensión
público por assembly.

**Decisión adoptada:** `AgregarServicios()` en Servicios y `AgregarRepositorios(connectionString)` en
Repositorios; todo con lifetime Scoped (`DbContext` incluido, vía `AddDbContext`).

**Justificación:** conserva el encapsulamiento (`internal`), cada assembly es dueño de su registro y
los tests verifican implementación y lifetime, lo que protege la regla de no tener un `DbContext`
Singleton.

**Conceptos del curso:** DI, IoC, encapsulamiento, lifetimes.

**Evidencia:** `RegistroServicios.cs`, `RegistroRepositorios.cs`, sus tests y `Program.cs`.

**Participación de IA:** propuesta de la IA, aprobada por el equipo.

---

### D-07 — Filtro global de excepciones con diccionario tipo → error

**Problema:** traducir excepciones a códigos HTTP sin `try/catch` en los controllers y sin exponer
detalles internos.

**Alternativas consideradas:** `try/catch` en cada action; cadena de `if/else` en el filtro;
diccionario tipo de excepción → error mapeado.

**Decisión adoptada:** `FiltroExcepciones` con un diccionario `Type → ErrorMapeado(Status, ErrorCode,
UserMessage)`. Una excepción no mapeada devuelve 500 con `INTERNAL_ERROR` y el mensaje fijo
"Ocurrió un error inesperado.", sin el mensaje original.

**Justificación:** agregar una excepción nueva es agregar una entrada al diccionario, sin tocar la
lógica del filtro. El caso base impide filtrar stack traces, SQL o paths al cliente.

**Conceptos del curso:** SRP, OCP, filtros de ASP.NET, seguridad.

**Evidencia:** `FarmeORT.WebAPI/Filtros/FiltroExcepciones.cs`, `FiltroExcepcionesTests`.

**Participación de IA:** propuesta de la IA; el equipo definió los códigos `INVALID_CREDENTIALS` e
`INTERNAL_ERROR`.

---

### D-08 — Navegaciones obligatorias de EF Core inicializadas con `null!`

**Problema:** EF Core no puede pasar una navegación por constructor, así que `Sesion` necesita un
constructor privado sin parámetros, y el compilador exige inicializar la propiedad `Usuario`.

**Alternativas consideradas:** declarar `Usuario?`, lo que obliga a chequear `null` en cada uso;
quitar la navegación y dejar solo la FK, lo que contradice la regla de "navegación y FK"; asignar
`Usuario = null!` en el constructor privado.

**Decisión adoptada:** constructor privado con `Usuario = null!` y un comentario que explica que EF
completa la navegación al materializar la entidad.

**Justificación:** es el patrón recomendado por la documentación de EF Core para navegaciones
obligatorias. No suprime ninguna regla.

**Evidencia:** `FarmeORT.Dominio/Sesiones/Sesion.cs`.

**Participación de IA:** la IA señaló que contradice la regla de `AGENTS.md` ("`= null!` solo en
campos de test"). Pendiente: aclarar la excepción en `AGENTS.md`.

---

### D-09 — Secretos fuera del repositorio

**Problema:** la contraseña del usuario `sa` del contenedor de SQL Server quedaba escrita en
`appsettings.json` y en `docker-compose.yml`, que se versionan.

**Alternativas consideradas:** dejarla en los archivos versionados; `dotnet user-secrets`; archivos
ignorados por el `.gitignore` obligatorio.

**Decisión adoptada:** el connection string va en `appsettings.Development.json` y la contraseña del
contenedor en `.env`, que `docker-compose.yml` lee como `${MSSQL_SA_PASSWORD}`. Ambos archivos ya
están excluidos por el `.gitignore` obligatorio.

**Justificación:** no requiere modificar la configuración obligatoria y `dotnet run` / `dotnet ef`
toman el entorno Development. Cada integrante crea esos dos archivos en su máquina.

**Evidencia:** `docker-compose.yml`, `FarmeORT.WebAPI/Program.cs`.

**Participación de IA:** la IA detectó el problema y propuso la solución; el equipo aprobó.

---

### D-10 — Errores de validación con el mismo formato que el resto de los errores

**Problema:** con `[ApiController]`, un DTO inválido devuelve un 400 en formato `ProblemDetails`, pero
el contrato exige `ErrorResponse`.

**Alternativas consideradas:** aceptar `ProblemDetails` para los 400; configurar
`InvalidModelStateResponseFactory`.

**Decisión adoptada:** `RespuestaValidacion.Crear` como `InvalidModelStateResponseFactory`: 400 con
`errorCode` `VALIDATION_ERROR`, el mensaje "Los datos enviados no son válidos." y un detalle
`"Campo: mensaje"` por cada error.

**Justificación:** el cliente recibe un único formato de error en toda la API.

**Evidencia:** `FarmeORT.WebAPI/Validaciones/RespuestaValidacion.cs`, `RespuestaValidacionTests`,
`LoginRequestTests`.

**Participación de IA:** propuesta de la IA, aprobada por el equipo.

---

### D-11 — Administrador precargado con hash fijo en la migración

**Problema:** el alta de usuarios es solo para administradores, así que la base necesita al menos un
administrador desde el inicio para poder usar la API (y para que los docentes puedan probarla).

**Alternativas consideradas:** script `.sql` aparte que inserte el usuario; alta al arrancar la API
(código en `Program.cs` que hashee la contraseña en cada ejecución); `HasData` en el contexto con el
hash calculado una sola vez.

**Decisión adoptada:** `HasData` con `Id = 1`, `admin@farmeort.com`, rol `Administrador` y un hash
PBKDF2 fijo, calculado aparte con los mismos parámetros que `HasheadorContrasenas`. La migración
`CreaUsuariosYSesiones` crea las tablas e inserta al administrador.

**Justificación:** el administrador queda versionado junto con el esquema y se crea con
`dotnet ef database update`, sin pasos manuales. El hash es fijo porque si se generara en cada build
la sal cambiaría y EF detectaría un cambio de modelo en cada migración. En el repositorio solo está el
hash; la contraseña inicial se documenta en la guía de instalación.

**Evidencia:** `FarmeORT.Repositorios/FarmeORTContext.cs`,
`Migrations/20260930041829_CreaUsuariosYSesiones.cs`,
`RepositorioUsuariosTests.ObtenerPorEmail_AdministradorPrecargado_DevuelveAdministrador` y la prueba
manual de login (201 con la contraseña correcta, 401 con otra).

**Participación de IA:** la IA propuso `HasData` con hash fijo y calculó el hash; el equipo definió la
contraseña inicial.

---

### D-12 — Roles como constantes de texto en DTOs

**Problema:** cada endpoint protegido tiene que indicar qué rol exige, pero WebAPI no conoce el
Dominio (D-01), así que no puede usar el enum `Rol`.

**Alternativas consideradas:** strings sueltos en cada atributo (magic strings); un enum propio en
DTOs, espejo del de Dominio, con mapeo en el servicio; constantes de texto en DTOs.

**Decisión adoptada:** la clase estática `Roles` en `FarmeORT.DTOs.Usuarios` con `Jugador`,
`Organizador` y `Administrador`. La sesión autenticada y la respuesta del login llevan el rol como
texto, generado con `Rol.ToString()`.

**Justificación:** evita magic strings sin duplicar el enum ni mantener dos tipos sincronizados. El test
`ObtenerSesionAutenticada_TokenConSesion_DevuelveTokenUsuarioYRol` compara con `Roles.Jugador`, lo que
garantiza que el texto del servicio coincide con el que compara el filtro.

**Conceptos del curso:** bajo acoplamiento, separación en capas, Clean Code (sin magic strings).

**Evidencia:** `FarmeORT.DTOs/Usuarios/Roles.cs`, `AutorizacionAttribute`, `ServicioSesionesTests`.

**Participación de IA:** la IA planteó las alternativas; el equipo eligió las constantes.

---

### D-13 — `[Autorizacion]` hereda de `[Autenticacion]` con un punto de extensión

**Problema:** la autorización por rol necesita que antes se haya autenticado el request. Si son dos
atributos independientes, es fácil marcar un endpoint con autorización y olvidar la autenticación.

**Alternativas consideradas:** dos atributos independientes con orden fijado por `Order`;
`[Autorizacion]` que hereda de `[Autenticacion]`.

**Decisión adoptada:** `AutenticacionAttribute` valida el token (401) y, al final, llama a un método
`protected virtual AutorizarSesion(context, sesion)` que por defecto no hace nada.
`AutorizacionAttribute(params string[] rolesPermitidos)` lo redefine y responde 403 si el rol no está
permitido. Un endpoint lleva un solo atributo: `[Autenticacion]` o `[Autorizacion(Roles.X)]`.

**Justificación:** no se puede exigir un rol sin autenticar, y la lectura del token no se repite. Es una
aplicación acotada de Template Method: la base fija los pasos y la subclase redefine uno. Cada clase
sigue teniendo una sola responsabilidad (autenticar o autorizar). Las reglas del tipo "¿es el
organizador asignado a este torneo?" no van en el filtro sino en el servicio.

**Conceptos del curso:** Template Method, OCP, SRP, herencia, filtros de ASP.NET.

**Evidencia:** `FarmeORT.WebAPI/Filtros/AutenticacionAttribute.cs`, `AutorizacionAttribute.cs` y sus
tests (incluido `OnAuthorization_SinHeaderAuthorization_Devuelve401ConInvalidToken` sobre
`[Autorizacion]`, que verifica el comportamiento heredado).

**Participación de IA:** la IA planteó las dos alternativas; el equipo eligió la herencia.

---

### D-14 — Los filtros de autenticación y autorización responden ellos mismos el 401 y el 403

**Problema:** el filtro global de excepciones (D-07) no captura las excepciones que lanzan los filtros
de autorización de ASP.NET, así que lanzar una excepción desde ahí terminaría en un 500.

**Alternativas consideradas:** lanzar excepciones y capturarlas con un middleware; que el filtro asigne
`context.Result` con el `ErrorResponse` correspondiente.

**Decisión adoptada:** los filtros asignan `context.Result`: 401 con `INVALID_TOKEN` ("La sesión no es
válida o expiró.") si falta el header, si no tiene la forma `Bearer {guid}` o si el token no tiene
sesión; 403 con `FORBIDDEN` ("No tenés permisos para realizar esta operación.") si el rol no está
permitido. El servicio devuelve `null` en lugar de lanzar cuando el token no existe.

**Justificación:** mantiene el mismo formato de error que el resto de la API sin agregar un
middleware. La consulta al servicio solo ocurre si el header tiene un formato válido.

**Evidencia:** `AutenticacionAttributeTests`, `AutorizacionAttributeTests` y la prueba manual de
`DELETE /sesiones/actual` (204 con token válido; 401 al repetirlo, sin token o con formato inválido).

**Participación de IA:** la IA detectó la limitación del filtro de excepciones y propuso esta solución;
el equipo definió los códigos `INVALID_TOKEN` y `FORBIDDEN`.

---

### D-15 — Test por reflexión de los atributos de seguridad de cada endpoint

**Problema:** los tests unitarios de un controller no ejecutan los filtros. Si alguien borra
`[Autenticacion]` de un endpoint, ningún test falla y el endpoint queda abierto.

**Alternativas consideradas:** tests de integración con `WebApplicationFactory` (no vistos en el curso
y más lentos); un test por reflexión que verifique que el método tenga el atributo.

**Decisión adoptada:** un test por endpoint protegido que verifica el atributo por reflexión, por
ejemplo `SesionesControllerTests.Eliminar_RequiereAutenticacion`.

**Justificación:** es rápido, no requiere infraestructura y protege la regla de seguridad. El
comportamiento del filtro en sí ya está cubierto por sus propios tests.

**Evidencia:** `FarmeORT.WebAPI.Tests/Controllers/SesionesControllerTests.cs`.

**Participación de IA:** propuesta de la IA, aprobada por el equipo.

---

## Pendientes detectados

### Detalles de las respuestas de validación (detectados en la prueba manual de `POST /sesiones`)

1. **Idioma:** los mensajes de `detalles` salen en inglés (por ejemplo, "The Email field is
   required."). Se resuelve con `ErrorMessage = "..."` en cada anotación de los DTOs.
2. **Nombres de campo:** los detalles usan `Email` y `Contrasena`, pero el contrato define el JSON en
   camelCase (`email`, `contrasena`).
3. **Mensajes redundantes:** con el body vacío aparecen dos errores para `Email` (requerido y formato
   inválido).
4. **JSON mal formado:** el detalle incluye el mensaje técnico del parser. No expone datos sensibles,
   pero no es un mensaje pensado para el usuario.

### Otros

- Documentar en `AGENTS.md` la convención de D-05 y la excepción de D-08.
- ~~Reemplazar la migración `InitialCreate` por una con nombre descriptivo después de agregar el
  administrador precargado.~~ Resuelto: `CreaUsuariosYSesiones` (D-11).
- Explicar en la guía de instalación cómo crear `.env` y `appsettings.Development.json` (D-09) y cuál
  es la contraseña inicial del administrador (D-11).
- Las sesiones no vencen: un token vale hasta que se hace logout. La letra no pide vencimiento, pero el
  mensaje del 401 dice "o expiró". Decidir si se agrega un vencimiento o se ajusta el mensaje.
- Cuando más controllers lean la sesión del contexto, extraer esa lectura a un solo lugar (por ejemplo,
  un método de extensión `HttpContext.ObtenerSesionAutenticada()`), para no repetir el cast.
