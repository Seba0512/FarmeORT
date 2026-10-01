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

**Participación de IA:** Claude Code planteó las alternativas y recomendó la lista fija; el equipo la
eligió. Pendiente: confirmar los valores contra el diagrama de clases de Dominio.

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

**Justificación:** el código implementa el contrato aprobado, y mantener las convenciones existentes
hace que skills se lea igual que usuarios sin refactorizar lo que ya está hecho. El equipo eligió
seguir el código en lugar del diagrama, que queda por actualizar.

**Conceptos del curso:** REST (`PATCH` para un cambio parcial, `PUT` como reemplazo completo,
autenticación y autorización separadas), Controller de GRASP, consistencia entre código y UML.

**Evidencia:** `SkillsController`, `ServicioSkills`, `Skill`, `RepositorioSkills`, `FiltroExcepciones`,
los tests de cada capa y la carpeta Skills de la colección de Postman.

**Participación de IA:** Claude Code detectó las diferencias entre el diagrama y la especificación y
propuso seguir el código; el equipo eligió esa opción. Claude Code implementó la funcionalidad con
ciclos RED/GREEN/REFACTOR por commit, siguiendo `AGENTS.md` y las skills `da2-*`; el equipo revisa y
aprueba en el PR.

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

**Participación de IA:** propuesta de Claude Code a partir de D-21, D-23 y D-28.

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

**Participación de IA:** propuesta de Claude Code.

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
- Restricciones de eliminación de usuarios que dependen de entidades que todavía no existen: no se puede
  eliminar si tiene batallas sin resolver o si organiza un torneo que no está Finalizado ni Cancelado.
  `DELETE /usuarios/{id}` ya existe (D-27): al implementar batallas y torneos hay que agregar estas
  precondiciones a `ServicioUsuarios.Eliminar` (409) con sus ciclos TDD.
- Cambiar el rol de un usuario con datos asociados (por ejemplo, un Organizador con torneos activos
  que pasa a Jugador) no tiene restricciones porque esas entidades todavía no existen. Revisarlo junto
  con torneos e inscripciones.
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
- Notificaciones: la letra 2026.2 y la especificación de la API no las piden, pero `AGENTS.md` (§8) y la
  skill de dominio sí. Se postergan (decisión del 2026-10-01); si se descartan, corregir `AGENTS.md` por PR.
- Documentar en el informe de diseño qué tipos forman la interfaz de cada assembly (letra,
  "Independencia de librerías").
- La colección de Postman trae `baseUrl` con el puerto 5088, pero `launchSettings.json` levanta la API
  en el 5294: unificarlo o explicarlo en la guía de instalación.
