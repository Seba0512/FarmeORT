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

**Actualización (D-34):** los métodos de registro se mantienen, pero ya no los llama `Program.cs` sino
la fábrica (`FarmeORT.Fabrica`), y la WebAPI dejó de referenciar Servicios y Repositorios.

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

**Actualización (D-35):** `limit` y `offset` fuera de rango ya no responden 400: se ajustan al tope, al
valor por defecto o a 0, como sugieren los docentes. El tipo de paginado se justifica en D-35.

---

### D-22 — Registro público y alta administrativa comparten creación con rol tipado

**Problema:** el registro anónimo no puede permitir elegir rol ni ELO. Reutilizar directamente
`UsuarioAdminRequest` desde `RegistrarJugador` acoplaba el caso público al contrato administrativo.

**Alternativas consideradas:** duplicar toda la creación; convertir el request público a un DTO
administrativo; extraer una creación privada común con los datos compartidos y `Rol` como enum.

**Decisión adoptada:** `RegistrarJugador` recibe `RegistroJugadorRequest`, fija `Rol.Jugador` y
llama a `CrearUsuario`; `Crear` convierte y valida el rol administrativo y llama al mismo método.
El método común consulta unicidad, obtiene la fecha local, hashea, construye y persiste. Se conserva
el orden de validaciones de D-20 y el registro no convierte el rol Jugador de enum a texto y viceversa.
El jugador nace con ELO 1200, sin skills declaradas y sin iniciar sesión automáticamente.

**Location:** se devuelve `/api/v1/usuarios/{id}`, el recurso existente que representa al usuario
creado. La consulta sigue siendo administrativa: el anónimo recibe la representación en el POST,
pero no puede consultar esa URL. La especificación exige `UsuarioResponse + Location` y no define
un GET público de jugadores. Se acepta esta limitación sin inventar una ruta ni ampliar permisos.

**Justificación:** evita duplicar reglas y depender del DTO administrativo desde el registro, y
mantiene la especificación aprobada. El repertorio del punto 1.4 se implementará con esa funcionalidad.

**Conceptos del curso:** SRP, bajo acoplamiento, reutilización y Controller de GRASP.

**Evidencia:** `ServicioUsuarios.RegistrarJugador`, `Crear` y `CrearUsuario`, `JugadoresController`,
tests de ambos servicios y `Datos/FarmeORT.postman_collection.json` (rol/ELO adicionales y Location).

**Participación de IA:** Codex implementó el registro y aplicó el refactor solicitado por el usuario
a partir de la revisión del compañero, quien informó haber consultado a Claude. Se siguieron la
letra, tabla API, AGENTS.md, contexto local y skills de arquitectura, API, dominio, TDD y revisión.

---

### D-23 — Traducir unicidad al guardar y recuperar el contexto tras un fallo

**Problema:** otra alta puede ocupar el email o nickname entre la validación y `SaveChanges`.
Además, un guardado fallido dejaba al usuario como `Added`, contaminando el próximo guardado del contexto.

**Alternativas consideradas:** mantener el 500; interpretar mensajes SQL específicos del proveedor;
consultar las claves después del fallo y traducir el conflicto. Para el tracking: conservarlo,
limpiar todo el contexto o desasociar únicamente el usuario cuya inserción falló.

**Decisión adoptada:** ante `DbUpdateException`, desasociar el usuario con `EntityState.Detached`,
consultar email y nickname con los métodos existentes y lanzar sus excepciones de unicidad (409).
Si no hay esos conflictos, relanzar el error original con `throw;`. No limpiar otras entidades.

**Justificación:** el índice único protege la base; las consultas posteriores permiten expresar
el conflicto con las excepciones existentes sin depender del texto del proveedor. El usuario fallido
no se reintenta en otro `SaveChanges`, y otros errores mantienen el tratamiento seguro de 500.

**Conceptos del curso:** encapsulación de persistencia, traducción de excepciones y tracking de EF.

**Evidencia:** `RepositorioUsuarios.Agregar` y tests de email, nickname y error no relacionado con
esas unicidades, que además guardan otra alta en el mismo contexto después de cada fallo.

**Participación de IA:** Codex reprodujo y corrigió el problema con RED/GREEN separados, siguiendo
la revisión aportada por el usuario y las skills de TDD, arquitectura y dominio.

---

### D-24 — Compartir las validaciones de datos personales y exigir la fecha en ambas altas

**Problema:** las anotaciones estaban duplicadas entre DTOs y `UsuarioAdminRequest.FechaNacimiento`
era no anulable. Una fecha omitida llegaba como `01/01/0001`, pasaba la edad mínima y podía persistirse.

**Alternativas consideradas:** mantener dos DTOs independientes con reglas duplicadas; corregir
solo la fecha administrativa; compartir las propiedades mediante herencia del request público.

**Decisión adoptada:** `UsuarioAdminRequest` extiende `RegistroJugadorRequest` y agrega únicamente
`Rol`. Las seis propiedades compartidas conservan nombres, anotaciones y mensajes; la fecha es
`DateOnly?` con `[Required]`. El servicio también exige la fecha, sin inventar una fecha por defecto.

**Justificación:** el alta administrativa exige los mismos datos del registro más el rol (punto 2.1).
Una única definición evita divergencias. La API distingue ausencia y null del valor de una fecha;
el dominio sigue recibiendo una fecha concreta. No cambia JSON ni se agregan campos al registro.

**Conceptos del curso:** reutilización, herencia de contratos de datos y validación en el borde y servicio.

**Evidencia:** `UsuarioAdminRequestTests.Validar_JsonSinFechaNacimiento_RechazaFechaOmitida`,
`ServicioUsuariosTests.Crear_FechaNacimientoOmitida_LanzaDatosInvalidosSinGuardarNiHashear`,
tests de ambos DTOs y requests administrativos de fecha omitida, nula y válida en Postman.

**Participación de IA:** el compañero identificó el bug en su revisión con Claude; el usuario pidió
corregirlo. Codex ejecutó y commiteó RED antes de GREEN y realizó el refactor con tests verdes.

---

### D-25 — Rechazar fechas futuras antes del cálculo de edad

**Problema:** `AddYears(13)` podía desbordar con una fecha extrema y producir 500. El guardado
contra ese desbordamiento daba el mensaje de edad mínima incluso para un nacimiento futuro.

**Alternativas consideradas:** capturar el desbordamiento; agrupar ambos casos con el mensaje de
edad mínima; validar primero que la fecha no sea futura y luego comprobar los 13 años cumplidos.

**Decisión adoptada:** rechazar una fecha futura antes de llamar a `AddYears`, con el mensaje
"La fecha de nacimiento no puede ser futura.". Para menores de 13 se conserva el mensaje de edad
mínima; cumplen la regla quienes cumplen 13 en la fecha local actual. Ambos errores son 400 INVALID_DATA.

**Justificación:** un dato inválido del cliente no debe causar un error del servidor y cada mensaje
debe describir el problema concreto. Se conserva el reloj local inyectado de D-18.

**Conceptos del curso:** validación de dominio, casos límite y errores REST coherentes.

**Evidencia:** tests de fecha máxima, fecha futura cercana, edad mínima y cumpleaños 13; caso de
fecha futura extrema en `Datos/FarmeORT.postman_collection.json`.

**Participación de IA:** Codex corrigió el desbordamiento en el registro y luego precisó el mensaje
solicitado en la revisión, con RED/GREEN separados para ambos cambios.

**Alcance documental de esta revisión:** se versionan esta bitácora y la colección JSON. Por pedido
expreso del usuario no se generan UML, imágenes ni otros Markdown; los diagramas y evidencias
gráficas quedan a cargo del equipo para su consolidación en los entregables académicos.

---

### D-26 — Operar sobre uno mismo es un conflicto (409) que decide el servicio

**Problema:** la letra (2.1) prohíbe que un usuario se modifique o elimine a sí mismo. La
especificación admite 403 y 409 en `PUT` y `DELETE /usuarios/{id}`, y hay que decidir dónde vive la
regla.

**Alternativas consideradas:** 403 desde un filtro (como si fuera falta de permisos); 403 desde el
servicio; 409 desde el servicio.

**Decisión adoptada:** el controller toma el id del usuario autenticado de la sesión (nunca del body ni
de la query) y lo pasa al servicio, que lanza `OperacionSobreSiMismoException` antes de consultar la
base. El filtro global la traduce a 409 `SELF_OPERATION_NOT_ALLOWED` ("Un usuario no puede modificarse
ni eliminarse a sí mismo."). La lectura de la sesión se centralizó en
`HttpContext.ObtenerSesionAutenticada()` (resuelve un pendiente de esta bitácora).

**Justificación:** el administrador sí tiene permiso sobre el recurso `/usuarios`; lo que se rechaza es
la operación por el estado (el destino es él mismo). Así el 403 queda reservado al rol, que deciden los
filtros (D-14), y la regla depende del recurso concreto, por lo que va en el servicio (`AGENTS.md`, §7).

**Conceptos del curso:** semántica de status codes, SRP de los filtros, identidad tomada solo del token.

**Evidencia:** `ServicioUsuarios.Modificar`/`Eliminar`, `FiltroExcepciones`, tests
`Modificar_UsuarioActual_...`, `Eliminar_UsuarioActual_...`,
`OnException_OperacionSobreSiMismo_Devuelve409ConSelfOperationNotAllowed` y casos 409 de Postman.

**Participación de IA:** Claude Code (Claude Opus 5.5) presentó las dos opciones de status con sus
ventajas; el equipo eligió 409.

---

### D-27 — Eliminación de usuarios como baja lógica

**Problema:** la letra permite eliminar a un jugador que ya tiene batallas finalizadas, pero esas
batallas siguen alimentando el ranking (ganadas y perdidas del rival) y un torneo puede tener a ese
jugador como campeón. Un borrado físico rompería ese historial o lo impediría por claves foráneas.

**Alternativas consideradas:** borrado físico con cascada (pierde historial ajeno); borrado físico con
`Restrict` (impide eliminar a quien jugó); baja lógica con un indicador en `Usuario`.

**Decisión adoptada:** baja lógica. `Usuario.Eliminar()` marca `Eliminado = true` y
`RepositorioUsuarios.Eliminar` guarda la baja y borra las sesiones del usuario en un único
`SaveChanges` (atómico). Un usuario dado de baja no se obtiene por id (404 en `GET`, `PUT` y `DELETE`),
no aparece en el listado y no puede iniciar sesión (401 `INVALID_CREDENTIALS`, sin revelar el motivo).
Migración `AgregaBajaLogicaAUsuarios`.

**Supuesto:** el email y el nickname de un usuario dado de baja siguen reservados (los índices únicos
los incluyen), así que reutilizarlos responde 409. Liberarlos exigiría índices filtrados y queda fuera
de esta entrega.

**Justificación:** conserva la integridad del historial sin depender de cascadas, y cerrar las sesiones
en la misma transacción evita que un token emitido antes de la baja siga funcionando.

**Conceptos del curso:** Information Expert (el usuario sabe darse de baja), atomicidad de operaciones
con varios efectos, persistencia con EF Core.

**Evidencia:** `Usuario.Eliminar`, `RepositorioUsuarios.Eliminar`/`ObtenerPorId`/`Listar`,
`ServicioSesiones.IniciarSesion`, tests de dominio, servicio y repositorio (SQLite) de la baja, y el
recorrido de Postman: baja → sesión cerrada (401) → login rechazado → 404 → email reservado (409).

**Participación de IA:** Claude Code propuso baja lógica frente a borrado físico con sus
consecuencias; el equipo eligió la baja lógica.

---

### D-28 — `PUT /usuarios/{id}` reemplaza todos los datos y valida unicidad contra otros usuarios

**Problema:** la modificación usa el mismo `UsuarioAdminRequest` del alta (especificación) y las mismas
validaciones (letra 2.1). Hay que evitar que el usuario choque consigo mismo al conservar su email o
nickname, también con otro uso de mayúsculas, porque SQL Server compara sin distinguirlas.

**Alternativas consideradas:** contraseña opcional en el `PUT` (rompe el reemplazo completo y el
contrato); consultar unicidad siempre y excluir el propio id en el repositorio; consultar solo si el
valor cambió.

**Decisión adoptada:** reemplazo completo: la contraseña es obligatoria y se vuelve a hashear; el rol
puede cambiar y el ELO se conserva. Orden: operación sobre sí mismo (409) → validaciones del request
(400) → existencia (404) → unicidad (409) → hash → `Usuario.Modificar` → guardado. La unicidad se
consulta solo si el email o el nickname cambiaron sin distinguir mayúsculas. El constructor de
`Usuario` reutiliza `Modificar`, así que alta y modificación comparten validaciones. Si otro usuario
ocupa el valor entre la consulta y el guardado, el repositorio traduce el conflicto como en D-23,
ahora excluyendo el propio id y compartido entre `Agregar` y `Actualizar`.

**Justificación:** un `PUT` reemplaza el recurso completo; no consultar lo que no cambió evita falsos
409, y excluir el propio id al traducir errores evita confundir un fallo ajeno a la unicidad con un
duplicado (que terminaría en 409 en lugar del 500 seguro).

**Conceptos del curso:** idempotencia y semántica de `PUT`, DRY, validación en el borde y en el dominio.

**Evidencia:** `ServicioUsuarios.Modificar`, `Usuario.Modificar`, `RepositorioUsuarios.Actualizar` y
`Guardar`, tests `Modificar_ConservaSuEmailYNickname_NoLoConsideraDuplicado`,
`Actualizar_NicknameOcupadoConservandoSuEmail_LanzaNicknameYaExiste`,
`Actualizar_ErrorDeBaseConservandoEmailYNickname_PropagaDbUpdateException` y casos de Postman.

**Participación de IA:** Claude Code implementó la funcionalidad con ciclos RED/GREEN/REFACTOR por
commit, siguiendo `AGENTS.md` y las skills `da2-*`; el equipo revisa y aprueba en el PR.

---

### D-29 — Categoría de skill como lista fija

**Problema:** la letra (2.2) deja la categoría "a criterio del equipo" y pide filtros "en base a sus
atributos". Con texto libre, `Gestual`, `gestual` y `Gestos` serían categorías distintas y el filtro
por categoría no serviría.

**Alternativas consideradas:** texto libre con largo máximo; una tabla de categorías administrable
(otro recurso con su CRUD y endpoints que la especificación no tiene); una lista fija.

**Decisión adoptada:** enum `CategoriaSkill` en Dominio con los cuatro ejemplos de la letra
(`Gestual`, `Vocal`, `Baile`, `Meme`) y las constantes `CategoriasSkill` en DTOs, igual que `Roles`
(D-12). Se valida en tres lugares: el DTO (`AllowedValues`, 400 `VALIDATION_ERROR`), el servicio
(compara contra los nombres del enum, para que un texto como `"1"` no se acepte, cosa que `Enum.Parse`
sí haría) y el dominio (`Enum.IsDefined`). En la base se guarda como texto (`nvarchar(20)`), como el rol.

**Justificación:** los filtros son confiables y no hace falta agregar endpoints fuera del contrato.
Sumar una categoría es un cambio acotado (enum, constantes y mensaje) que hoy nadie pidió. La
comparación por nombre resuelve una trampa real que dejó en evidencia el test
`Crear_CategoriaInexistente_LanzaDatosInvalidosSinConsultarLaBase("1")`.

**Conceptos del curso:** validación en el borde y en el dominio (D-16), Information Expert, bajo
acoplamiento entre capas (D-01, D-12).

**Evidencia:** `CategoriaSkill`, `CategoriasSkill`, `SkillRequest`, `ServicioSkills.ConvertirCategoria`,
`Skill.ValidarCategoria`, sus tests y la migración `AgregaCatalogoDeSkills`.

**Participación de IA:** Claude Code planteó las tres alternativas y recomendó la lista fija; el
integrante la aceptó. Problema que apareció en el camino: la primera implementación convertía la
categoría con `Enum.Parse`, que aceptaba `"1"` como categoría válida; lo detectó un test y se cambió por
la comparación por nombre. Pendiente: confirmar los valores contra el diagrama de clases de Dominio.

---

### D-30 — Contrato y diseño del catálogo de skills

**Problema:** implementar alta, modificación, cambio de estado y consulta de skills (letra 2.2, con
asterisco) respetando la especificación de la API, que en algunos puntos difiere del diagrama de clases.

**Alternativas consideradas:** seguir el diagrama (`FiltroSkills` sin `Nombre`, métodos `Obtener`,
`Buscar` y `ExisteCodigo`, jerarquía de excepciones con `FarmeORTExcepcion`); seguir la especificación
de la API y las convenciones que ya tiene el código de usuarios.

**Decisión adoptada:**
- Endpoints de la especificación: `GET /skills` y `GET /skills/{id}` para cualquier usuario autenticado
  (`[Autenticacion]`); `POST /skills`, `PUT /skills/{id}` y `PATCH /skills/{id}` solo para el
  Administrador (`[Autorizacion(Roles.Administrador)]`). Como el controller mezcla roles, los atributos
  van en cada método y no en la clase.
- Filtros `categoria`, `dificultad`, `activa` y `nombre` (por contenido y sin distinguir mayúsculas),
  combinados con Y lógico; `limit` 20 por defecto (1 a 100), `offset` desde 0 y orden por `Id`, igual
  que D-21. `nombre` se incluye porque está en la especificación aunque falte en el diagrama.
- Los nombres siguen el código de usuarios: `ObtenerPorId`, `Listar` y `ExistePorCodigo`.
- Excepciones por caso, como D-07: `SkillNoEncontradaException` (404 `SKILL_NOT_FOUND`) y
  `CodigoSkillYaExisteException` (409 `SKILL_CODE_ALREADY_EXISTS`). Las reglas del dominio usan
  `DatosInvalidosException` (400 `INVALID_DATA`).
- Supuesto: una skill nace activa (la letra no lo dice, y una skill recién creada que no se pueda usar
  no tendría sentido). `PATCH` con `{ "activa": true | false }` la activa o la desactiva y devuelve 200
  con la skill; `activa` es obligatorio.
- Orden en `PUT`: datos (400) → existencia (404) → unicidad del código (409) → `Skill.Modificar` →
  guardado, como D-28. El constructor de `Skill` reutiliza `Modificar`, así que alta y modificación
  comparten las validaciones del dominio.
- El body de `PUT` no lleva `activa`. En clase se dijo que en `PUT` se manda toda la información; acá el
  estado de la skill tiene su propia operación (`PATCH`), igual que el `Id` no va en el body, así que el
  `PUT` reemplaza los datos editables del catálogo y conserva el estado.
- Excepciones propias en lugar de las de .NET (por ejemplo `KeyNotFoundException`): cada una lleva su
  `errorCode` y su mensaje, y una excepción genérica que saliera de un error de programación terminaría
  en un 404 o un 409 falsos en lugar del 500 seguro. Es la justificación que piden los docentes para no
  "crear excepciones por crear".
- Actualización (D-35): los filtros y la paginación fuera de rango ya no responden 400.

**Justificación:** el código implementa el contrato aprobado, y mantener las convenciones existentes
hace que skills se lea igual que usuarios sin refactorizar lo que ya está hecho. El equipo eligió
seguir el código en lugar del diagrama, que queda por actualizar.

**Conceptos del curso:** REST (`PATCH` para un cambio parcial, `PUT` como reemplazo completo,
autenticación y autorización separadas), Controller de GRASP, consistencia entre código y UML.

**Evidencia:** `SkillsController`, `ServicioSkills`, `Skill`, `RepositorioSkills`, `FiltroExcepciones`,
los tests de cada capa y la carpeta Skills de la colección de Postman.

**Participación de IA:** Claude Code detectó las diferencias entre el diagrama y la especificación y
propuso seguir el código; el integrante aceptó esa opción. Claude Code implementó la funcionalidad con
ciclos RED/GREEN/REFACTOR por commit, siguiendo `AGENTS.md` y las skills `da2-*`. Lo que el integrante
modificó: pidió un código más simple de leer (ifs explícitos en lugar de `?? throw`, `Regex.IsMatch` común,
`.ToList()`) y se aplicó en commits REFACTOR. Lo que se rechazó: la propuesta de clases públicas sin
`sealed` (D-33), que se revirtió. Problema que apareció: las justificaciones del `PUT` sin `activa` y de las
excepciones propias faltaban hasta que se revisaron las transcripciones de clase.

---

### D-31 — El código de una skill es único sin distinguir mayúsculas

**Problema:** el índice único de SQL Server no distingue mayúsculas, pero el SQLite de los tests sí.
Sin cuidado, `sixseven` pasaría la consulta de unicidad aunque exista `SIXSEVEN` y fallaría al guardar
con un 500, y cambiar solo las mayúsculas del propio código daría un falso 409.

**Alternativas consideradas:** comparar con `==` y depender de la colación de cada base; normalizar el
código a mayúsculas al guardarlo (cambia lo que escribió el administrador); comparar en minúscula y no
consultar la unicidad si el código no cambió.

**Decisión adoptada:** `ExistePorCodigo` compara en minúscula (mismo criterio que D-21); el servicio
consulta la unicidad solo si el código cambió sin distinguir mayúsculas (como D-28); y si otra skill
ocupa el código entre la consulta y el guardado, el repositorio traduce el `DbUpdateException` a
`CodigoSkillYaExisteException` y suelta la entidad del contexto (como D-23). Cualquier otro error de
base se propaga y termina en el 500 seguro.

**Justificación:** el resultado es el mismo en SQL Server y en SQLite, y nunca se devuelve un 500 por
un error del cliente.

**Conceptos del curso:** encapsulación de la persistencia, traducción de excepciones, tests repetibles
(FIRST).

**Evidencia:** `RepositorioSkills.ExistePorCodigo` y `Guardar`, `ServicioSkills.Modificar`, tests
`ExistePorCodigo_CodigoGuardado_DevuelveTrueSinDistinguirMayusculas`,
`Modificar_SuMismoCodigoConOtrasMayusculas_NoLoConsideraDuplicado`,
`Agregar_CodigoOcupadoDespuesDeConsultar_LanzaCodigoSkillYaExisteSinDuplicar`,
`Agregar_ErrorDeBaseSinCodigoDuplicado_PropagaDbUpdateException` y casos de Postman.

**Participación de IA:** propuesta de Claude Code a partir de D-21, D-23 y D-28, aceptada sin cambios.
El problema lo mostró la prueba de punta a punta contra SQL Server: los tests con SQLite no lo detectaban
porque SQLite sí distingue mayúsculas.

---

### D-32 — EF Core materializa las skills con su constructor público

**Problema:** `Usuario` tiene un constructor privado sin parámetros para EF porque el público recibe
datos que no son columnas (`fechaActual`). `Skill` no tiene ese problema.

**Alternativas consideradas:** agregar un constructor privado sin parámetros, como en `Usuario`;
dejar que EF use el constructor público, cuyos parámetros se llaman igual que las propiedades.

**Decisión adoptada:** EF usa el constructor público y después asigna `Id` y `Activa`. No se agrega un
constructor que solo usaría EF.

**Justificación:** hay menos código y la entidad siempre pasa por sus validaciones. La desventaja
aceptada es que una fila inválida cargada a mano en la base fallaría al leerse; la API no puede
producirla.

**Evidencia:** `Skill`, `RepositorioSkillsTests` (cargan skills activas e inactivas desde la base).

**Participación de IA:** propuesta de Claude Code, aceptada. Es una decisión técnica menor; puede pasar a
supuestos si el informe necesita espacio.

---

### D-33 — Las clases del catálogo de skills son `public` y sin `sealed` (revertida)

**Problema:** `AGENTS.md` pide `internal sealed` por defecto. Quien desarrolló el catálogo de skills
prefiere clases más simples de leer y de explicar, sin modificadores que no aportan en un proyecto de
este tamaño, pero sin obligar al resto del equipo a cambiar el código que ya escribió.

**Alternativas consideradas:** mantener `internal sealed` también en skills; pasar todo el repositorio a
`public` sin `sealed` (se probó y se descartó porque modificaba código de otros integrantes sin
acordarlo); `internal` sin `sealed` (no es posible: CA1852 lo marca y `dotnet format` vuelve a agregar
`sealed`, así que falla el check `Analysis`); `public` sin `sealed` solo en las clases nuevas de skills.

**Decisión adoptada:** las clases y records creados para el catálogo de skills (controller, DTOs,
servicio, entidad, filtro, repositorio, excepciones y sus tests) son `public` y sin `sealed`. El código
existente conserva `internal sealed` y `AGENTS.md` no cambia.

**Justificación:** el código nuevo queda más simple sin tocar lo que hicieron otros integrantes. La
independencia entre assemblies no depende de esto: cada capa sigue dependiendo de interfaces
(`IServicioSkills`, `IRepositorioSkills`) y cada assembly registra sus implementaciones (D-06). Las
desventajas aceptadas son que conviven dos criterios en el repositorio y que `ServicioSkills` y
`RepositorioSkills` quedan visibles desde otros assemblies; por eso la documentación de diseño indica
qué tipos forman la interfaz de cada assembly.

**Conceptos del curso:** encapsulamiento, DIP, independencia de librerías.

**Evidencia:** commit `refactor [REFACTOR] (.): quita sealed e internal de las clases del catálogo de
skills`.

**Participación de IA:** el cambio lo pidió el integrante; Claude Code señaló el conflicto con
`AGENTS.md` y con la regla CA1852, y aplicó el cambio solo en las clases de skills.

**Actualización (revertida el 2026-10-01):** al revisar las transcripciones de clase, los docentes
remarcan que cada assembly exponga solo su interfaz ("Independencia de librerías") y que la WebAPI no
conozca la lógica ni el acceso a datos: en un ejemplo bajaron un diagrama de componentes de 8 a 4 por
eso. Con `ServicioSkills` y `RepositorioSkills` públicos, la WebAPI podía usarlos directamente. Claude
Code planteó el conflicto y el integrante eligió volver las clases de skills a `internal sealed`, igual
que el resto del código, en lugar de mantener la excepción. Evidencia: commit `refactor [REFACTOR] (.):
vuelve a internal sealed las clases del catálogo de skills`.

---

### D-34 — La WebAPI no conoce Servicios ni Repositorios: la inyección se resuelve en una fábrica

**Problema:** la WebAPI referenciaba Servicios y Repositorios para registrar las dependencias en
`Program.cs` (D-06). En clase, un docente bajó un diagrama de componentes de 8 a 4 justamente porque la
WebAPI conocía la lógica y el acceso a datos; la letra pide que un cambio impacte en la menor cantidad
posible de componentes.

**Alternativas consideradas:** dejar a `Program.cs` como composition root y justificarlo; un assembly
fábrica que haga el registro; además, dentro de la fábrica, que la WebAPI igual pueda ver Servicios y
Repositorios de forma indirecta (referencia transitiva) o impedirlo.

**Decisión adoptada:** nuevo assembly `FarmeORT.Fabrica` con `AgregarDependencias(connectionString)`, que
registra el `TimeProvider` y llama a `AgregarServicios` y `AgregarRepositorios`. La WebAPI referencia solo
DTOs, IServicios, Excepciones y la fábrica, con `DisableTransitiveProjectReferences` para que el
compilador no le deje usar Servicios ni Repositorios. `Program.cs` además delega el registro de los
controllers, el filtro global y la validación en `RegistroWebApi.AgregarControllersConFiltros`.

**Justificación:** la WebAPI depende solo de abstracciones (DIP) y el diagrama de componentes queda
`WebAPI → IServicios ← Servicios`, con la fábrica como único componente que conoce las implementaciones.
Las clases siguen siendo `internal sealed` y cada assembly sigue registrando las suyas (D-06). Se verificó
que en ejecución las DLL se copian igual, que la API levanta y que `dotnet ef` sigue funcionando con la
WebAPI como proyecto de arranque.

**Conceptos del curso:** DIP, Indirection (GRASP), independencia de librerías, diagrama de componentes.

**Evidencia:** `FarmeORT.Fabrica/RegistroDependencias.cs` y `RegistroDependenciasTests` (resuelve los
servicios con `ValidateOnBuild` y `ValidateScopes`), `FarmeORT.WebAPI/RegistroWebApi.cs` y
`RegistroWebApiTests`, `FarmeORT.WebAPI.csproj`, `Program.cs`; prueba de compilación que confirma que la
WebAPI no puede usar Servicios ni Repositorios; prueba de punta a punta contra SQL Server.

**Participación de IA:** Claude Code detectó el problema al revisar las transcripciones de clase y
propuso la fábrica; el integrante la aceptó en lugar de solo justificar D-06. Claude Code agregó
`DisableTransitiveProjectReferences` después de comprobar que, sin eso, la WebAPI seguía viendo las
implementaciones a través de la fábrica.

---

### D-35 — Paginación y filtros fuera de rango se ajustan en lugar de rechazarse

**Problema:** `GET /usuarios` y `GET /skills` respondían 400 ante un `limit` de 600, un `offset` negativo
o un filtro con un valor que no existe. En clase sugirieron lo contrario: "en vez de tirarle un error…
hacerle un fallback al límite" y anular el filtro inválido "para no trancar al usuario". Además, piden
como obligatorio documentar el tipo de paginado con sus alternativas.

**Alternativas consideradas:**
- Para los valores fuera de rango: responder 400 (lo que había) o ajustarlos.
- Para el tipo de paginado: `limit`/`offset`, número de página con tamaño de página, o cursor.

**Decisión adoptada:**
- `limit` mayor a 100 usa 100; `limit` menor a 1 usa el valor por defecto (20); `offset` negativo usa 0.
- En skills, una categoría que no existe o una dificultad fuera de 1 a 5 no filtran.
- Un tipo mal formado (`limit=abc`) sigue siendo 400, porque no hay un valor razonable que usar.
- El ajuste vive en los servicios (`Paginacion`, compartida por skills y usuarios) y los DTOs de filtro ya
  no tienen validaciones de rango.
- Paginado con `limit`/`offset`, ordenado por `Id`.

**Justificación:** un cliente que pide de más recibe lo que hay en lugar de un error. Un `limit` menor a 1
se trata igual que uno ausente. Sobre el tipo de paginado:
- El paginado por página es equivalente a `limit`/`offset`, pero menos flexible.
- El cursor evita repetir o saltear elementos cuando se insertan datos entre dos pedidos, pero no permite
  saltar directo a una página.
- El catálogo de skills y los usuarios cambian poco, que es el caso en que en clase aceptan
  `limit`/`offset`. El orden fijo por `Id` hace que las páginas sean estables.

**Conceptos del curso:** buenas prácticas REST (filtros y paginación en la query, valores por defecto),
DRY, Clean Code.

**Evidencia:** `FarmeORT.Servicios/Paginacion.cs`, `ServicioSkills.Listar`, `ServicioUsuarios.Listar`, sus
tests (`Listar_LimitMayorAlMaximo_UsaElMaximo`, `Listar_LimitMenorAUno_UsaElValorPorDefecto`,
`Listar_OffsetNegativo_EmpiezaDesdeElPrincipio`, `Listar_CategoriaInexistente_NoFiltraPorCategoria`,
`Listar_DificultadFueraDeRango_NoFiltraPorDificultad`), los tests de los DTOs de filtro y los casos de
Postman.

**Participación de IA:** Claude Code encontró la sugerencia en las transcripciones y la propuso; el
integrante la aceptó para skills y usuarios. Claude Code eligió el valor por defecto (y no el mínimo) para
un `limit` menor a 1, y que un tipo mal formado siga dando 400.

---

### D-36 — Contrato y visibilidad de la gestión de torneos

**Problema:** implementar alta, modificación, cancelación y consulta de torneos (letra 1.3 y 2.3) con la
especificación de la API, que abre las consultas al anónimo con `Authorization` opcional y pide
`TorneoDetalleResponse` para el detalle. Parte del detalle y del listado depende de entidades que todavía
no existen (skills obligatorias, inscripciones y rondas).

**Alternativas consideradas:**
- Detalle: devolver `TorneoResponse` hasta que existan las rondas, o crear ya `TorneoDetalleResponse`.
- `skillObligatoriaId` e inscripciones aceptadas: agregarlos al contrato cuando exista su funcionalidad,
  o ya.
- Anónimo: un filtro nuevo e independiente, o una variante de `[Autenticacion]`. Para saber quién
  consulta, pasarle al servicio el rol o solo si es anónimo.
- Torneo cancelado pedido por un anónimo: 404 o 401.

**Decisión adoptada:**
- Endpoints de la especificación. `POST`, `PUT` y `POST /torneos/{id}/cancelaciones` solo para el
  Administrador. `GET /torneos` y `GET /torneos/{id}` con `[AutenticacionOpcional]`.
- `[AutenticacionOpcional]` hereda de `[Autenticacion]` (como `[Autorizacion]`, D-13) y redefine
  `PermiteAnonimo`. Sin el encabezado `Authorization` deja pasar como anónimo. Con un token inválido
  responde 401, como dice la nota general de la especificación.
- El controller solo calcula si quien consulta es anónimo (`ObtenerSesionOpcional`). La regla de
  visibilidad la aplica el servicio, que recibe `esAnonimo`:
  - el listado le pide al repositorio excluir los torneos cancelados;
  - el detalle de un torneo cancelado responde 404 `TOURNAMENT_NOT_FOUND`, igual que un id inexistente.
  - Un usuario autenticado, con cualquier rol, ve todos los estados.
- `TorneoDetalleResponse` hereda de `TorneoResponse`: "se devuelven además" (letra 1.3). Por ahora no
  agrega propiedades. Las skills obligatorias, los jugadores aceptados y las rondas se suman con sus
  funcionalidades.
- `TorneoResponse.InscripcionesAceptadas` y `FiltroTorneosRequest.SkillObligatoriaId` ya están en el
  contrato. Mientras no existan esas entidades, la cantidad vale 0 y el repositorio no aplica el filtro
  (pendiente).
- Filtros:
  - `estado` usa los nombres del enum (`Abierto`, `EnCurso`, `Finalizado`, `Cancelado`), comparados por
    nombre como la categoría de skill (D-29).
  - `fechaDesde` y `fechaHasta` filtran por la fecha de inicio, ambas incluidas.
  - `nombre` filtra por contenido y sin distinguir mayúsculas.
  - Se combinan con Y lógico; un estado que no existe no filtra y la paginación se ajusta como en D-35.
    Orden por `Id`.

**Justificación:**
- El contrato queda igual a la especificación desde ahora, y las funcionalidades que faltan solo agregan
  datos, sin cambiar tipos ni rutas.
- Heredar el filtro reutiliza la validación del token sin duplicarla.
- Pasar un booleano y no el rol mantiene la regla en el servicio sin acoplarlo a cómo se representa el
  rol en la WebAPI.
- El 404 no revela que existe un torneo cancelado, y la especificación de `GET /torneos/{id}` ya lista 200
  y 404.

**Conceptos del curso:** REST (autenticación opcional, sub-recurso `cancelaciones`, filtros en la query),
Template Method (D-13), SRP en los filtros, Information Expert, Low Coupling.

**Evidencia:**
- `TorneosController`, `AutenticacionOpcionalAttribute`, `ExtensionesHttpContext.ObtenerSesionOpcional`.
- `ServicioTorneos.ObtenerPorId` y `Listar`, `RepositorioTorneos.Listar`.
- Los tests `Consultas_IncluidoAnonimo_UsanAutenticacionOpcional`,
  `OnAuthorization_TokenEnviadoPeroInvalido_Devuelve401ConInvalidToken`,
  `ObtenerPorId_TorneoCanceladoConsultadoPorAnonimo_LanzaTorneoNoEncontrado`,
  `Listar_ConsultaAnonima_PideExcluirLosTorneosCancelados` y
  `Listar_RangoDeFechas_DevuelveLosQueEmpiezanDentroDelRangoIncluyendoLosExtremos`.

**Participación de IA:**
- Los ciclos del controller y del filtro se hicieron en un chat sin las skills `da2-*` cargadas. Claude
  Code los revisó después con las skills y verificó commit por commit que cada RED fallara por la razón
  esperada y cada GREEN pasara.
- En esa revisión encontró que faltaban `TorneoDetalleResponse`, `skillObligatoriaId` y la cantidad de
  inscripciones aceptadas. Propuso agregarlos ya al contrato; el integrante aceptó, igual que el 404 para
  el anónimo.
- Problema que apareció: un RED de dominio (`rechaza cancelar un torneo ya cancelado`) incluyó además un
  test que ya pasaba. En los ciclos siguientes, esos tests se commitearon aparte como
  `test (...): documenta ...`.

---

### D-37 — Reglas del alta y la modificación de torneos y sus errores

**Problema:** la letra fija los datos del torneo, pero no dice cómo responder a un organizador que no
existe o no tiene el rol, si la regla "inicio no anterior a hoy" vale al modificar, ni con qué código se
rechaza modificar o cancelar en un estado que no lo permite.

**Alternativas consideradas:**
- Organizador con otro rol: 400 o 409.
- Fecha de inicio al modificar: validarla siempre, nunca, o solo si cambia.
- Errores de estado: un `errorCode` genérico o uno por operación.

**Decisión adoptada:**
- Organizador inexistente o dado de baja: 404 `USER_NOT_FOUND` (se reutiliza
  `UsuarioNoEncontradoException`). Con otro rol: 400 `INVALID_DATA` desde el dominio ("El organizador debe
  ser un usuario con rol Organizador.").
- En el borde: código, nombre de 5 a 60, descripción de 20 a 500, fechas, cupo entre 4 y 64 y organizador
  obligatorios. En el dominio, además: cupo potencia de 2, fin posterior al inicio, inicio no anterior a
  hoy y rol del organizador. La fecha actual llega por `TimeProvider` (D-18).
- Al modificar, la regla de la fecha de inicio solo se aplica si la fecha cambia. Un torneo Abierto cuya
  fecha ya pasó puede editar el resto, pero no se le puede poner una fecha pasada.
- `Torneo` valida todo antes de asignar (`AsignarDatos`, compartido por el constructor y `Modificar`):
  un dato inválido no deja el torneo a medio cambiar.
- Orden en el servicio: datos obligatorios (400) → existencia (404) → unicidad del código (409) → dominio
  → guardado, como D-28 y D-30. El código se compara sin distinguir mayúsculas y se traduce el conflicto
  al guardar, como D-31.
- Un `errorCode` por operación: `TOURNAMENT_NOT_MODIFIABLE` (modificar un torneo que no está Abierto) y
  `TOURNAMENT_NOT_CANCELLABLE` (cancelar uno Finalizado o Cancelado), los dos 409. Además, 404
  `TOURNAMENT_NOT_FOUND` y 409 `TOURNAMENT_CODE_ALREADY_EXISTS`. Las excepciones son propias porque cada
  una tiene su código (D-30).
- La FK `Torneos.OrganizadorId` es `Restrict`: un organizador no se borra físicamente (la baja es lógica,
  D-27). La migración es `AgregaTorneos`.

**Justificación:**
- El rol equivocado es un dato inválido del pedido, no un conflicto con el estado del sistema.
- La lectura literal de la fecha ("al momento del alta") dejaría modificar un torneo para que empiece en
  el pasado, y validarla siempre bloquearía editar un torneo Abierto con la fecha vencida, cuando las
  fechas son solo informativas.
- Un código por operación le permite al cliente distinguir el caso sin leer el mensaje.

**Conceptos del curso:** validación en el borde y en el dominio (D-16), Information Expert, status codes
(400, 404, 409), excepciones propias justificadas, integridad referencial en EF Core.

**Evidencia:**
- `Torneo`, `ServicioTorneos`, `RepositorioTorneos`, `FiltroExcepciones`, `FarmeORTContext` y la migración
  `AgregaTorneos`.
- Los tests `Constructor_OrganizadorSinRolOrganizador_LanzaDatosInvalidos`,
  `Modificar_FechaInicioNuevaAnteriorALaActual_LanzaDatosInvalidos`,
  `Modificar_FechaInicioSinCambiosYaPasada_ReemplazaLosDemasDatos`,
  `Modificar_DatosInvalidos_LanzaDatosInvalidosSinCambiarElTorneo`,
  `OnException_ErrorMapeado_DevuelveSuStatusYCodigo` y
  `Agregar_CodigoOcupadoDespuesDeConsultar_LanzaCodigoTorneoYaExisteSinDuplicar`.

**Participación de IA:**
- Claude Code planteó las alternativas de cada punto con una recomendación. El integrante eligió las
  recomendadas: 404/400, la fecha solo si cambia, el filtro por fecha de inicio y un código por operación.
- Problemas que aparecieron:
  - El compilador no veía que `AsignarDatos` asigna `Organizador` (CS8618). Se resolvió con
    `[MemberNotNull]`, que describe el método sin suprimir ninguna regla.
  - El test de la fábrica mostró que registrar el servicio exige registrar antes el repositorio
    (`ValidateOnBuild`), así que el GREEN del registro del servicio se cerró después del repositorio.
  - En los tests del repositorio, un organizador desasociado del contexto hacía que EF intentara
    insertarlo de nuevo. Los tests ahora lo toman del contexto, como en la API.

---

### D-38 — El repertorio vive en `Usuario` y se guarda a través de `RepositorioUsuarios`

**Problema:** el jugador declara las skills que sabe hacer, con nivel de 1 a 5 y fecha (letra 1.4). Las
reglas (no repetir, no declarar una inactiva) las van a necesitar también la inscripción ("tiene todas
las obligatorias") y lanzar skill ("la skill está en su repertorio"). Hay que decidir dónde viven esos
datos y esas reglas, y qué responder en cada error.

**Alternativas consideradas:**
- Dominio: una colección de `SkillDeclarada` dentro de `Usuario`, con las reglas en sus métodos; o
  `SkillDeclarada` como agregado aparte, con su propio repositorio y las reglas en el servicio.
- Skill inactiva: 400 (dato inválido) o 409 con código propio.
- Quitar una skill que no está declarada: 404 con código propio o reutilizar `SKILL_NOT_FOUND`.

**Decisión adoptada:**
- `Usuario` tiene `Repertorio` (colección de solo lectura sobre un campo privado) y los métodos
  `DeclararSkill(skill, nivel, fechaActual)` y `QuitarSkill(skillId)`. `SkillDeclarada` es la entidad
  intermedia explícita (D-17 sigue: un solo `Usuario` con rol).
- Reglas en el dominio:
  - skill inactiva → 409 `SKILL_INACTIVE` ("No es posible utilizar la skill porque está inactiva.", el
    texto de la letra);
  - skill ya declarada → 409 `SKILL_ALREADY_DECLARED`;
  - nivel fuera de 1 a 5 → 400 `INVALID_DATA` (el DTO también lo valida → 400 `VALIDATION_ERROR`);
  - quitar una skill no declarada → 404 `DECLARED_SKILL_NOT_FOUND`.
- La inactivación no es retroactiva: solo se controla al declarar. Una skill ya declarada que después
  se desactiva sigue en el repertorio.
- Persistencia:
  - Tabla `SkillsDeclaradas` con clave compuesta (`JugadorId`, `SkillId`), que impide los duplicados
    también en la base.
  - FK al jugador en cascada y a la skill con `Restrict`.
  - `IRepositorioUsuarios` suma `ObtenerConRepertorio` (incluye cada skill) y `GuardarRepertorio`, que
    solo llama a `SaveChanges`. No se usa `Update`: marcaría como modificada una skill nueva con la
    clave ya asignada y el UPDATE fallaría.
  - Si otro pedido declara la misma skill entre la carga y el guardado, la clave compuesta lo frena y
    se responde 409 en lugar de 500 (como D-23).
- Contrato:
  - Las rutas son `/jugadores/actual/repertorio`. El jugador sale siempre del token: el controller
    lleva `[Autorizacion(Roles.Jugador)]` y nunca recibe un id de jugador.
  - `POST` responde 201 con `Location: /api/v1/jugadores/actual/repertorio/{skillId}`, la URI del
    recurso que después se borra con `DELETE`.
  - El listado se ordena por `SkillId` para que no dependa del orden en que la base devuelve las filas.
- Se extrajo `DatosObligatorios` en Servicios. Se usa en torneos y repertorio para controlar los datos
  que el DTO declara opcionales.

**Justificación:**
- Información y reglas juntas (Information Expert): la inscripción y lanzar skill van a preguntarle
  al jugador por su repertorio en vez de repetir consultas en cada servicio.
- El repertorio no tiene sentido sin el jugador, así que no necesita un repositorio propio.
- La skill inactiva es 409 porque el pedido está bien formado, pero choca con el estado actual de la
  skill. Además, la letra pide un mensaje específico.

**Conceptos del curso:** Information Expert, agregados y entidad intermedia en EF Core, clave compuesta,
encapsulamiento de colecciones, REST (`/actual` en lugar de un id), status 404 y 409.

**Evidencia:**
- `Usuario.DeclararSkill` y `QuitarSkill`, `SkillDeclarada`, `ServicioRepertorio`, `RepertorioController`.
- `RepositorioUsuarios.ObtenerConRepertorio` y `GuardarRepertorio`, `FarmeORTContext`, la migración
  `AgregaRepertorioDeJugadores` y sus tests.
- La carpeta Repertorio de Postman.

**Participación de IA:**
- Claude Code planteó las dos ubicaciones del repertorio y los códigos de error, con una recomendación;
  el integrante eligió las recomendadas.
- Problemas que aparecieron:
  - Agregar la navegación a `Usuario` rompió los tests de todos los repositorios, porque EF descubrió
    `SkillDeclarada` sin clave. La configuración del contexto entró en el mismo GREEN.
  - Claude Code advirtió antes de implementar el problema de `Update` con una clave ya asignada y lo
    evitó con `GuardarRepertorio`.
  - La cobertura combinada marcó una rama de `QuitarSkill` como no cubierta, aunque la cubrían los tests
    del servicio. Se agregó el caso en los tests del dominio.

---

### D-40 — Skills obligatorias del torneo y el organizador asignado (403)

**Problema:** el Organizador asocia y quita las skills que todo jugador debe lanzar en sus batallas (letra
2.4): de 0 a 2, solo skills activas y solo con el torneo Abierto. El filtro de autorización sabe el rol,
pero no si quien llama es el organizador asignado a ese torneo. La especificación lista 403 y 409, y la
bitácora no definía cuál corresponde. La decisión la reusan las rondas, el fallo de batallas y la
finalización (Personas B y C).

**Alternativas consideradas:**
- Organizador no asignado: 403 o 409.
- Modelo: entidad intermedia explícita con datos propios, o una relación muchos a muchos sin datos.
- Errores de cada regla: un código por regla o un 409 genérico.
- Anónimo que pide las obligatorias de un torneo Cancelado: 404 (como D-36) o 200.

**Decisión adoptada:**
- **Organizador no asignado → 403 `NOT_TOURNAMENT_ORGANIZER`.**
  - Lo decide el servicio con `NoEsOrganizadorDelTorneoException`, mapeada a 403 en el filtro global.
  - El filtro `[Autorizacion(Roles.Organizador)]` sigue controlando solo el rol (D-13).
  - Las Personas B y C reusan la misma excepción para las rondas, los fallos y la finalización.
- Controller propio, `SkillsObligatoriasController`, con ruta `/api/v1/torneos/{torneoId}/skills-obligatorias`:
  - `GET` → 200 `SkillResponse[]`, ordenadas por id. Usa `[AutenticacionOpcional]`.
  - `POST` con `{ skillId }` → 201 con `Location: .../skills-obligatorias/{skillId}`.
  - `DELETE /{skillId}` → 204.
  - No se toca `TorneosController`, que también va a usar la Persona C.
- `Torneo` tiene `SkillsObligatorias` (colección de solo lectura) y los métodos `AsociarSkillObligatoria` y
  `QuitarSkillObligatoria`. En la base es una relación muchos a muchos sin datos propios: tabla
  `SkillsObligatorias` con clave (`TorneoId`, `SkillId`), FK al torneo en cascada y a la skill con
  `Restrict`. Una skill puede ser obligatoria en varios torneos.
- Reglas en el dominio:
  - torneo que no está Abierto → 409 `TOURNAMENT_NOT_MODIFIABLE` (se reusa);
  - skill inactiva → 409 `SKILL_INACTIVE` (se reusa; no es retroactivo);
  - ya asociada → 409 `MANDATORY_SKILL_ALREADY_ADDED`;
  - tercera obligatoria → 409 `MANDATORY_SKILLS_LIMIT_REACHED`;
  - quitar una que no está asociada → 404 `MANDATORY_SKILL_NOT_FOUND`.
- En el servicio:
  - torneo o skill inexistentes → 404 (`TOURNAMENT_NOT_FOUND` y `SKILL_NOT_FOUND`);
  - sin `skillId` → 400.
- El anónimo no ve las obligatorias de un torneo Cancelado (404, como D-36). Esa regla pasó a
  `Torneo.EsVisibleParaAnonimos`, que también usa `ServicioTorneos`.
- `GET /torneos?skillObligatoriaId=` ya filtra: devuelve los torneos que exigen esa skill.
- La regla de la letra que rechaza asociar una skill si hay inscripciones Aceptadas de jugadores que no la
  tienen declarada queda para `feature/inscripciones`: necesita la entidad `Inscripcion` (resuelta en D-41).

**Justificación:**
- 403 es falta de permiso sobre ese recurso: el usuario está autenticado y tiene el rol correcto, pero el
  torneo no es suyo. 409 se reserva para conflictos con el estado (D-26 es distinto: ahí el recurso es él
  mismo).
- Que la regla de visibilidad viva en el torneo evita repetirla en cada servicio (Information Expert).
- Un código por regla le permite al cliente distinguir el caso sin leer el mensaje (como D-37).

**Conceptos del curso:** autorización vs. reglas de negocio (D-13), status 403/404/409, Information Expert,
relaciones muchos a muchos en EF Core.

**Evidencia:**
- `SkillsObligatoriasController`, `ServicioSkillsObligatorias`, `Torneo.AsociarSkillObligatoria`,
  `QuitarSkillObligatoria` y `EsVisibleParaAnonimos`.
- `FarmeORTContext.ConfigurarSkillsObligatorias`, `RepositorioTorneos` y la migración
  `AgregaSkillsObligatoriasDeTorneos`.
- Los tests de cada capa y la carpeta "Skills obligatorias" de Postman.

**Participación de IA:**
- Claude Code (agente de la Persona A) planteó las alternativas con una recomendación; el integrante
  eligió 403, un código por regla y 404 para el anónimo.
- Problemas que aparecieron:
  - Al agregar la colección a `Torneo`, EF la interpretó por convención como uno a muchos (la skill
    hubiera quedado atada a un solo torneo). El primer RED del repositorio, con dos torneos que comparten
    la skill, lo dejó en evidencia y se configuró el muchos a muchos explícito.
  - StyleCop no permite poner un método público después de los privados. Los métodos nuevos de `Torneo`
    van al final de los públicos y no al final de la clase, sin reordenar código existente.
  - `CrearResponse` de skills queda repetido entre `ServicioSkills` y `ServicioSkillsObligatorias`. No se
    extrajo para no modificar el catálogo de skills fuera del alcance de esta rama.

---

### D-41 — Inscripciones a torneos, detalles en los errores y reglas que dependen de ellas

**Problema:** el Jugador se inscribe a un torneo y puede cancelar su inscripción (letra 1.5 y 1.6). La
inscripción es la entidad que esperaban tres reglas ya pendientes:
- la cantidad de inscripciones Aceptadas del torneo (D-36 la devolvía en 0);
- el rechazo de una skill obligatoria nueva si hay inscriptos que no la tienen, identificándolos (D-40);
- el rechazo de quitar del repertorio una skill que es obligatoria de un torneo en curso del jugador (D-38).

La identificación de los jugadores y de las skills que faltan no entraba en el cuerpo de error: el filtro
global solo completaba `detalles` con las validaciones del modelo. `Inscripcion` es además contrato
compartido: la Persona B la elimina al fallar una batalla y la Persona A la cancela al cancelar el torneo.

**Alternativas consideradas:**
- Las skills o jugadores que faltan: dentro del mensaje, en `detalles` o en un campo nuevo del error.
- Inscripción ajena (jugador u organizador de otro torneo): 403 o 404.
- Dónde vive "la skill es obligatoria en una inscripción vigente": en el repositorio (una consulta con la
  regla) o en `Inscripcion`, con el repositorio solo trayendo las inscripciones Aceptadas del jugador.
- Cantidad de Aceptadas en `GET /torneos`: contar por torneo (N+1 consultas) o una sola consulta agrupada.
- Migración: solo las inscripciones, o también las batallas de B, que llegaron a develop sin migración.

**Decisión adoptada:**
- Contrato, en `InscripcionesController` (`/api/v1/inscripciones`):
  - `POST` con `{ torneoId }`, solo Jugador → 201 con `Location: /api/v1/inscripciones/{id}`;
  - `GET` con filtros `torneoId`, `estado`, `fechaDesde`, `fechaHasta`, `limit` y `offset` (D-35). El
    jugador ve las suyas, el organizador las de sus torneos y el administrador todas;
  - `GET /{id}` → 200, 403 si no es suya o no es su torneo, 404 si no existe;
  - `POST /{id}/cancelaciones`, solo Jugador → 200 con la inscripción Cancelada.
- Reglas de la inscripción, en este orden:
  - torneo inexistente → 404 `TOURNAMENT_NOT_FOUND` (se reusa);
  - ya tiene una Aceptada en el torneo → 409 `ALREADY_REGISTERED`;
  - Aceptadas == Cupo → 409 `TOURNAMENT_FULL`;
  - en el constructor de `Inscripcion`: torneo que no está Abierto → 409 `TOURNAMENT_NOT_OPEN`, y skills
    obligatorias sin declarar → 409 `MISSING_MANDATORY_SKILLS`.
- Cancelar: inexistente → 404 `REGISTRATION_NOT_FOUND`; ajena → 403 `NOT_REGISTRATION_OWNER`; torneo que no
  está Abierto → 409 `TOURNAMENT_NOT_OPEN`; inscripción que ya no está Aceptada → 409
  `REGISTRATION_NOT_ACTIVE`. `Inscripcion.Cancelar()` no mira el torneo, porque la cancelación del torneo
  (fase 2) cancela inscripciones de torneos EnCurso. `Inscripcion.Eliminar()` es la que usa B.
- **`detalles` lleva la lista.** `ExcepcionConDetalles` es la base de las excepciones que identifican
  elementos: `MISSING_MANDATORY_SKILLS` lista los códigos de las skills que faltan y
  `PLAYERS_MISSING_MANDATORY_SKILL` (409) los nicknames de los inscriptos. El cambio compartido es una línea
  en `FiltroExcepciones.OnException`.
- **Inscripción ajena → 403**, igual que el organizador no asignado (D-40). El organizador de otro torneo
  recibe `NOT_TOURNAMENT_ORGANIZER`.
- **La regla del repertorio vive en `Inscripcion.ExigeSkill(skillId)`**: Aceptada, torneo no Finalizado ni
  Cancelado y la skill es obligatoria. El repositorio solo trae las Aceptadas del jugador con sus torneos.
  Quitar una skill que se exige → 409 `MANDATORY_SKILL_IN_USE`.
- `ContarAceptadasPorTorneo` cuenta en una sola consulta agrupada las Aceptadas de la página de torneos.
- Tabla `Inscripciones` con índice (`TorneoId`, `JugadorId`), estado como texto y FKs `Restrict`: una
  inscripción es historia del torneo y del jugador, no se borra en cascada.
- La migración `AgregaInscripcionesYBatallas` crea también `Batallas` y `Lanzamientos`: el PR de B entró
  sin migración y el snapshot de develop no tenía sus tablas. Una migración solo de inscripciones las
  hubiera incluido igual, porque EF compara contra el modelo completo.

**Justificación:**
- `detalles` ya es parte del cuerpo de error del contrato; un campo nuevo cambiaría el formato para todos.
  Separar los nicknames del mensaje deja al cliente usarlos sin parsear texto.
- 403 sobre 404: el recurso existe y el usuario está autenticado; no hay datos que ocultar, porque el
  organizador ve las inscripciones de su torneo y el jugador las suyas.
- `ExigeSkill` usa solo datos de la inscripción y su torneo (Information Expert). Con la regla en el
  repositorio, el repositorio decidiría (AGENTS §6).
- La validación del torneo y de las skills está en el constructor: no puede existir una inscripción que
  rompa esas reglas, la cree quien la cree.

**Conceptos del curso:** Information Expert, Creator, SRP, contrato REST (403/404/409), formato de error
uniforme, consultas sin N+1, Code First y migraciones.

**Evidencia:**
- `Inscripcion`, `EstadoInscripcion`, `Usuario.SkillsFaltantes` y sus tests de dominio.
- `ServicioInscripciones`, `ServicioSkillsObligatorias`, `ServicioRepertorio` y `ServicioTorneos` con un
  ciclo RED/GREEN por regla.
- `InscripcionesController`, `FiltroExcepciones` y los tests de controller y filtro.
- `RepositorioInscripciones` con SQLite en memoria, `FarmeORTContext.ConfigurarInscripciones` y la migración
  `AgregaInscripcionesYBatallas`.
- La carpeta "Inscripciones" de Postman, que incluye el cupo completo con un torneo de cupo 4.

**Participación de IA:**
- Claude Code (agente de la Persona A) desarrolló los ciclos y planteó las alternativas con una
  recomendación; el integrante aprobó los códigos, el 403 y la excepción con detalles, y pidió incluir
  todas las tablas en la migración y avisar al equipo.
- Problemas que aparecieron:
  - Un test de repositorio armaba una inscripción después de agregar una obligatoria que el jugador no
    tenía: el constructor la rechazó. Se reordenó el arrange para respetar la regla del dominio.
  - Registrar `ServicioTorneos` con la dependencia nueva hizo fallar la validación del contenedor hasta
    registrar el repositorio de inscripciones.
  - `dotnet ef migrations remove` falló sin SQL Server y dejó una migración vacía duplicada. Se borraron los
    archivos sin versionar, se restauró el snapshot y se regeneró.
  - El merge de develop chocó con el código de B en el contexto, el registro de DI y el filtro de
    excepciones. Se conservaron ambos bloques, cada uno en su método o sección.

---

### D-42 — Cancelación del torneo atómica con un solo `SaveChanges` del contexto compartido

**Problema:** la letra (gestión de torneos) pide que, al cancelarse un torneo, sus batallas no Finalizadas y
sus inscripciones Aceptadas pasen a Cancelada. `POST /torneos/{id}/cancelaciones` solo cambiaba el estado
del torneo (pendiente de D-36/D-37). Los tres cambios viven en entidades de tres repositorios distintos
(`Torneo`, `Batalla` de la Persona B e `Inscripcion`) y AGENTS §6 exige que la operación sea atómica: o se
guardan todos o ninguno.

**Alternativas consideradas:**
- **Un solo `SaveChanges`**: los repositorios comparten el `FarmeORTContext` Scoped del pedido; el servicio
  carga las entidades rastreadas, aplica `Cancelar()` en cada una y guarda una sola vez. Es lo que ya hace
  el lanzamiento (D-52).
- **Transacción explícita** detrás de una abstracción nueva en IRepositorios (`ITransaccion` con iniciar y
  confirmar) que envuelva varios `SaveChanges`. Es más explícita, pero es en la práctica una Unit of Work,
  que AGENTS §1.6 pide aprobar y registrar aparte.
- **Un método de repositorio** (`CancelarConEfectos`) que actualice batallas e inscripciones con
  `ExecuteUpdate` dentro de una transacción. La regla (qué estados se cancelan) quedaría en el repositorio y
  se saltearían `Batalla.Cancelar()` e `Inscripcion.Cancelar()`.

**Decisión adoptada:** un solo `SaveChanges` (el integrante eligió la opción recomendada).
- `ServicioTorneos.Cancelar` obtiene el torneo y llama primero a `Torneo.Cancelar()`: si el torneo no se
  puede cancelar (409 `TOURNAMENT_NOT_CANCELLABLE`) no consulta batallas ni inscripciones.
- Después trae las batallas sin resolver con `IRepositorioBatallas.ListarSinResolverDelTorneo` (consulta
  nueva, agregada al final de la interfaz de B como estaba acordado) y las inscripciones Aceptadas con
  `IRepositorioInscripciones.ListarAceptadasDelTorneo`, y llama a `Cancelar()` en cada una. Las reglas
  quedan en las entidades; los repositorios solo devuelven entidades rastreadas.
- Guarda una sola vez con `IRepositorioTorneos.Actualizar(torneo)`: su `SaveChanges` persiste todos los
  cambios rastreados del contexto en una transacción, así que también las batallas y las inscripciones.
- Si otra operación cambió una de esas batallas entre la lectura y el guardado (por ejemplo, el octavo
  lanzamiento la pasó a EnArbitraje), el token de concurrencia del estado (D-52) hace fallar el
  `SaveChanges`, que revierte todo. `RepositorioTorneos` lo traduce a 409 `BATTLE_CHANGED` (se reusa
  `BatallaModificadaException`) y limpia el contexto.
- La respuesta informa 0 inscripciones Aceptadas sin volver a contarlas: la cancelación acaba de pasarlas
  todas a Cancelada.
- No hay cambios en el modelo, así que no hay migración.

**Justificación:**
- `SaveChanges` ya es una unidad atómica de EF Core: no hace falta otra abstracción para un caso de uso que
  usa un solo contexto. Se mantiene la misma solución que D-52, sin Unit of Work ni segundo contexto.
- Cada entidad valida su propia transición (Information Expert): una batalla Finalizada o Cancelada no
  aparece en la consulta, y si apareciera, `Batalla.Cancelar()` la rechazaría.
- Contra: la atomicidad depende de que los repositorios compartan el contexto Scoped. Queda explícita en un
  comentario del servicio, en esta decisión y en un test de repositorio que lo comprueba.
- Limitación aceptada: `Torneo` no tiene token de concurrencia, así que una inscripción que se confirme
  entre la consulta y el guardado quedaría Aceptada en un torneo Cancelado. Cerrar esa ventana necesita un
  token en el torneo y una migración; con el plazo actual no se hace.

**Conceptos del curso:** atomicidad, concurrencia optimista, Information Expert, SRP (el repositorio no
decide reglas), DI Scoped, TDD Outside-In y tests de integración con SQLite.

**Evidencia:**
- `ServicioTorneosTests`: cancela la batalla y la inscripción **antes** del único `Actualizar` (se mira el
  estado en el momento del guardado), responde cero aceptadas sin volver a contarlas, y un torneo ya
  cancelado no toca batallas ni inscripciones.
- `RepositorioBatallasTests`: `ListarSinResolverDelTorneo` devuelve rastreadas solo las Pendientes y
  EnArbitraje de ese torneo.
- `RepositorioTorneosTests`: `Actualizar` guarda junto con el torneo la batalla y la inscripción canceladas;
  si otro contexto cambió la batalla, lanza `BatallaModificadaException` y no guarda ninguno de los tres
  cambios.
- Secuencia en `docs/uml/cancelar-torneo.puml` y contrato en `docs/api/torneos.md`.

**Participación de IA:**
- Claude Code (agente de la Persona A) planteó las tres alternativas con una recomendación; el integrante
  eligió el `SaveChanges` único. Se descartó la transacción explícita para no introducir una Unit of Work
  sin aprobación del equipo.
- Claude Code propuso además traducir el conflicto de concurrencia: sin eso, la carrera con un lanzamiento
  respondía 500. También detectó que el test existente que esperaba la cantidad de Aceptadas después de
  cancelar describía un caso imposible, y lo reemplazó en un RED.
- Problema que apareció: el primer arrange del test de repositorio creaba la batalla con jugadores sin
  guardar, y el constructor la rechazó porque los dos tenían id 0. Se guardan los jugadores primero.

---

### D-43 — Restricciones para eliminar a un usuario o cambiarle el rol

**Problema:** la letra (usuarios) dice que un usuario no se puede eliminar si tiene batallas sin resolver
o si organiza un torneo que no está Finalizado ni Cancelado. `DELETE /usuarios/{id}` (D-27) no lo
controlaba porque esas entidades no existían. Además quedaba pendiente qué pasa al cambiar el rol
(`PUT /usuarios/{id}`) de un usuario con datos asociados: un inscripto que pasa a Organizador rompería la
creación de batallas, que exige rol Jugador, y un organizador que pasa a Jugador dejaría un torneo
organizado por un Jugador.

**Alternativas consideradas:**
- Errores: una excepción genérica con mensajes distintos, o una por caso con su `errorCode`.
- Cambio de rol: sin restricciones (supuesto documentado), las mismas reglas que eliminar, o bloquearlo
  si el usuario tiene cualquier dato asociado, incluso histórico.
- Dónde vive "vigente": filtrar en el repositorio por estado, o pedirle a la entidad.

**Decisión adoptada:**
- `Eliminar` → 409 si organiza un torneo vigente (`USER_ORGANIZES_ACTIVE_TOURNAMENT`) o si juega una
  batalla Pendiente o EnArbitraje (`USER_HAS_UNRESOLVED_BATTLES`). Una excepción por caso, como pidió el
  equipo.
- Cambiar el rol: el integrante eligió las mismas reglas que eliminar, más una inscripción Aceptada en un
  torneo vigente → 409 `USER_HAS_ACTIVE_REGISTRATION`. Solo se controla si el rol cambia: modificar el
  nombre o el email de un organizador con un torneo en curso sigue permitido.
- **Supuesto (no está en la letra):** eliminar también se rechaza con `USER_HAS_ACTIVE_REGISTRATION` si el
  jugador tiene una inscripción Aceptada en un torneo vigente. Si no, quedaría inscripto un usuario dado de
  baja y las rondas lo emparejarían. El integrante lo eligió frente a cancelar la inscripción en la baja, que
  agregaba un efecto que la letra no menciona. Así, eliminar y cambiar el rol comparten una sola validación.
- Consultas al final de las interfaces: `IRepositorioTorneos.ExisteVigenteDelOrganizador` y
  `IRepositorioBatallas.ExisteSinResolverDelJugador` (de B, agregar consultas estaba acordado). Son `Any`
  en la base: no traen entidades para contestar sí o no.
- La inscripción vigente la decide `Inscripcion.EstaVigente` (Aceptada y torneo no Finalizado ni
  Cancelado), extraída en un REFACTOR de `ExigeSkill`, que ya tenía esa regla. El servicio reusa
  `ListarAceptadasDelJugador`.
- Los tres mensajes son genéricos: sirven tanto para la baja como para el cambio de rol.

**Justificación:**
- Un `errorCode` por caso le dice al cliente qué tiene que resolver antes de reintentar.
- Las mismas reglas que eliminar cubren los dos riesgos del cambio de rol sin bloquear para siempre a
  quien solo tiene historial (torneos finalizados, batallas resueltas).
- `EstaVigente` evita repetir la regla de estados en el servicio (Information Expert, DRY). Para torneos y
  batallas el filtro queda en la consulta: es una consulta de existencia, y `Torneo` es de C en esta ola.

**Conceptos del curso:** contrato REST (409 con `errorCode`), Information Expert, SRP, TDD Outside-In
(servicio con mocks y repositorios con SQLite) y refactor sin cambiar comportamiento.

**Evidencia:**
- `ServicioEliminacionUsuariosTests`: organizador de torneo vigente, jugador con batallas sin resolver y
  jugador inscripto en un torneo vigente.
- `ServicioModificacionUsuariosTests`: las tres reglas al cambiar el rol, el mismo rol sin controles y una
  inscripción de un torneo cancelado que no impide el cambio.
- `RepositorioTorneosTests` y `RepositorioBatallasTests`: un `DataRow` por estado.
- `FiltroExcepcionesTests`: los tres códigos.
- La carpeta "Usuarios" de Postman, con el organizador de un torneo abierto y el jugador inscripto. Las
  batallas todavía no se pueden crear por la API (las crean las rondas de C).

**Participación de IA:**
- Claude Code (agente de la Persona A) propuso las tres opciones del cambio de rol con una recomendación;
  el integrante eligió las mismas reglas que eliminar. Los nombres de las excepciones y los `errorCode`
  los propuso Claude Code siguiendo la convención UPPER_SNAKE en inglés.
- Claude Code detectó el riesgo de las inscripciones en la baja, que no está en la letra, y lo consultó
  antes de implementarlo; el integrante eligió rechazarla con 409.
- Problema que apareció: en los tests de modificación el usuario existente era Organizador y el pedido
  cambiaba a Jugador, así que todos cambiaban el rol. Hubo que dejar valores por defecto en
  `[TestInitialize]` para que siguieran describiendo lo mismo con los mocks estrictos.

---

### D-44 — El detalle del torneo suma sus skills obligatorias y sus jugadores aceptados

**Problema:** la letra (1.3, detalle de un torneo) pide devolver, además del resumen, las skills
obligatorias, la lista de jugadores aceptados con nickname y ELO actual, y el cuadro completo. D-36 dejó
`TorneoDetalleResponse` sin propiedades propias hasta que existieran esas entidades. El cuadro (rondas y
batallas) lo agrega la Persona C después; este cambio suma las otras dos partes.

**Alternativas consideradas:**
- Skills obligatorias: un DTO nuevo con menos campos o reusar `SkillResponse`, que ya devuelve
  `GET /torneos/{id}/skills-obligatorias`.
- Jugadores: reusar `InscripcionResponse` (con id de inscripción, torneo, estado y fecha) o un DTO propio
  con lo que pide la letra.
- Orden de los jugadores: por ELO, por nickname o por orden de inscripción.
- Cómo completar el detalle sin repetir el mapeo común con el resumen: propiedades `init` y un mapeo
  aparte del detalle, o propiedades propias con `set`, que se completan después del mapeo común.

**Decisión adoptada:**
- `skillsObligatorias`: `SkillResponse[]`, el mismo formato que el sub-recurso de skills obligatorias.
- `jugadoresAceptados`: `JugadorAceptadoResponse[]` con `jugadorId`, `nickname` y `elo`, en orden de
  inscripción. Es el orden de `ListarAceptadasDelTorneo`, que ordena por id en la consulta y no depende
  del orden accidental de la base.
- Una sola consulta de las Aceptadas sirve para la lista y para `inscripcionesAceptadas`. El detalle ya no
  hace un `COUNT` aparte.
- Las dos propiedades nuevas tienen `set`: `CrearResponse<T>` comparte el mapeo del resumen y el servicio
  completa el detalle después. Las propiedades del resumen siguen siendo `init`.
- `RespuestasSkills.Crear` reemplaza el mapeo de `Skill` a `SkillResponse` que repetían `ServicioSkills`,
  `ServicioSkillsObligatorias` y ahora `ServicioTorneos`.
- La visibilidad no cambia (D-36): el anónimo ve el detalle de los torneos no cancelados, con los mismos
  datos que un usuario autenticado. El ranking ya es público, así que nickname y ELO no son datos
  reservados.

**Justificación:**
- Reusar `SkillResponse` evita dos formatos para la misma skill. Un DTO propio para los jugadores expone
  solo lo que pide la letra, sin datos de la inscripción.
- El orden de inscripción es estable y no cambia cuando cambia un ELO. El ranking ya ordena por ELO.
- `set` en dos propiedades es más simple que repetir once asignaciones del resumen en un mapeo propio del
  detalle. La Persona C puede sumar el cuadro de la misma forma.

**Conceptos del curso:** contrato REST (una sola llamada con todo el detalle), DTOs sin entidades, DRY,
Information Expert y refactor sin cambiar comportamiento.

**Evidencia:**
- `ServicioTorneosTests`: un ciclo para las skills obligatorias y otro para los jugadores aceptados.
  Además, dos REFACTOR: `RespuestasSkills` y la consulta única. El test de la cantidad de aceptadas
  cambió solo su arrange.
- `TorneoDetalleResponse`, `JugadorAceptadoResponse` y `RespuestasSkills`.
- Postman: "Inscripciones - el detalle del torneo muestra obligatorias y jugadores aceptados", como
  anónimo.

**Participación de IA:**
- Claude Code (agente de la Persona A) propuso el formato de las dos listas, el orden de inscripción y
  reusar `SkillResponse`; el integrante lo revisa en el PR.
- Problema que apareció: con `init`, el detalle no se podía completar después del mapeo genérico del
  resumen. Se eligió `set` en las dos propiedades nuevas y se documentó en el DTO.

---

### D-45 — Ranking de un torneo: participantes Aceptados y Eliminados, y 404 para inexistente o cancelado

**Problema:** la letra (2.6) permite filtrar el ranking por torneo, y la especificación de la API pide
`torneoId` con 404 si el torneo no existe. D-61 dejó pendiente qué jugadores del torneo entran. El ranking
pasó a la Persona A en la ola 2, y D-61 sigue valiendo: el desempate es por id y el orden va en la
consulta. Las batallas ganadas y perdidas siguen en 0 hasta que los fallos de la Persona B lleguen a
develop.

**Alternativas consideradas:**
- Quién entra: los que tienen inscripción Aceptada o Eliminada, solo los que jugaron al menos una
  batalla, o toda inscripción incluidas las Canceladas.
- Torneo Cancelado: devolver su ranking, o responder 404 como el detalle del torneo para el anónimo
  (D-36).
- `torneoId` no positivo: ignorarlo (como un filtro inválido de un listado, D-35) o tratarlo como un torneo
  que no existe.
- La consulta: un parámetro opcional en `ListarRanking` o una consulta nueva que comparta el orden.

**Decisión adoptada:**
- Entran los jugadores sin baja con inscripción **Aceptada o Eliminada** en el torneo (lo eligió el
  integrante). Las Canceladas no entran, porque se retiraron antes de jugar. El ELO es el actual, que es
  global.
- Torneo inexistente o **Cancelado** → 404 `TOURNAMENT_NOT_FOUND`. El ranking es público y no distingue
  quién consulta, y para el público un torneo cancelado no existe (D-36).
- Un `torneoId` que no existe, incluso 0 o negativo, responde 404. La especificación pide 404 para un
  torneo inexistente; ignorar el filtro devolvería el ranking general como si fuera el del torneo. Un
  `torneoId` que no es un número sigue siendo 400 `VALIDATION_ERROR`.
- `IRepositorioUsuarios.ListarRankingDelTorneo(torneoId, cantidad)` es una consulta nueva. Comparte con
  `ListarRanking` el filtro de jugadores activos y el orden (ELO descendente y id ascendente, D-61), que se
  extrajeron en un REFACTOR. La cantidad se ajusta igual que sin filtro.
- Con el filtro, ganadas y perdidas van a contar solo las batallas de ese torneo, y sin filtro todas (lo
  eligió el integrante). Se calculan con `Batalla.GanadorId` cuando los fallos de B estén en develop.

**Justificación:**
- Aceptados y Eliminados son quienes compiten o compitieron en el torneo: el ranking muestra a todos los
  participantes, también a los ya eliminados. Exigir batallas jugadas dejaría vacío el ranking de un
  torneo Abierto.
- El 404 del cancelado mantiene una sola regla de visibilidad para lo público.
- Una consulta nueva evita un parámetro opcional, que Moq no admite en sus expresiones, y el orden
  compartido garantiza el mismo desempate en los dos rankings.

**Conceptos del curso:** REST (filtro en la query, 404 para un recurso referenciado inexistente),
determinismo, TDD Outside-In, tests de repositorio con SQLite y DRY.

**Evidencia:**
- `ServicioRankingTests`: torneo inexistente, torneo cancelado y ranking de los participantes, cada uno con
  su RED y su GREEN, más un REFACTOR del servicio.
- `RepositorioRankingTests`: Aceptados y Eliminados por ELO, sin Cancelados, sin otros torneos, sin
  inscriptos ni dados de baja. Además, un test que documenta el desempate por id con el índice por ELO de
  D-61 (se comprobó que falla sin `ThenBy`) y la cantidad.
- Postman, carpeta "Ranking": los cuatro aceptados del torneo de cupo 4, la cantidad, un torneo inexistente,
  un torneo cancelado y `torneoId=abc`.

**Pendiente:** `batallasGanadas` y `batallasPerdidas` (ver arriba).

**Participación de IA:**
- Claude Code (agente de la Persona A) propuso las opciones de quién entra y el conteo por torneo; el
  integrante eligió las recomendadas. El 404 del torneo cancelado y el del `torneoId` no positivo los
  propuso Claude Code siguiendo D-36 y la especificación; el integrante los aprobó antes del PR.
- Para que el test del desempate no pasara por casualidad, Claude Code quitó `ThenBy` a propósito, vio
  fallar los dos tests de desempate y restauró el código antes de commitear.

---

### D-50 — Coordinación del alcance de Persona B

**Problema:** A, B y C trabajan en paralelo sobre un modelo compartido. Numerar decisiones y mantener migraciones sin coordinación puede causar conflictos y respuestas distintas para la misma autorización por torneo.

**Alternativas consideradas:** numeración consecutiva compartida o rangos por persona; resolver a mano el snapshot o regenerar únicamente la migración propia todavía no integrada. Para el organizador no asignado se consideraron 403 y 409; el acuerdo HTTP ya está registrado en D-40 y no se redefine aquí.

**Decisión adoptada:** el usuario confirmó D-50 a D-59 para B y permite quitar/regenerar solo una migración propia que nunca llegó a develop. Se genera una por PR al final, con develop incorporado; las integradas se preservan. B reutilizará D-40: 403 `NOT_TOURNAMENT_ORGANIZER` mediante la excepción existente cuando el organizador no sea el asignado, sin volver a registrar esa decisión común. Los filtros siguen comprobando autenticación y rol.

**Justificación:** los rangos evitan colisiones; regenerar la migración no integrada evita duplicar cambios del snapshot recibido. Referenciar D-40 mantiene una sola definición de la autorización compartida.

**Conceptos del curso:** trabajo concurrente en GitFlow, Code First, trazabilidad y autorización por recurso.

**Evidencia:** confirmación explícita del usuario en el chat de Persona B e incorporación de develop `d2a5bd1` (PR #13). No se generó una migración de B ni se empezó fallos: espera `Inscripcion` e `ICalculadorElo` en develop.

**Participación de IA:** Codex comprobó las fuentes y consultó los acuerdos pendientes; el usuario confirmó los tres. Al recibir D-40 se reemplazó la definición duplicada de autorización por su referencia. La autoría Git corresponde exclusivamente al integrante; esta declaración académica queda separada.

---

### D-51 — Batalla como experto de turnos y sorteo inyectable

**Problema:** C crea las batallas al generar rondas, A las cancela al cancelar el torneo y B registra lanzamientos. Las reglas de turnos y estados tienen que ser consistentes para todos esos consumidores, con pruebas deterministas del empate.

**Alternativas consideradas:** reglas y azar directamente en el servicio; entidad que conozca una implementación aleatoria; entidad que valide sus datos y reciba `ISorteo` solo en la creación.

**Decisión adoptada:** `Batalla(Torneo, int, Usuario, Usuario, ISorteo)` conserva torneo, ronda y dos jugadores distintos de rol Jugador. Guarda `JugadorInicialId` según los ELO de creación; solo en empate llama al sorteo. `ISorteo` vive en Dominio/Batallas y `SorteoAleatorio` en Servicios/Batallas, registrado Scoped. No se modifica `Torneo`, no se crean rondas ni se implementa una fórmula ELO desde B.

La entidad registra sus lanzamientos y aplica estado Pendiente, participación, turno, repertorio y repetición por **jugador y skill**. El rival puede lanzar la misma skill: prohibirlo globalmente impediría que ambos cumplan las obligatorias. Una skill desactivada después de declararla sigue siendo válida. El lanzamiento ocho deja cuatro por jugador y pasa a EnArbitraje; `Cancelar()` acepta Pendiente/EnArbitraje y no cambia estados finales. El servicio carga las dependencias, usa `TimeProvider.GetUtcNow()` y pide un guardado después de validar. Al recibir el modelo de A en `d2a5bd1`, B usa `Torneo.SkillsObligatorias` sin modificar Torneo: cuando las pendientes de ese jugador igualan sus turnos restantes, solo admite una de esas pendientes. El repositorio carga la asociación aunque la skill se haya desactivado.

**Justificación:** las reglas que usan datos de la batalla quedan con el experto; las dependencias de acceso a datos y reloj permanecen fuera del dominio. El azar se reemplaza por un mock o doble preparado en tests. No hace falta una jerarquía State para estos cuatro estados.

**Conceptos del curso:** Information Expert, Creator, Protected Variations, DIP, SRP, encapsulación, DI Scoped y TDD Outside-In.

**Evidencia:** `BatallaTests`, `ServicioLanzamientosTests`, `SorteoAleatorioTests` y `RegistroBatallasTests`, ciclos RED/GREEN separados. Clases de fase 1 y secuencia de lanzamiento en `docs/uml/`, contrastadas con el código; render pendiente. El fallo se identifica como diseño futuro, sin implementación. Contrato y estado real en `docs/api/batallas-lanzamientos.md`.

**Participación de IA:** Codex desarrolló los ciclos y propuso mantener las reglas en Batalla siguiendo el contrato explícito del usuario. Se aceptaron el sorteo inyectable, la no retroactividad y las excepciones existentes para datos/recursos inexistentes. No se aceptó implementar inscripciones, el modelo de obligatorias de A o la fórmula ELO de C. No existe una excepción genérica de negocio reutilizable en el código actual: se agregan tipos para distinguir los conflictos de lanzamiento y mapear sus códigos sin alterar errores de otros recursos.

---

### D-52 — Guardado atómico y conflictos concurrentes de lanzamientos

**Problema:** validar el turno en memoria no evita que dos pedidos acepten simultáneamente el mismo turno, ni que una cancelación llegue entre la lectura y el guardado. Un error debe revertir el lanzamiento y los cambios de la batalla, sin responder éxito ni ocultar errores distintos.

**Alternativas consideradas:** validación solo en memoria; bloqueo manual o versión adicional; restricciones de unicidad y concurrencia optimista de EF Core sobre el estado, aprovechando un único contexto Scoped.

**Decisión adoptada:** el repositorio recupera una batalla rastreada con torneo, organizador, obligatorias, jugadores e historial ordenado. ServicioLanzamientos realiza un solo `Guardar()` y el repositorio un `SaveChanges()` para todos sus cambios. El estado es token de concurrencia; `(BatallaId, Turno)` y `(BatallaId, JugadorId, SkillId)` son únicos y el turno se restringe a 1..8. No se introduce Unit of Work ni un segundo contexto en el caso de uso.

Un `DbUpdateConcurrencyException` se traduce a `BatallaModificadaException` (409 `BATTLE_CHANGED`). Ante `DbUpdateException`, se consulta si hay un turno o una skill de ese jugador ya persistidos y se traduce al conflicto correspondiente, siguiendo D-23; si no, se propaga el error original. Antes de devolver un conflicto traducido se limpia el tracking para no conservar cambios rechazados.

**Justificación:** el índice único impide dos lanzamientos de un turno; el token de estado impide guardar contra una cancelación o resolución concurrente. `SaveChanges` revierte todos sus cambios cuando falla. Las consultas de traducción evitan acoplarse a códigos de SQL Server y funcionan también en los tests SQLite. Las restricciones de B se configuraron en un método privado propio al final del contexto, conservando la configuración y migración integradas de A. La migración de B queda para el cierre de la rama.

**Conceptos del curso:** Code First, atomicidad, concurrencia optimista, Information Expert, persistencia separada del dominio, DI Scoped y pruebas de integración con un fake SQLite.

**Evidencia:** `RepositorioBatallasTests`: guardado conjunto, duplicados por turno/skill, turnos fuera de rango, cancelación desde otro contexto, rollback y propagación de un error no traducible. `ServicioLanzamientosTests`: guardado una vez, rechazo sin guardar y conflicto propagado sin respuesta exitosa. Los códigos tienen pruebas independientes en `FiltroExcepcionesBatallasTests`.

**Participación de IA:** Codex propuso las restricciones y el token de estado y verificó cada cambio mediante ciclos RED/GREEN. Se aceptó la protección concurrente dentro de las entidades de B y se descartaron bloqueos manuales, fórmulas ELO y cambios al modelo de A. La revisión humana y las Actions quedan pendientes; no se sustituyen por estas validaciones locales.

---

### D-53 — Fallo opcional embebido e historial de ELO por batalla

**Problema:** A y C necesitan consultar `GanadorId` y `Estado`; el detalle debe conservar comentario y ELO ganado/perdido aunque cambie el ELO actual en rondas posteriores. La letra permite resolver una batalla Pendiente por abandono o EnArbitraje por decisión del organizador.

**Alternativas consideradas:** entidad `Fallo` 0..1 con tabla y repositorio propios; propiedades opcionales en `Batalla`; calcular el cambio histórico restando el ELO actual al inicial. La última alternativa pierde el resultado cuando hay otras batallas y confunde ELO al crear con ELO al fallar.

**Decisión adoptada:** el fallo se representa con `GanadorId: int?`, `Ganador: Usuario?`, `Comentario: string?` y los cambios firmados `CambioEloJugador1/2: int?` dentro de `Batalla`. `Fallar` fija esos datos, pasa a Finalizada y elimina el turno. No se introduce una entidad/recurso consultable adicional. Los cambios se calculan a partir de los ELO previos al fallo y se persisten; el ELO de cada jugador en la respuesta sigue siendo el actual.

`Batalla.ValidarFallo` controla estado, participación y comentario de 10 a 500 caracteres no blanco. El servicio lo invoca antes de los efectos; `Fallar` lo vuelve a invocar para proteger llamadas directas al dominio. El request valida presencia, id positivo y longitud en el borde. El organizador ajeno reutiliza D-40. `BATTLE_NOT_JUDGEABLE` distingue el estado no fallable del estado no lanzable. El mapeo queda en `RespuestasBatallas`, dentro de Servicios, como en los otros casos de uso (D-12).

**Justificación:** hay como máximo un fallo por batalla y no tiene ciclo de vida ni consultas propias; las propiedades reducen tablas y dependencias sin perder historial. La FK del ganador es opcional, explícita y Restrict; el comentario tiene máximo 500 en EF. Information Expert concentra las reglas en Batalla, y la representación HTTP permanece en DTOs.

**Conceptos del curso:** Information Expert, alta cohesión, bajo acoplamiento, SRP, validación en borde/dominio, REST y trazabilidad de un resultado histórico.

**Evidencia:** `ServicioFallosTests` cubre ambos ganadores y estados válidos, el orden de efectos, ELO previos, historial y rechazos; `BatallaTests` comprueba límites y llamadas directas; `RepositorioBatallasTests` recarga el fallo y su FK; especificación y diagramas en `docs/api/batallas-lanzamientos.md`, `docs/uml/fallar-batalla.puml` y `docs/uml/batallas-lanzamientos.puml`.

**Participación de IA:** Codex contrastó letra, tabla API, AGENTS y skills da2 con el código integrado. Se eligieron las propiedades opcionales dentro de la alternativa que autorizó el usuario y se descartó introducir una entidad Fallo con repositorio. Se agregó el historial de cambios porque la letra pide ELO ganado/perdido y el ELO actual no lo conserva. Los tests guiaron cada implementación y el refactor separó el mapeo sin cambiar comportamiento. El primer arreglo de un test de datos inválidos no configuraba el repositorio y se corrigió antes de guardar la evidencia RED. Autor y committer Git son exclusivamente el integrante; esta declaración académica no atribuye autoría Git a la herramienta.

---

### D-54 — Un guardado para fallo, ELO y eliminación, con concurrencia optimista

**Problema:** guardar los ELO, el resultado y la eliminación por separado puede dejar una batalla Finalizada con ELO incorrecto o un perdedor que sigue aceptado. Un ELO leído puede cambiar antes del guardado, incluso sin cambiar el estado de esa batalla.

**Alternativas consideradas:** varios guardados con transacción explícita y una nueva abstracción; un único `SaveChanges` del contexto Scoped compartido; actualizaciones directas en SQL. Para concurrencia: solo el token existente de `Batalla.Estado`, bloqueo pesimista o sumar el ELO como token.

**Decisión adoptada:** reutilizar el único guardado de D-52/D-42. `ObtenerAceptada(torneoId, jugadorId)`, al final de la interfaz de Inscripciones, obtiene rastreada la inscripción del perdedor sin guardar. Todas las precondiciones se verifican antes de mutar. El calculador inyectado recibe los dos ELO previos en una única llamada; luego `Usuario.ActualizarElo` actualiza ambos, `Batalla.Fallar` registra resultado y Finalizada, `Inscripcion.Eliminar` aplica la derrota y `IRepositorioBatallas.Guardar` ejecuta el único `SaveChanges`. No se llama a `IRepositorioInscripciones.Actualizar`, que guardaría por separado.

El ELO de Usuario se configura como token de concurrencia, además del estado de Batalla existente. Un conflicto revierte el guardado completo, limpia el seguimiento y se traduce a `BATTLE_CHANGED`. El cliente debe consultar otra vez; no se reintenta automáticamente con valores antiguos. `ActualizarElo` rechaza valores menores a 100; el calculador base de C conserva sus reglas y no se modifica. El perdedor sin inscripción Aceptada produce `REGISTRATION_NOT_ACTIVE` antes del cálculo.

**Justificación:** EF Core ya ofrece atomicidad para un `SaveChanges`; no hace falta Unit of Work ni una transacción nueva. La atomicidad depende del contexto Scoped común registrado por la fábrica. El token ELO evita una actualización perdida entre batallas; el estado impide aplicar un segundo fallo o fallar una batalla cancelada concurrentemente. No modifica el proceso de rondas de C ni las restricciones de usuarios de A.

**Conceptos del curso:** transacciones, concurrencia optimista, DI Scoped, DIP/OCP mediante `ICalculadorElo` (D-60), Information Expert y TDD Outside-In con SQLite.

**Evidencia:** el test del servicio inspecciona todos los efectos en el momento del único guardado y usa un mock estricto de `ICalculadorElo` con ELO previos distintos. Los tests SQLite prueban persistencia conjunta y rollback ante cancelación y ELO concurrentes, preservando la modificación ajena. PR #19 se incorporó antes de la migración y se conserva `ExisteSinResolverDelJugador`, la consulta de A.

**Participación de IA:** Codex aplicó la atomicidad aprobada en el prompt y contrastó el patrón con D-42/D-52. Un RED demostró que solo el estado no evitaba sobrescribir un ELO concurrente; se agregó el token mínimo y se reutilizó la traducción de conflicto existente. Se descartaron transacciones con abstracciones nuevas y SQL que omitiera las reglas del dominio. Se conservó la migración integrada `AgregaInscripcionesYBatallas`; la migración nueva agrega únicamente el fallo y el modelo de concurrencia de ELO.

---

### D-55 — Visibilidad histórica por torneo y consultas filtradas antes de paginar

**Problema:** la letra 1.7 restringe las consultas por torneo, no por la pareja de una batalla. El fallo elimina la inscripción del perdedor; restringir a Aceptadas haría desaparecer inmediatamente su historial. El filtro jugadorId proviene del cliente y no puede usarse como identidad para autorizar.

**Alternativas consideradas:** limitar a las batallas donde juega el usuario; exigir una inscripción Aceptada; admitir cualquier inscripción en el torneo y separar autorización de filtros. Para el listado: filtrar/paginar en memoria o realizar ambas operaciones en SQL con orden explícito.

**Decisión adoptada:** los tres GET autorizan al Jugador mediante `ExisteDelJugador(torneoId, usuarioId)` sin filtrar estado de inscripción; al Organizador mediante la asignación de D-40; al Administrador para cualquier torneo. La identidad y el rol vienen de la sesión autenticada. La consulta histórica conserva acceso para Aceptada, Eliminada o Cancelada, conforme al prompt «tiene inscripción». El jugador sin inscripción recibe 403 `PLAYER_NOT_IN_TOURNAMENT`. Primero se comprueba existencia del recurso; después se autoriza; al final se aplican filtros y se crean DTOs.

`FiltroBatallas` transporta filtros normalizados al repositorio. `Listar` aplica torneo, estado, ronda y cualquiera de los dos jugadores, ordena por NumeroRonda e Id y recién entonces usa Skip/Take en SQL. Reutiliza Paginacion y ValoresPorNombre (D-35); la consulta del listado usa AsNoTracking. Las consultas nuevas se agregan al final de las interfaces, conservando las de A. El detalle reutiliza el fallo y el historial de D-53; mapeo y carga autorizada están compartidos entre los GET.

**Justificación:** el ámbito de visibilidad es estable después del fallo y el cliente no puede ampliar permisos modificando filtros. La paginación tiene resultados deterministas y limita lo leído de la base. Las consultas no guardan ni mutan entidades. `HistorialLanzamientosController` protege el GET con los tres roles; el controller de POST conserva exclusivamente Jugador sin que su atributo restrinja las consultas.

**Conceptos del curso:** alta cohesión, bajo acoplamiento, SRP, DI, autorización a nivel de recurso, REST, separación de responsabilidades y TDD Outside-In con Moq/SQLite.

**Evidencia:** `BatallasControllerTests`, `HistorialLanzamientosControllerTests`, `ServicioBatallasTests`, tests SQLite de ambas consultas, filtro de excepciones y registro Scoped. Los tests cubren un jugador inscripto que no pertenece a la pareja, jugadores ajenos, organizadores ajenos, administrador, estados de inscripción y filtrado/orden/paginación. PR #18–#21 de A incorporados conservando cancelación, restricciones, detalle y filtro del ranking; B no implementa los conteos del ranking ni las rondas.

**Participación de IA:** Codex contrastó el prompt, letra, contrato API, AGENTS y skills con los cambios integrados de A. Propuso separar el permiso por torneo del filtro jugadorId, retener acceso histórico y reutilizar los normalizadores existentes. Se descartaron permisos basados en una batalla o solo en Aceptadas porque contradicen el ámbito solicitado y eliminan el historial del perdedor. RED/GREEN guiaron controllers, servicio, consultas, errores y DI; un refactor real extrajo carga autorizada y mapeo. El integrante conserva la autoría Git exclusiva. C aún no dispone de rondas en develop: la ejecución integrada de Postman requiere sus fixtures y no se declara verificada. Las carpetas B se validaron por separado con Newman sobre la API real y SQLite con batallas manuales: 82 requests, 174 assertions, sin fallos. Se conservan todos los saltos ajenos y se documentan las variables que debe producir Rondas.

---

### D-60 — El cálculo de ELO es un punto de variación (Strategy): `ICalculadorElo` ← `CalculadorEloBase`

**Problema:** al fallar una batalla hay que calcular el nuevo ELO de los dos jugadores (letra: K = 32,
`Ea = 1 / (1 + 10^((Rb − Ra) / 400))`, `Ra' = Ra + K·(S − Ea)`, piso 100). El diseño del equipo
(`AGENTS.md` y el diagrama de clases) lo marca como punto de variación: tiene que poder agregarse otra
forma de calcularlo sin modificar el código que finaliza la batalla. Fallar batalla lo implementa otra
persona del equipo, que necesita el cálculo antes de que exista la batalla.

**Alternativas consideradas:**
- Un método privado en el servicio de batallas: es lo más simple, pero cambiar la política obliga a
  modificar ese servicio.
- Template Method: una clase base abstracta en la que las subclases redefinen un paso (por ejemplo, el
  K). Alcanza si solo cambia un paso, pero acopla por herencia y no sirve para una política con otra
  fórmula.
- Strategy: una interfaz con una implementación por política, inyectada por DI.

**Decisión adoptada:**
- Strategy. `ICalculadorElo.Calcular(eloGanador, eloPerdedor)` devuelve
  `ResultadoElo(NuevoEloGanador, NuevoEloPerdedor)`. Como los dos valores nuevos salen juntos, quien lo
  usa no puede calcular uno con el valor ya actualizado del otro (el error clásico).
- `CalculadorEloBase` implementa la política de la letra: K = 32, S = 1 para el ganador y 0 para el
  perdedor, los dos con los valores previos, redondeo al entero más cercano y piso de 100. Con ELO
  iguales el cambio es exactamente 16; en los demás casos `Ea` no deja un ,5 exacto, así que el
  criterio de desempate del redondeo no influye.
- La interfaz, el resultado y la implementación viven en `FarmeORT.Dominio/Elo/`: es una regla del
  dominio y no depende de infraestructura.
- `CalculadorEloBase` es `public sealed`. Es público porque lo registra `RegistroServicios`, que está en
  otro assembly, igual que las entidades del dominio. Es sellado porque una política nueva implementa
  `ICalculadorElo` en lugar de heredar. "Base" quiere decir "la política base de la letra", no una
  clase base.
- Se registra Scoped en `RegistroServicios`, en un bloque propio, como el resto de los servicios.
- Para agregar una política: una clase nueva que implemente `ICalculadorElo` y cambiar la línea del
  registro. El servicio de batallas no se toca.

**Justificación:** OCP y Protected Variations: la política de ELO puede cambiar y queda aislada detrás
de una interfaz. DIP: fallar batalla depende de la abstracción, y en sus tests se reemplaza por un
mock. El cálculo se prueba aparte, con valores conocidos.

**Conceptos del curso:** Strategy, OCP, DIP, Protected Variations y Polymorphism (GRASP), inyección de
dependencias, TDD con "fake it" y triangulación con `DataRow`.

**Evidencia:**
- `FarmeORT.Dominio/Elo/`: `ICalculadorElo`, `ResultadoElo` y `CalculadorEloBase`.
- `CalculadorEloBaseTests`, con un ciclo por comportamiento: ELO iguales, ELO distintos con los valores
  previos y redondeo, y piso de 100. Los valores esperados de los ELO distintos (1408/1192 y 1224/1376)
  fallan si se trunca en lugar de redondear o si el perdedor se calcula con el ELO nuevo del ganador
  (daría 1193 y 1377).
- `RegistroServicios` y `RegistroServiciosTests`.
- Pendiente: agregar `ICalculadorElo`, `ResultadoElo` y `CalculadorEloBase` al diagrama de clases.

**Participación de IA:**
- La firma de `ICalculadorElo` y `ResultadoElo` la acordó el equipo como contrato compartido entre las
  tres personas; Claude Code la implementó tal cual.
- Claude Code propuso el plan de ciclos, el "fake it" del primer GREEN para que cada test agregue un
  comportamiento y la ubicación en Dominio como `public sealed`. El integrante revisó el primer RED y
  el plan, y aprobó el resto de las etapas en bloque.
- Claude Code calculó los valores esperados con un script y eligió casos que distinguen redondear de
  truncar y que detectan el error clásico de usar el ELO ya actualizado del ganador.

---

### D-61 — Ranking: solo jugadores sin baja, ELO de mayor a menor con desempate por id

**Problema:** la letra pide un ranking público con una cantidad (10 por defecto) y un filtro opcional
por torneo, ordenado por ELO de mayor a menor, que devuelva posición, nickname, ELO, batallas ganadas y
batallas perdidas. La especificación de la API los define como `cantidad` y `torneoId`, con los estados
200, 400 y 404. Hay que decidir el desempate, quién entra, qué hacer con una cantidad fuera de rango y
qué devolver en ganadas y perdidas mientras no se puedan fallar batallas, que otra persona implementa
en paralelo.

**Alternativas consideradas:**
- Desempate: por id (va primero el que se registró antes), por nickname o por batallas ganadas.
- Dónde ordenar: en la consulta del repositorio o en el servicio, trayendo a todos los jugadores.
- Ganadas y perdidas: devolverlas en 0 hasta que se puedan fallar batallas, o sumar los campos recién
  cuando se puedan calcular.

**Decisión adoptada:**
- Contrato:
  - `GET /api/v1/ranking?cantidad=N`, público: no pide token.
  - Responde 200 con una lista de `JugadorRankingResponse`: `posicion`, `nickname`, `elo`,
    `batallasGanadas` y `batallasPerdidas`.
  - Una `cantidad` que no es un número (`cantidad=abc`) responde 400 `VALIDATION_ERROR`.
  - El filtro `torneoId` va en un PR aparte, junto con las rondas. Hasta entonces solo se responde 200 o
    400; con el filtro, un torneo inexistente va a responder 404 `TOURNAMENT_NOT_FOUND`. No hay 401, 403
    ni 409 porque el recurso es público.
- Entran solo los usuarios con rol Jugador y sin baja lógica (D-27): organizadores y administradores no
  juegan batallas.
- Orden: ELO de mayor a menor y, si empatan, id de menor a mayor. La posición es el lugar en la lista
  (1, 2, 3…), así que dos jugadores con el mismo ELO tienen posiciones distintas.
- El orden y el corte por cantidad van en la consulta del repositorio (`IRepositorioUsuarios.ListarRanking`):
  la base devuelve solo los primeros N. Ordenar en el servicio obligaría a traer a todos los jugadores.
- `cantidad` fuera de rango se ajusta igual que la paginación (D-35): mayor a 100 usa 100 y menor a 1 usa
  10.
- `batallasGanadas` y `batallasPerdidas` valen 0 hasta que se puedan fallar batallas: hoy la batalla
  todavía no guarda el ganador. El contrato sale completo desde ahora, como `InscripcionesAceptadas` en
  torneos (D-36).

**Justificación:**
- Desempate por id y no por nickname: el id es único y no cambia, y no depende de cómo compara texto
  cada base. SQL Server, con la intercalación por defecto, no distingue mayúsculas y SQLite sí, así que
  ordenar por nickname podría dar otro orden en los tests. Las batallas ganadas todavía no existen.
- El desempate tiene que estar escrito en la consulta: sin `ThenBy`, el orden de los empates depende del
  plan de ejecución de la base.
- Para que el test del desempate falle sin `ThenBy`, crea un índice por ELO en la base de prueba: SQLite
  lo recorre al revés y devuelve los empates del id mayor al menor, como puede pasar en SQL Server. Sin
  el índice, SQLite devuelve los empates en el orden en que se guardaron y el test pasaba por casualidad.

**Conceptos del curso:** REST (query para filtros, recurso público), status codes, determinismo, tests
de repositorio con SQLite, TDD (un RED que falle por la razón correcta), FIRST (Repeatable).

**Evidencia:**
- `RankingController`, `ServicioRanking` e `IRepositorioUsuarios.ListarRanking` / `RepositorioUsuarios`.
- `RankingControllerTests`, `ServicioRankingTests` y `RepositorioRankingTests` (solo jugadores, desempate
  con el índice, baja lógica y cantidad).
- La carpeta Ranking de Postman: cantidad por defecto, 1, mayor al tope, menor a 1, que no aparezcan
  organizadores, administradores ni usuarios dados de baja, y `cantidad=abc` → 400. Se corrió la
  colección completa contra SQL Server sin fallos.

**Pendientes:**
- Calcular `batallasGanadas` y `batallasPerdidas` cuando se puedan fallar batallas. En ese momento hay
  que actualizar `ServicioRankingTests.Obtener_CantidadValida_DevuelveLosJugadoresConSuPosicion`, que hoy
  afirma que valen 0, y el test de Postman que comprueba lo mismo.
- ~~El filtro `torneoId`, con su 404: definir qué jugadores del torneo entran.~~ Resuelto en D-45.
- Agregar `ServicioRanking` al diagrama de clases. La especificación de esta parte está en
  `docs/api/ranking.md`.

**Participación de IA:**
- Claude Code propuso el contrato, el desempate por id frente al nickname (por la intercalación) y los
  campos en 0; el integrante eligió las opciones recomendadas y aprobó el resto de las etapas en bloque.
- Problema que apareció: el primer intento de RED del desempate pasaba sin `ThenBy`. Claude Code lo
  detectó antes de commitear; `PRAGMA reverse_unordered_selects` no sirvió porque no aplica a consultas
  con `ORDER BY`, y lo resolvió con el índice por ELO en el test.
- Para correr la carpeta en la colección completa se cambió el salto (`setNextRequest`) del último
  request de Inscripciones, que ahora pasa por Ranking antes del logout.

---

### D-62 — Rondas: emparejamiento como Strategy, ronda derivada de las batallas y guardado atómico

**Problema:** la letra pide generar las rondas del torneo eliminatorio. La primera exige torneo Abierto y tantas
inscripciones Aceptadas como el cupo, y pasa el torneo a En curso. Las siguientes exigen la ronda anterior
resuelta y emparejan a los jugadores que no fueron eliminados. El emparejamiento tiene que ser determinístico y
documentado, con un desempate explícito (nunca el orden accidental de la base). Hay que decidir si la ronda es una
entidad, dónde vive el emparejamiento, cómo se cruzan los jugadores y cómo se guarda todo junto.

**Alternativas consideradas:**
- Ronda: una entidad con tabla propia (`Id`, `TorneoId`, `Numero`) o derivarla de `Batalla.NumeroRonda`, que ya
  está en el contrato de Batalla.
- Emparejamiento: un método privado del servicio o una Strategy (`IEstrategiaEmparejamiento`), que el diseño del
  equipo marca como punto de variación.
- Cruces: mejor contra peor (1.º contra último, 2.º contra penúltimo) o vecinos de ELO (1.º contra 2.º).
- Guardado: un `SaveChanges` por batalla o uno solo para las batallas y el torneo.

**Decisión adoptada:**
- **La ronda es derivada:** es el grupo de batallas del torneo con el mismo número. No hay tabla ni migración; la
  ronda siguiente es la última más uno, y las consultas agrupan las batallas por número.
- **Strategy:** `IEstrategiaEmparejamiento.Emparejar(jugadores)` devuelve `ParDeJugadores`. `EmparejamientoPorElo`
  (en `FarmeORT.Dominio/Rondas/`, registrada Scoped) ordena en memoria por ELO de mayor a menor y desempata por id,
  así el resultado no depende del orden en que llegan los jugadores. Cruza mejor contra peor: es el cruce
  clásico de los cuadros eliminatorios, en el que los favoritos se encuentran al final. Lo eligió el integrante.
- **`Torneo.Iniciar(aceptadas)`**, al final de los métodos públicos de `Torneo`: si no está Abierto, 409
  `TOURNAMENT_NOT_OPEN` (se reusa `TorneoNoAbiertoException`); si las aceptadas no igualan el cupo, 409
  `TOURNAMENT_NOT_FULL` (`TorneoIncompletoException`); si no, pasa a En curso.
- **Servicio (`ServicioRondas.Generar`):**
  1. Valida el torneo y quién llama (D-63).
  2. Trae a los jugadores con inscripción Aceptada.
  3. Prepara la ronda. Si el torneo no está En curso, es la primera: llama a `Iniciar`. Si está En curso, es una
     siguiente: la última ronda tiene que estar resuelta (todas sus batallas Finalizadas o Canceladas, si no 409
     `ROUND_NOT_RESOLVED`) y tienen que quedar al menos dos jugadores. Con uno solo ya hay campeón y responde 409
     `TOURNAMENT_READY_TO_FINISH`, porque corresponde finalizar el torneo.
  4. Empareja y crea una batalla por par con el constructor de Batalla y el `ISorteo` inyectado (empieza el de
     mayor ELO; si empatan, el sorteo).
  5. Agrega cada batalla con `IRepositorioBatallas.Agregar`, que no guarda, y guarda todo una sola vez con
     `IRepositorioTorneos.Actualizar`: las batallas y el cambio de estado del torneo quedan juntos o no queda nada
     (como D-42).
- **Consultas:** `GET /torneos/{id}/rondas` y `GET /torneos/{id}/rondas/{numero}`, con autenticación opcional. El
  anónimo no ve las rondas de un torneo Cancelado (D-36). Un número que el torneo no tiene responde 404
  `ROUND_NOT_FOUND`. `IRepositorioBatallas.ListarDelTorneo` trae las batallas con sus jugadores, ordenadas por
  ronda y por id.
- **Respuesta:** `RondaResponse` con el número y sus batallas en el mismo `BatallaResponse` que `GET /batallas`
  (se reusa `RespuestasBatallas.Resumen` de la persona B, en lugar de un DTO propio que repetía los mismos campos).
  `ganadorId` vale `null` hasta que la batalla se falle.
- **Cuadro:** el detalle del torneo trae `rondas` con todas las rondas y sus batallas, así una sola llamada alcanza
  para dibujar el cuadro (letra). El armado de las rondas lo comparten `ServicioRondas` y `ServicioTorneos` en
  `RespuestasRondas`.
- Un torneo En curso sin batallas (cargado a mano) arranca por la ronda 1 en lugar de fallar.
- Las consultas nuevas van al final de `IRepositorioBatallas`, la interfaz de la persona B, como estaba acordado.

**Justificación:**
- La ronda derivada evita duplicar un dato que Batalla ya tiene: con una tabla aparte, el número de ronda podría
  quedar distinto en los dos lugares.
- Strategy (OCP, Protected Variations, Polymorphism): otra forma de emparejar (por ejemplo, vecinos de ELO o por
  sorteo) es una clase nueva y un cambio en el registro, sin tocar el servicio. El servicio depende de la
  abstracción (DIP) y en sus tests se reemplaza por un mock.
- `Torneo` controla su propio paso a En curso (Information Expert), como ya controlaba su cancelación.
- El orden de las consultas está escrito en la consulta. Para que el test falle si falta el desempate, crea un
  índice de prueba con el id descendente: sin un orden explícito, SQLite devuelve las batallas de una misma ronda
  del id mayor al menor (la misma técnica de D-61).

**Conceptos del curso:** Strategy, OCP, DIP, Protected Variations, Polymorphism e Information Expert (GRASP),
atomicidad con un solo `SaveChanges`, REST (sub-recurso `/rondas`), status 404 y 409, TDD Outside-In.

**Evidencia:**
- Dominio: `IEstrategiaEmparejamiento`, `EmparejamientoPorElo`, `ParDeJugadores`, `Torneo.Iniciar`;
  `EmparejamientoPorEloTests` (dos jugadores, ELO igual con desempate por id, ELO distinto) y
  `TorneoRondasTests`.
- `ServicioRondas` y `ServicioRondasTests`: primera ronda, estado En curso con un solo guardado, torneo
  inexistente, organizador no asignado, administrador, torneo incompleto, rondas siguientes, ronda sin resolver,
  un solo jugador, batalla cancelada como resuelta, consultas, anónimo y ganador.
- `RondasController` y `RondasControllerTests` (201 con `Location`, 200 y la seguridad por reflexión).
- `RepositorioBatallas.Agregar` y `ListarDelTorneo`, y `RepositorioRondasTests`, que incluye que actualizar el
  torneo guarda junto con él las batallas de la ronda.
- `FiltroExcepcionesRondasTests` y `docs/api/rondas.md`.

**Supuestos y pendientes:**
- Dos pedidos simultáneos de generar la misma ronda podrían crear batallas repetidas: `Torneo` no tiene token de
  concurrencia. Se acepta el riesgo, porque la ronda la genera una sola persona (el organizador asignado o un
  administrador).
- `TOURNAMENT_NOT_OPEN` reusa el mensaje de las inscripciones ("El torneo no está abierto a inscripciones."), que
  en las rondas se lee algo forzado. Cambiarlo toca el mapeo de la persona A.
- La secuencia de generar ronda y las clases que participan están en `docs/uml/generar-ronda.puml` y
  `docs/uml/rondas-clases.puml`; falta sumarlas al diagrama de clases general del informe.

**Participación de IA:**
- Claude Code propuso el contrato, la ronda derivada, mejor contra peor y los códigos de error nuevos; el
  integrante eligió la ronda derivada y mejor contra peor, y aprobó el resto de las etapas en bloque.
- Problemas que aparecieron:
  - `Usuario.ActualizarElo` (persona B) no estaba en develop, así que no se podían armar jugadores con distinto ELO
    en los tests. Primero se hicieron los ciclos que no lo necesitaban (dos jugadores y desempate por id) y el de
    "mayor ELO contra menor ELO" se hizo después de traer los fallos.
  - El test del orden de `ListarDelTorneo` pasaba por el orden accidental de SQLite. Claude Code lo detectó
    probando la consulta sin desempate antes del GREEN y agregó el índice de prueba.
  - `dotnet format` convierte las lambdas de los `Assert.ThrowsExactly` en method groups; un script de edición dejó
    de coincidir con el archivo y hubo que rehacerlo sobre el archivo real.

---

### D-63 — Generan rondas el organizador asignado o un administrador (supuesto)

**Problema:** la letra asigna al Organizador generar las rondas de sus torneos, pero no dice nada del
Administrador. Si el organizador asignado no está disponible, nadie podría avanzar el torneo.

**Alternativas consideradas:** solo el organizador asignado, como las skills obligatorias (D-40); cualquier
Organizador; el organizador asignado o un Administrador.

**Decisión adoptada:** el organizador asignado o un Administrador (lo eligió el integrante; la IA recomendaba solo
el organizador asignado, que es lo que dice la letra).
- El filtro de autorización admite los roles Organizador y Administrador; otro rol recibe 403 `FORBIDDEN`.
- El servicio decide si el Organizador es el asignado a ese torneo; si no, 403 `NOT_TOURNAMENT_ORGANIZER` (se
  reusa `NoEsOrganizadorDelTorneoException`, D-40). Un Administrador pasa sin esa comprobación.
- Es un supuesto: si una aclaración docente dice que solo el organizador, alcanza con sacar el rol del atributo
  y la condición del servicio. Se aplica igual a finalizar el torneo.

**Justificación:** mantiene la regla de D-40 para los organizadores y le da al Administrador, que ya crea y cancela
torneos, la posibilidad de destrabar uno.

**Conceptos del curso:** autorización por rol en el filtro y regla de negocio en el servicio (SRP), status 403.

**Evidencia:** `RondasController.Generar` con `[Autorizacion(Roles.Organizador, Roles.Administrador)]`,
`ServicioRondas.ObtenerTorneoDelOrganizador` y los tests `Generar_OrganizadorNoAsignado_...` y
`Generar_AdministradorQueNoEsElOrganizador_GeneraLaRonda`.

**Participación de IA:** Claude Code planteó las tres opciones citando la letra y recomendó solo el organizador
asignado; el integrante eligió sumar al Administrador.

---

### D-64 — Finalizar el torneo: precondiciones en el servicio, campeón 0..1 y un solo guardado

**Problema:** la letra pide finalizar el torneo cuando está En curso, no tiene batallas sin resolver y queda un
solo jugador sin eliminar, que pasa a ser el campeón. Sin esta operación ningún torneo llegaba a Finalizado: el
campeón seguía con su inscripción vigente para siempre, lo que le bloqueaba quitar skills obligatorias del
repertorio y bloqueaba la baja del jugador y del organizador (D-43). Hay que decidir el contrato, dónde viven las
reglas y cómo se guarda el campeón.

**Alternativas consideradas:**
- Contrato: `POST /torneos/{id}/finalizaciones` con 200 y la representación, como la cancelación, o 201 con
  `Location`, como los fallos. No hay un recurso "finalización" que consultar después.
- Reglas: todas en `Torneo.Finalizar(jugadoresSinEliminar, batallasSinResolver)`, o el estado en el dominio y las
  que dependen de otras entidades en el servicio, como en las rondas (D-62).
- Campeón: una propiedad del torneo (`CampeonId` + `Campeon`, 0..1) o deducirlo de la final.

**Decisión adoptada:**
- **Contrato** (`docs/api/finalizacion.md`): 200 con `FinalizacionResponse`, que tiene `torneoId`, `estado` y
  `campeon`, este último en el mismo `JugadorBatallaResponse` de las batallas. Controller propio
  (`FinalizacionesController`), con la misma autorización que las rondas (D-63).
- **Reglas, en este orden:**
  1. El torneo existe (404).
  2. Quien llama es el organizador asignado o un Administrador (403).
  3. No hay batallas sin resolver: 409 `TOURNAMENT_HAS_UNRESOLVED_BATTLES`.
  4. Queda exactamente un jugador con inscripción Aceptada: 409 `TOURNAMENT_WITHOUT_CHAMPION`.
  5. El torneo está En curso: 409 `TOURNAMENT_NOT_IN_PROGRESS`.
  - El estado lo controla `Torneo.Finalizar(campeon)` (Information Expert). Las batallas y las inscripciones las
    controla el servicio con consultas que ya existían (`ListarSinResolverDelTorneo` y
    `ListarAceptadasDelTorneo`), sin agregar métodos a los repositorios de otros.
- **Campeón:** `Torneo.CampeonId` y `Campeon` (0..1), con FK a `Usuarios` y `Restrict`: un usuario con historial no
  se borra, se da de baja lógica (D-27). Se configura en un método propio del contexto
  (`ConfigurarFinalizacion`) y se agrega con la migración de este PR.
- **Guardado:** un solo `SaveChanges` con `IRepositorioTorneos.Actualizar`.
- La inscripción del campeón no cambia: con el torneo Finalizado deja de estar vigente, así que ya no le exige las
  obligatorias ni le bloquea la baja.
- Se extrajo `PermisosDelTorneo.ValidarOrganizadorOAdministrador`, que comparten las rondas y la finalización.

**Justificación:**
- La misma forma que la cancelación hace que las dos operaciones sobre el estado del torneo se usen igual desde el
  cliente.
- Guardar el campeón deja explícita la relación Torneo → Campeón 0..1 del diagrama de clases. Además, el torneo
  responde quién ganó sin recorrer las batallas.
- Reusar las consultas existentes evita tocar interfaces de otras personas.

**Conceptos del curso:** Information Expert, REST (sub-recurso sustantivado), status 403, 404 y 409, persistencia
con FK opcional y `Restrict`, TDD Outside-In.

**Evidencia:**
- `FinalizacionesController` y sus tests (200 y la seguridad por reflexión).
- `ServicioFinalizaciones` y sus tests: campeón, estado y un solo guardado, torneo inexistente, organizador no
  asignado, administrador, batallas sin resolver y cantidad de jugadores distinta de uno.
- `Torneo.Finalizar` y `TorneoFinalizacionTests`, que incluyen el pendiente de que un torneo Finalizado no se
  cancela.
- `InscripcionFinalizacionTests`: `ExigeSkill` con el torneo En curso y Finalizado (pendiente de D-41).
- `RepositorioTorneosFinalizacionTests`: el campeón se guarda y la FK es `Restrict`.
- `FiltroExcepcionesFinalizacionTests`, `docs/uml/finalizar-torneo.puml` y `finalizacion-clases.puml`, y la carpeta
  Finalización de Postman: 401, 403, 404, cada 409, finalizar dos veces, no cancelar ni generar rondas en un
  torneo Finalizado.

**Supuestos:**
- Como en las rondas, también puede finalizar un Administrador (D-63).
- `TOURNAMENT_NOT_IN_PROGRESS` es un código nuevo: el torneo Abierto, Finalizado o Cancelado no se puede
  finalizar.

**Participación de IA:**
- Claude Code propuso el contrato y los códigos de error. Recomendó separar el servicio de la finalización del de
  torneos de la persona A y reusar sus consultas; el integrante aprobó las etapas en bloque.
- Problemas que aparecieron:
  - Un archivo de tests generado desde la consola quedó en Windows-1252 en vez de UTF-8. Compilaba igual, pero se
    corrigió en un commit aparte.
  - Por convención, EF mapeaba el campeón con `ClientSetNull`. El test del repositorio lo detectó y la relación se
    configuró explícitamente con `Restrict`.

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
- ~~Cuando más controllers lean la sesión del contexto, extraer esa lectura a un solo lugar (por ejemplo,
  un método de extensión `HttpContext.ObtenerSesionAutenticada()`), para no repetir el cast.~~
  Resuelto en D-26.
- Un id que no es número (`/usuarios/abc`) responde 404 **sin cuerpo**, porque lo resuelve el
  enrutamiento antes de los filtros. Igual con los errores de conversión de la query
  (`limit=abc`), cuyo detalle sale en inglés ("The value 'abc' is not valid for Limit.").
- ~~Repertorio (D-38): quitar una skill tiene que rechazarse (409) si es obligatoria de un torneo donde el
  jugador tiene una inscripción Aceptada y el torneo no está Finalizado ni Cancelado.~~ Resuelto en D-41.
  Pendiente: diagrama de clases con `SkillDeclarada` y la colección
  `Repertorio` de `Usuario`.
- Torneos (D-36, D-37), lo que espera a otras funcionalidades:
  - ~~Cancelar un torneo tiene que cancelar sus batallas no finalizadas y sus inscripciones Aceptadas en la
    misma operación (atómica). Hoy solo cambia el estado.~~ Resuelto en D-42.
  - ~~Aplicar el filtro `skillObligatoriaId` en `RepositorioTorneos.Listar`.~~ Resuelto en D-40.
  - ~~Calcular `InscripcionesAceptadas` (hoy vale 0).~~ Resuelto en D-41.
  - ~~Sumar al detalle las skills obligatorias y los jugadores aceptados.~~ Resuelto en D-44.
    ~~Falta el cuadro (rondas y batallas), que agrega la Persona C.~~ Resuelto en D-62.
  - ~~Skills obligatorias (D-40): rechazar asociar una skill si hay inscripciones Aceptadas de jugadores
    que no la tienen declarada, identificándolos (409 `PLAYERS_MISSING_MANDATORY_SKILL`).~~ Resuelto en D-41.
  - ~~Agregar el test de que no se cancela un torneo Finalizado cuando exista finalizar torneo. La regla ya
    está en `Torneo.Cancelar`, pero esa rama todavía no se puede cubrir. Lo mismo con
    `Inscripcion.ExigeSkill` para un torneo Finalizado (D-41).~~ Resuelto en D-64.
  - ~~Postman: carpeta Torneos con los casos de alta, modificación, cancelación y consulta (anónimo y
    autenticado).~~ Resuelto en el PR de torneos. Diagrama de clases con `Torneo`, `EstadoTorneo`, `IRepositorioTorneos`,
    `FiltroTorneos` y las excepciones nuevas.
- ~~Restricciones de eliminación de usuarios que dependen de entidades que todavía no existen: no se puede
  eliminar si tiene batallas sin resolver o si organiza un torneo que no está Finalizado ni Cancelado.
  `DELETE /usuarios/{id}` ya existe (D-27): al implementar batallas y torneos hay que agregar estas
  precondiciones a `ServicioUsuarios.Eliminar` (409) con sus ciclos TDD.~~ Resuelto en D-43.
- ~~Cambiar el rol de un usuario con datos asociados (por ejemplo, un Organizador con torneos activos
  que pasa a Jugador) no tiene restricciones porque esas entidades todavía no existen. Revisarlo junto
  con torneos e inscripciones.~~ Resuelto en D-43.
- ~~Si dos altas con el mismo email o nickname llegan al mismo tiempo, la segunda terminaba en 500.~~
  Resuelto en D-23: conflicto detectado al guardar → 409; se desasocia el alta fallida y otros errores
  de base se propagan al tratamiento seguro de 500.
- Registrar en AGENTS.md que Dominio referencia a Excepciones (D-16).
- Actualizar el diagrama de clases con el catálogo de skills: `Skill`, `CategoriaSkill`,
  `IRepositorioSkills` y `RepositorioSkills` con los nombres del código (`ObtenerPorId`, `Listar`,
  `ExistePorCodigo`), `FiltroSkills` con `Nombre`, `Limite` y `Desplazamiento`, las excepciones por caso
  (D-07, D-30) y el nombre real del contexto (`FarmeORTContext`).
- Repertorio, skills obligatorias e inscripción tienen que aplicar la regla de skill inactiva (letra
  2.2): no puede declararse en un repertorio ni asociarse como obligatoria, sin efecto retroactivo.
- Notificaciones: la letra oficial 2026.2 no las incluye (el borrador leído en clase sí); `AGENTS.md` §8 ya
  lo refleja. Se postergan: confirmarlo en el foro antes de que cierre.
- Documentar en el informe de diseño qué tipos forman la interfaz de cada assembly (letra,
  "Independencia de librerías").
- La colección de Postman trae `baseUrl` con el puerto 5088, pero `launchSettings.json` levanta la API
  en el 5294: unificarlo o explicarlo en la guía de instalación.
- Completar la participación de IA de D-01 a D-28 con qué se aceptó, qué se rechazó o modificó y qué
  problemas aparecieron (lo pide la letra; no alcanza con "propuesta de la IA, aprobada"). Lo tiene que
  escribir quien trabajó en cada una.
- Pasar a supuestos las entradas menores (D-08, D-19, D-32): la bitácora es para decisiones con impacto.
- Justificar en la especificación por qué `POST /jugadores` devuelve `Location: /usuarios/{id}` (D-22):
  en clase remarcaron no representar lo mismo de dos maneras.
- Los commits RED de las validaciones de DTOs del catálogo de skills (`0162a1f`, `eac0efa`) agrupan varios
  tests; son anteriores a la regla de un test por RED y hay que explicarlo en el documento de TDD.
