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

### D-16 — Validación de usuarios en el borde y en el dominio

**Problema:** las reglas del usuario (nombre, apellido, email, nickname, contraseña, edad) tienen que
cumplirse siempre, pero también conviene rechazar datos inválidos lo antes posible.

**Alternativas consideradas:** validar solo en el DTO; validar solo en el dominio; validar en los dos.

**Decisión adoptada:** anotaciones en `UsuarioAdminRequest` (400 `VALIDATION_ERROR`, con un detalle
por campo) **y** reglas en el constructor de `Usuario` y en `ReglaContrasena` (400 `INVALID_DATA`,
con el mensaje de la regla). La duplicación es deliberada: el borde corta temprano y el dominio queda
consistente aunque lo use otra entrada. Para eso Dominio pasa a referenciar a Excepciones, que no
depende de nada, así que no se forma ningún ciclo.

**Justificación:** el dominio es el experto en sus datos (Information Expert) y no puede quedar en un
estado inválido. `ReglaContrasena` usa las mismas clases de caracteres ASCII que la expresión regular
del DTO, para que las dos capas acepten y rechacen exactamente lo mismo.

**Conceptos del curso:** Information Expert, encapsulamiento, validación en el borde.

**Evidencia:** `Usuario`, `ReglaContrasena`, `UsuarioAdminRequest` y sus tests; prueba manual del
alta (400 con siete detalles en español y 400 `INVALID_DATA` para un menor de 13).

**Participación de IA:** propuesta de la IA; el equipo eligió validar en las dos capas.

---

### D-17 — Un solo `Usuario` con rol y ELO, creado por constructor

**Problema:** el ELO solo tiene sentido para los jugadores, y un usuario no debe poder quedar con datos
inválidos.

**Alternativas consideradas:** subclase `Jugador` con el ELO (obliga a cambiar de tipo si el
administrador cambia el rol, y a usar herencia en EF); una sola clase `Usuario` con rol y ELO.

**Decisión adoptada:** una sola clase, con un constructor que valida y setters privados; el
constructor sin parámetros queda privado y solo lo usa EF. Todo usuario arranca con ELO 1200;
`UsuarioResponse` incluye el ELO solo si el rol es Jugador.

**Justificación:** respeta "un solo rol por usuario" sin jerarquías, y nadie puede modificar un usuario
salteando sus reglas. El administrador precargado se carga con un objeto anónimo en `HasData`, porque
el constructor no recibe el `Id`.

**Evidencia:** `FarmeORT.Dominio/Usuarios/Usuario.cs`, `FarmeORTContext`, `ServicioUsuarios.CrearResponse`.

**Participación de IA:** la IA planteó las dos alternativas; el equipo eligió la clase única.

---

### D-18 — La fecha actual se inyecta

**Problema:** la regla de edad mínima depende de la fecha de hoy, y un test que usa el reloj real da
resultados distintos según el día en que corre.

**Alternativas consideradas:** `DateTime.Now` dentro del dominio; una interfaz propia de reloj;
`TimeProvider` de .NET.

**Decisión adoptada:** el constructor de `Usuario` recibe la fecha actual como parámetro; el servicio
la obtiene de `TimeProvider` (registrado como `TimeProvider.System`) usando la hora **local**, porque
en Uruguay la fecha UTC ya es la del día siguiente a partir de las 21:00. La edad se calcula como
"la fecha en que cumple 13 todavía no llegó", sin restar años a mano.

**Justificación:** el dominio no depende de infraestructura y los tests son repetibles. `TimeProvider`
viene en .NET, así que no se agrega ninguna tecnología nueva, y se puede mockear con Moq.

**Conceptos del curso:** DIP, tests FIRST (repetibles), dobles de prueba.

**Evidencia:** `Usuario.ValidarEdad`, `ServicioUsuarios`, tests `Constructor_MenorDe13Anios_…` y
`Constructor_Cumple13AniosHoy_EsValido`.

**Participación de IA:** propuesta de la IA, aprobada por el equipo.

---

### D-19 — Supuesto: el nombre tiene como máximo 50 caracteres

**Problema:** la letra solo pide "mínimo un carácter" para el nombre, pero AGENTS.md pide largos máximos
en las columnas. Sin un máximo validado, un nombre largo pasaría las validaciones y fallaría recién al
guardar, con un 500.

**Alternativas consideradas:** columna sin límite (`nvarchar(max)`); asumir un máximo y validarlo.

**Decisión adoptada:** máximo de 50 caracteres, validado en el DTO y en el dominio, igual que la columna.

**Justificación:** cubre cualquier nombre real y evita un error del servidor por culpa del cliente. Es
un supuesto propio y se documenta como tal.

**Evidencia:** `Constructor_NombreDeMasDe50Caracteres_LanzaDatosInvalidos`, `UsuarioAdminRequestTests`.

**Participación de IA:** la IA detectó el hueco y propuso las dos opciones; el equipo eligió el máximo.

---

### D-20 — Orden de los pasos del alta de usuario

**Problema:** el alta valida datos, consulta la base y hashea una contraseña con PBKDF2, que es costoso a
propósito.

**Decisión adoptada:** primero la contraseña y el rol (sin tocar la base), después email y nickname
únicos (409), recién después el hash, y por último el constructor de `Usuario` y el guardado. El rol se
convierte con `Enum.TryParse` más `Enum.IsDefined`, para que un texto como `"5"` no se acepte como rol.

**Justificación:** no se consulta la base con datos que ya se sabe que son inválidos, y no se paga el
hash si el email ya existe. Los tests lo verifican con mocks estrictos sin configurar: si el servicio
consultara la base antes, fallarían. El índice único de email y nickname en la base cubre el caso de
dos altas simultáneas.

**Evidencia:** `ServicioUsuarios.Crear` y sus tests `…SinConsultarLaBase`.

**Participación de IA:** propuesta de la IA, aprobada por el equipo.

---

### D-21 — Listado de usuarios: filtros, orden y paginación

**Problema:** `GET /usuarios` admite filtros por nombre, apellido y nickname, y paginación con `limit` y
`offset`.

**Decisión adoptada:**
- Los filtros buscan por **contenido** y sin distinguir mayúsculas, y se combinan con un Y lógico.
- Se comparan las dos partes en minúscula (`ToLower`), porque en SQL Server la comparación no distingue
  mayúsculas por defecto pero en el SQLite de los tests sí; así el resultado es el mismo en los dos.
- El orden es siempre por `Id`, explícito, para que la paginación sea estable y no dependa del orden
  accidental de SQL.
- `limit` vale 20 por defecto y va de 1 a 100; `offset` desde 0. Fuera de rango, 400.
- El repositorio recibe un record `FiltroUsuarios` definido en IRepositorios, porque no conoce los DTOs.
  Al ser un record, el test del servicio configura el mock con el valor exacto esperado.
- Los tres filtros quedan como tres `if` de dos líneas: extraerlos obligaría a armar el árbol de
  expresiones a mano para que EF los traduzca, y eso se lee peor.

**Evidencia:** `RepositorioUsuarios.Listar` y sus tests con SQLite; prueba manual del listado.

**Participación de IA:** propuesta de la IA, aprobada por el equipo.

---

### D-22 — Registro público con request propio y rol fijado por el servicio

**Problema:** el punto 1.2 permite registrarse sin sesión, pero el cliente no debe elegir un rol
administrativo. El alta de usuarios ya implementa las validaciones, hash, unicidad y ELO inicial.

**Alternativas consideradas:** reutilizar directamente el request administrativo en el endpoint
público; duplicar todo el caso de uso; un request público sin rol y una operación explícita del
servicio que reutiliza el alta existente.

**Decisión adoptada:** `RegistroJugadorRequest` y `IServicioUsuarios.RegistrarJugador`; el servicio
fija `Roles.Jugador` al delegar a `Crear`. La fecha nullable detecta una fecha omitida en el borde y
el servicio también la exige. El Controller devuelve `UsuarioResponse` y Location del recurso
Usuarios existente, cuyo GET conserva el acceso administrativo acordado.

**Justificación:** aplica el contrato aprobado sin introducir otro servicio, jerarquía de usuarios
o endpoint público de consulta. Las propiedades adicionales rol/ELO no conceden privilegios y no
se crea una sesión automáticamente. No se crean skills declaradas; el modelo del punto 1.4 se
implementará con su propia funcionalidad.

**Conceptos del curso:** SRP, bajo acoplamiento, reutilización, validación en el borde y el dominio,
Controller de GRASP.

**Evidencia:** `JugadoresController`, `ServicioUsuarios.RegistrarJugador`, sus tests y la colección
`Datos/FarmeORT.RegistroJugadores.postman_collection.json` (incluye rol/ELO adicionales y login).

**Participación de IA:** Codex implementó y verificó el flujo dentro del pedido del usuario,
reutilizando las decisiones D-01, D-03, D-16 a D-20 y el contrato API del equipo.

---

### D-23 — Conflictos de unicidad detectados al guardar también responden 409

**Problema:** otra alta puede guardar un email o nickname entre la consulta de unicidad y
`SaveChanges`. El índice único evita el duplicado, pero la excepción de EF antes terminaba en 500.

**Alternativas consideradas:** mantener el 500; interpretar mensajes internos específicos de SQL
Server; consultar las claves de unicidad después de una `DbUpdateException` y traducir solo el
conflicto encontrado a las excepciones existentes.

**Decisión adoptada:** `RepositorioUsuarios.Agregar` conserva un solo guardado. Si falla, consulta
el email y luego el nickname con los métodos existentes; lanza `EmailYaExisteException` o
`NicknameYaExisteException` cuando existe ese conflicto. Si no hay ninguno, propaga el error de EF.

**Justificación:** respeta el 409 aprobado con SQL Server y con el fake SQLite, sin examinar mensajes
del proveedor, cambiar índices o introducir dependencias. La consulta posterior confirma el
conflicto; no sustituye al índice único que protege la persistencia.

**Conceptos del curso:** separación de responsabilidades, encapsulación de persistencia, traducción
de excepciones y pruebas de integración. Es responsabilidad de infraestructura traducir el
conflicto detectado por la base, mientras el servicio conserva las validaciones previas.

**Evidencia:** tests `Agregar_EmailOcupadoDespuesDeConsultar_…`,
`Agregar_NicknameOcupadoDespuesDeConsultar_…` y `Agregar_ErrorDeBaseSinDuplicados_…`, con SQLite.

**Participación de IA:** Codex reprodujo el pendiente mediante RED/GREEN, reutilizó las consultas
en REFACTOR y adaptó el catch para satisfacer simultáneamente el formato obligatorio y StyleCop.

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
- Un id que no es número (`/usuarios/abc`) responde 404 **sin cuerpo**, porque lo resuelve el
  enrutamiento antes de los filtros. Igual con los errores de conversión de la query
  (`limit=abc`), cuyo detalle sale en inglés ("The value 'abc' is not valid for Limit.").
- Restricciones de eliminación de usuarios que dependen de entidades que todavía no existen: no se puede
  eliminar si tiene batallas sin resolver o si organiza un torneo que no está Finalizado ni Cancelado.
- ~~Si dos altas compiten por email o nickname, la segunda terminaba en 500.~~ Resuelto en D-23:
  el conflicto detectado al guardar se traduce a 409; los otros errores de EF se propagan.
- Registrar en AGENTS.md que Dominio referencia a Excepciones (D-16).
