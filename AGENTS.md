# AGENTS.md — FarmeORT (Diseño de Aplicaciones 2)

Instrucciones para agentes de IA (Claude, Copilot, Codex, Cursor, etc.) que trabajen en este
repositorio. Es el backend **FarmeORT**: ASP.NET Web API (.NET 10) + EF Core 10 Code First sobre
SQL Server Express, obligatorio del curso Diseño de Aplicaciones 2.

El proceso (commits, PRs, reviews, Actions, documentación) **es evidencia evaluada**, no solo el
código final. Ante la duda, frenar y preguntar al integrante antes que decidir en silencio.

---

## 0. Jerarquía de fuentes

Letra oficial 2026.2 > aclaraciones docentes (clases, sus transcripciones y el foro) >
**configuración obligatoria del repo** (rama
[`repo-configuration`](https://github.com/IngSoft-DA2/DA2-Tecnologia/tree/repo-configuration) de
DA2-Tecnologia) > diseño aprobado (UML) > especificación de la API > bitácora de decisiones >
tests existentes > código existente > este archivo > skills `da2-*` > conocimiento general.

Si algo de este archivo contradice una fuente superior, gana la superior: señalar la contradicción.

Lo que dicen los docentes en clase manda sobre este archivo, las skills, la bitácora y el código, con
tres cuidados:
- Algunas clases leen un **borrador** de la letra (por ejemplo, el que tenía notificaciones o un
  comentario de fallo de 10 a 15 caracteres). Si difiere de la letra oficial, manda la letra oficial.
- Distinguir lo que el docente **exige** de lo que **sugiere** u opina, y no tomar como regla un lapsus
  evidente (por ejemplo, 403 para un token que falta: se usa 401).
- El CI obligatorio se cumple igual aunque una clase pida menos (por ejemplo, cobertura de 80 %: el
  check `Test` exige 90 %).

## 1. Reglas que nunca se rompen

1. **No modificar la configuración obligatoria**: `.github/workflows/build-test.yml`,
   `.github/workflows/code-analysis.yml`, `.editorconfig`, `.gitattributes`, `.gitignore`,
   `.runsettings`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`,
   `.config/dotnet-tools.json`, `pull_request_template.md`. (Única excepción: el valor de
   `solution-name` en los workflows.) No crear workflows propios.
2. **Nunca escribir implementación sin un test RED que la pida** (TDD, sección 5).
3. **Nunca** `[ExcludeFromCodeCoverage]`, `#pragma warning disable`, `SuppressMessage` ni cambios de
   severidad para esquivar reglas, cobertura o tests. Nunca desactivar ni borrar un test que falla.
4. **Commits sin co-autoría ni firmas de IA**: prohibido `Co-Authored-By`, `Generated with ...`,
   `Signed-off-by`, emojis de bot o `--author` ajeno. El uso de IA se declara en la documentación.
5. **Nunca trabajar sobre `main` ni `develop`**; nunca mergear localmente; nunca pushear a esas ramas.
6. **No introducir tecnologías ni patrones no aprobados** (MediatR, CQRS, AutoMapper,
   FluentValidation, ASP.NET Identity, JWT, Unit of Work/repositorio genérico, Specification, Domain
   Events, Clean Architecture…) sin propuesta, aprobación del equipo y registro en la bitácora.
7. **No reemplazar decisiones de arquitectura en silencio**: si hay una mejora, proponer problema →
   alternativa → ventajas/desventajas → impacto (código, tests, API, UML, docs) y esperar aprobación.
8. **No inventar endpoints ni rutas**: el contrato se decide antes del código (sección 7).

## 2. Comandos

```bash
dotnet tool restore                      # dotnet-ef 10.0.11 (herramienta local), una vez tras clonar
dotnet restore <Solucion>.slnx
dotnet build <Solucion>.slnx --configuration Release
dotnet test <Solucion>.slnx --settings .runsettings --collect:"XPlat Code Coverage"
dotnet format <Solucion>.slnx             # aplicar estilo; el CI usa --verify-no-changes

# Migraciones (parado en el proyecto de acceso a datos)
dotnet ef migrations add <NombreDescriptivo> --startup-project ../<WebAPI>
dotnet ef database update --startup-project ../<WebAPI>
```

El SDK lo fija `global.json` (10.0.100, `latestFeature`). No cambiarlo ni instalar otra versión de
`dotnet-ef`. **Antes de cada push: `dotnet build`, `dotnet test` y `dotnet format` en local.**

## 3. CI obligatorio (GitHub Actions)

Dos workflows de la guía, que llaman a los reutilizables de `IngSoft-DA2/workflows@self-hosted` y
corren en los **self-hosted runners** de los integrantes. Generan tres checks, **los tres
obligatorios para mergear** a `main` y `develop`:

| Check | Qué valida | Falla si… |
|---|---|---|
| `Build` | `dotnet restore` + `dotnet build -c Release` | no compila (varias reglas de estilo están en `error`) |
| `Test` | tests + cobertura con `coverlet.collector` y `.runsettings` | un test falla; falta `coverlet.collector` en un proyecto de test; aparece `ExcludeFromCodeCoverage`; el reporte no tiene paquetes; **algún assembly tiene < 90 % de line coverage o < 90 % de branch coverage** |
| `Analysis` | `dotnet format --verify-no-changes` (`.editorconfig` + StyleCop) | hay cualquier cambio de formato/estilo/analizador pendiente |

- Exclusiones de cobertura: **solo** las del `.runsettings` (`*Migrations/*.cs`, `Program.cs`,
  `*DbContext.cs`, `*Context.cs`, código generado). Se explican en el documento de TDD.
- Cobertura con `coverlet.collector` (lo agrega `Directory.Build.targets`); si el check no lo
  encuentra, agregar el `PackageReference` explícito en el `.csproj` de test. Nunca `coverlet.msbuild`.
- Si una Action falla: leer el log del check, reproducir en local con el mismo comando y corregir la
  causa en el código. Cobertura baja → tests significativos sobre líneas y ramas sin cubrir, no
  tests triviales.

## 4. Estilo de código obligatorio (`.editorconfig` + StyleCop)

Todo código nuevo, **incluidos los tests y las firmas mínimas del commit RED**, debe cumplirlo:

- **Primary constructors** para inyectar dependencias (IDE0290 = error); usar el parámetro directo.
- **Namespaces file-scoped** (`namespace X;`) que coincidan con la carpeta.
- `using` fuera del namespace, `System` primero, sin usings sin usar.
- **`var`** cuando el tipo es predefinido o evidente.
- **Llaves siempre** en `if/for/foreach/while`, estilo Allman, línea en blanco después de `}`.
- Sin `this.`; campos `private` (y `readonly` cuando corresponda); modificador de acceso explícito.
- Sin variables, parámetros, asignaciones ni miembros privados sin usar; sin código inalcanzable;
  `async` solo con `await`.
- Collection expressions / inicializadores (`List<Skill> skills = [];`).
- Nullable: no anulables inicializados (CS8618 = error); `= null!` solo en campos de test asignados
  en `[TestInitialize]`.
- PascalCase para tipos y miembros, interfaces con `I`, parámetros en camelCase, `string.Empty`.
- `Program.cs` con top-level statements.
- Clases `internal` no heredadas → `sealed` (CA1852).
- `.cs` con CRLF y 4 espacios (lo fijan `.gitattributes` y `.editorconfig`).

```csharp
namespace FarmeORT.Servicios.Inscripciones;

public sealed class ServicioInscripciones(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioInscripciones repositorioInscripciones) : IServicioInscripciones
{
    public Inscripcion Inscribir(int jugadorId, int torneoId)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        ...
    }
}
```

## 5. TDD Outside-In

**RED → GREEN → REFACTOR, un commit por etapa.** Si piden "implementá X", el primer entregable es el
test RED de X. Si piden todo junto, explicar que rompe la evidencia y ofrecer ciclos.

- **RED**: un test de comportamiento no implementado que falla por la razón esperada (assertion o
  excepción de dominio), no por configuración. Si hacen falta tipos/métodos nuevos, solo la firma
  mínima (lanzando `NotImplementedException`) en el mismo commit, cumpliendo el estilo.
- **GREEN**: lo mínimo para pasar; la suite relevante queda verde.
- **REFACTOR**: mejorar estructura sin cambiar comportamiento ni el significado de los tests. Si no
  hay nada que mejorar, no inventarlo.
- **Bugs**: test que lo reproduce (RED) → corrección mínima (GREEN) → refactor si corresponde.

Orden Outside-In: `API → Controller → IServicio → Servicio → Dominio → IRepositorio → Repositorio`.

1. Controller con mock del servicio (status, `Location`, que se llamó con el usuario autenticado).
2. Servicio con stubs/mocks de repositorios: **un test por precondición y por efecto**.
3. Dominio: reglas que viven en las entidades.
4. Repositorio con **EF Core SQLite en memoria** (fake) solo si la consulta tiene lógica real; no
   mockear `DbSet`; encapsular el contexto de test en una clase reutilizable.

Moq: `new Mock<T>(MockBehavior.Strict)`, setups con **valores exactos** (no `It.IsAny` salvo en
`Verify(..., Times.Never)`), `mock.Object` al constructor, setup común en `[TestInitialize]`. No
testear que el mock devuelve lo configurado. Tests FIRST. En MSTest usar `Assert.ThrowsExactly<T>`
(`Assert.ThrowsException` está obsoleto/eliminado). Nombres: `Metodo_Escenario_Resultado`.

Funcionalidades con asterisco (TDD con evidencia por commit obligatorio): **inscripción a torneo,
lanzar skill, fallar batalla, catálogo de skills, generar rondas / finalizar torneo**.

Lo que piden los docentes en clase:
- En las funcionalidades con asterisco, **un test por commit RED**: se tiene que ver la evolución test
  por test. Las variantes de datos de un mismo caso van con `DataRow` en el mismo test.
- Tests **independientes** (la I de FIRST): para preparar o verificar no se usa otro método del objeto
  bajo prueba (en un test de `Agregar` no se llama a `ObtenerPorId` del mismo repositorio); se usa el
  contexto o datos armados a mano.
- Arrange, Act y Assert separados por una línea en blanco; el Act en una sola línea.

## 6. Arquitectura

Cada paquete lógico es un **assembly independiente** con prefijo `FarmeORT.`: Dominio, IServicios,
Servicios, DTOs, IRepositorios, Repositorios, WebAPI, Excepciones (solución `ObligatorioDA2.slnx`).
Los tests van en **un proyecto por assembly con lógica** (`FarmeORT.Dominio.Tests`,
`FarmeORT.Servicios.Tests`, `FarmeORT.Repositorios.Tests`, `FarmeORT.WebAPI.Tests`), cada uno
referenciando solo el assembly que prueba. Antes de crear algo, **buscar si ya existe** una clase,
interfaz, DTO, excepción, mapper, filter o helper equivalente.

```text
WebAPI       → DTOs, IServicios, Fabrica
Fabrica      → Servicios, Repositorios
Servicios    → IServicios, Dominio, IRepositorios
Repositorios → IRepositorios, Dominio
```

La WebAPI **no referencia** Servicios ni Repositorios: la inyección de dependencias se resuelve en
`FarmeORT.Fabrica`, que llama al registro de cada assembly. En clase bajaron un diagrama de componentes
de 8 a 4 porque la WebAPI conocía la lógica y el acceso a datos.

- **Dominio**: entidades y reglas propias (Information Expert). No conoce WebAPI, DTOs, EF ni ASP.NET.
- **Controller = recepcionista**: mapea DTO, llama al servicio, devuelve status. Sin reglas, sin
  DbContext/LINQ, sin try/catch (lo resuelve el exception filter global).
- **Servicios**: casos de uso; dependen de abstracciones; no hacen `new` de repositorios, contexto
  ni puntos de variación.
- **Repositorios**: persisten y consultan; **no deciden reglas**.
- **DTOs**: sufijos `*Request`/`*Response`; nunca exponer entidades de dominio.
- **Excepciones propias** solo si están justificadas (dan un `errorCode` propio o distinguen el origen
  del error); no crear excepciones por crear.
- `new` de lo propio y estable del dominio; inyectar infraestructura y lo que puede variar.
- DI: DbContext, repositorios y servicios **Scoped**. Nunca DbContext Singleton ni Singleton que
  reciba un Scoped. Program.cs delega el registro de DI, filtros y base en métodos/helpers.
- Por defecto `internal` (+ `sealed`); abrir solo lo necesario y documentar qué es público. Para tests
  sobre tipos internos: `InternalsVisibleTo` (+ `DynamicProxyGenAssembly2` para Moq).
- Puntos de variación a mantener: `ICalculadorElo ← CalculadorEloBase`,
  `IEstrategiaEmparejamiento ← EmparejamientoPorElo`, y el mecanismo extensible de notificaciones
  (postergado: ver §8).
  Un patrón se aplica solo si resuelve un problema concreto, y se registra en la bitácora.

### Persistencia (EF Core 10 Code First)

- El contexto vive en Repositorios; nunca en Controller ni Dominio.
- Relaciones configuradas explícitamente en `OnModelCreating`: FKs, índices, largos máximos de string.
- Navegación **y** FK (`TorneoId`) en las relaciones; entidad intermedia explícita en muchos-a-muchos
  con datos propios.
- Migraciones con nombre descriptivo; **nunca borrar migraciones**.
- Operaciones con varios efectos (fallar batalla, cancelar torneo) **atómicas**: un solo `SaveChanges`
  o transacción explícita.
- La carpeta `Datos` lleva los scripts `.sql` (base vacía y con datos de prueba) y la colección de
  Postman.

## 7. API REST

Base `/api/v1`. El código **implementa** el contrato; si falta un endpoint en la especificación del
equipo, frenar y proponer agregarlo.

| Recurso | Endpoints |
|---|---|
| Sesiones | `POST /sesiones` · `DELETE /sesiones/{token}` (solo las propias) |
| Jugadores | `POST /jugadores` (registro público) |
| Usuarios (admin) | `POST /usuarios` · `GET /usuarios` · `GET/PUT/DELETE /usuarios/{id}` |
| Repertorio | `GET/POST /repertorios` · `DELETE /repertorios/{skillId}` (el jugador sale del token) |
| Skills | `GET /skills` · `GET /skills/{id}` · `POST /skills` · `PUT /skills/{id}` · `PATCH /skills/{id}` |
| Torneos | `GET /torneos` · `GET /torneos/{id}` · `POST /torneos` · `PUT /torneos/{id}` · `POST /torneos/{id}/cancelaciones` · `POST /torneos/{id}/finalizaciones` |
| Skills obligatorias | `GET/POST /torneos/{id}/skills-obligatorias` · `DELETE /torneos/{id}/skills-obligatorias/{skillId}` |
| Rondas | `GET/POST /torneos/{id}/rondas` · `GET /torneos/{id}/rondas/{numero}` |
| Inscripciones | `POST /inscripciones` · `GET /inscripciones` · `GET /inscripciones/{id}` · `POST /inscripciones/{id}/cancelaciones` |
| Batallas | `GET /batallas` · `GET /batallas/{id}` |
| Lanzamientos | `GET/POST /batallas/{id}/lanzamientos` |
| Fallos | `POST /batallas/{id}/fallos` |
| Ranking | `GET /rankings` (público, token opcional) |

- Route = identidad del recurso; query = filtros/orden/paginación (no obligatorios salvo el torneo en
  `GET /batallas`); body = datos de la operación; GET y DELETE sin body.
- Rutas de **hasta tres niveles** (el id final no cuenta, como en `../users/{id}/orders/{id}`), en minúscula
  y con guiones. Recursos **en plural y consistentes** (`rankings`, no `ranking`).
- **Sin segmentos fijos como `actual`**: no son un identificador válido. El usuario autenticado sale del token
  y el recurso se identifica con su id (revisión docente del 05/10, D-69).
- Paginado con `limit`/`offset`, con valor por defecto y tope. **Fuera de rango no se rechaza**, "para no
  trancar al usuario": `limit` mayor al tope usa el tope, `limit` menor a 1 usa el valor por defecto,
  `offset` negativo usa 0, y un filtro con un valor inválido se ignora. Un tipo mal formado
  (`limit=abc`) sigue siendo 400. El tipo de paginado (offset o cursor) se documenta con sus
  alternativas: los docentes lo piden como obligatorio.
- Identidad **solo** desde `Authorization: Bearer {token}` (token opaco persistido, no JWT salvo
  decisión documentada). Nunca aceptar `userId`/`jugadorId` en query o body para el usuario actual.
- Filters separados (SRP): **autenticación** (valida la sesión en la base → 401), **autorización**
  por rol con enum/constantes (→ 403), **exception filter global** (diccionario tipo → status; caso
  base 500 con mensaje fijo). Reglas como "¿sos el organizador de *este* torneo?" van en el servicio.
- Status: 200 · 201 (+ `Location`) · 204 · 400 datos inválidos · 401 · 403 · 404 · 409 regla de
  negocio/estado/unicidad · 500 solo fallos inesperados. Nunca 500 por culpa del cliente ni 200 con
  error en el body.
- Cuerpo de error: `{ "userMessage", "errorCode", "moreInfo", "detalles" }`. Nunca exponer stack
  trace, SQL, connection strings, paths ni `exception.Message` en el 500.
- JSON y query en camelCase; recursos en plural; operaciones no CRUD como sub-recursos sustantivados.
- Validar en el borde (`[ApiController]` + data annotations → 400) **y** en el dominio.
- En la especificación, por endpoint: también el protocolo y de dónde sale cada parámetro de ruta; los
  objetos complejos se describen aparte y se enlazan. Swagger sirve solo si lista todas las respuestas.
- CORS: los docentes lo sugieren (no se evalúa).

## 8. Reglas de dominio clave

La letra oficial manda. Resumen de lo que más se rompe:

- **Usuarios**: un solo rol (Jugador, Organizador, Administrador); sin subclases salvo decisión.
  Registro de jugador: ELO 1200, repertorio vacío. Login sin revelar si falló email o contraseña.
  Un admin no se modifica ni elimina a sí mismo.
- **Torneos**: `Abierto → EnCurso → Finalizado`, o `Cancelado`. **Las fechas son informativas**:
  nada de jobs, timers ni transiciones automáticas. Cupo potencia de 2 entre 4 y 64. No se modifica
  si está EnCurso, Finalizado o Cancelado, y el cupo no baja de las inscripciones Aceptadas (409, sin
  cancelar inscripciones; foro). Cancelar (desde Abierto/EnCurso) cancela batallas no finalizadas e
  inscripciones Aceptadas, atómicamente. El anónimo no ve torneos Cancelados; el autenticado ve todos
  los estados, sin importar el rol (foro).
- **Skill inactiva**: no puede declararse en un repertorio nuevo ni agregarse como obligatoria nueva,
  y el rechazo dice que **no es posible utilizarla porque está inactiva**, con `errorCode` propio.
  **No es retroactiva**: sí puede lanzarse en batalla si ya estaba en el repertorio.
- **Repertorio**: eliminar una skill solo está bloqueado si es obligatoria de un torneo **y** el
  jugador tiene inscripción **Aceptada** en él **y** el torneo no está Finalizado ni Cancelado.
- **Skills obligatorias**: 0..2; torneo Abierto y skill activa; si hay inscriptos Aceptados que no la
  tienen, rechazar identificándolos.
- **Inscripción**: torneo existe, está Abierto, jugador sin inscripción Aceptada previa, tiene todas
  las obligatorias, Aceptadas < Cupo. Cancelar: solo la propia y con torneo Abierto.
- **Batallas**: 2 jugadores, 4 lanzamientos cada uno, turnos alternados. Empieza el de mayor ELO al
  crearla; empate → azar **inyectable**. Lanzar: batalla Pendiente, participa, es su turno, skill en
  su repertorio, no repetida; si `obligatoriasPendientes == turnosRestantes`, solo puede lanzar una
  obligatoria pendiente. Tras el 8.º lanzamiento → `EnArbitraje`.
- **Fallar batalla**: Pendiente o EnArbitraje; solo el **organizador asignado** a ese torneo; ganador
  es uno de los dos; comentario de 10 a 500 caracteres (letra oficial; el borrador leído en clase decía
  10 a 15). Efectos **atómicos**: nuevos ELO → ganador y comentario → Finalizada →
  inscripción del perdedor Eliminada.
- **ELO**: K = 32; `Ea = 1 / (1 + 10^((Rb − Ra) / 400))`; `Ra' = Ra + K·(S − Ea)`. Calcular **ambos**
  con los valores **previos** de ambos; redondear; piso 100.
- **Rondas**: la primera requiere Abierto y Aceptadas == Cupo (→ EnCurso). Emparejamiento
  **determinístico y documentado** con desempate explícito (nunca el orden accidental de SQL).
  Finalizar: EnCurso, sin batallas sin resolver, un solo jugador no eliminado → Campeón.
- **Notificaciones**: la letra oficial 2026.2 **no las incluye** (estaban en el borrador leído en
  clase). Postergadas: no implementarlas salvo aclaración en el foro. Si vuelven: turno del rival,
  EnArbitraje al organizador y resultado a ambos jugadores, con tipos nuevos que se agreguen **sin
  modificar el código que dispara los eventos** (OCP).
- **Ranking**: público; `cantidad` (default 10), `torneo` opcional; ELO DESC con desempate
  determinístico. Solo usuarios con rol Jugador hoy. Con `torneo`: entran Aceptada o Eliminada, se ordena
  por el ELO global y las batallas ganadas/perdidas son las **globales**, no las del torneo (foro).

Errores: formato/rango → 400; inexistente → 404; rol → 403; estado, unicidad, cupo, turno → 409.
"No es el organizador asignado": 403 o 409 según la decisión registrada en la bitácora (si no hay,
preguntar).

## 9. Git y Pull Requests

**GitFlow**: `main` ← `release/*`, `hotfix/*`; `develop` ← `feature/*`. Nunca `feature → main`.

```bash
git switch develop && git pull --ff-only origin develop
git switch -c feature/<tema>
```

**Commits**: `tipo [ETAPA] (ruta): descripción`. Descripción en español, presente, minúscula, sin
punto final, ~70 caracteres. Ruta: archivo, carpeta o proyecto afectado.

```text
test [RED] (FarmeORT.Servicios.Tests/Inscripciones/): rechaza torneo cerrado
feat [GREEN] (FarmeORT.Servicios/Inscripciones/): valida estado del torneo
refactor [REFACTOR] (FarmeORT.Servicios/Inscripciones/): extrae validación de elegibilidad
fix [GREEN] (FarmeORT.Servicios/Batallas/): usa ELO previo de ambos jugadores
docs (docs/api/): actualiza especificación REST
```

Los commits no TDD (`docs`, `ci`, `chore`) van **sin** etapa. Una etapa por commit.

**Pull Requests**
- Siempre a `develop` (o `release/*`/`hotfix/*` → `main`). PRs chicos.
- La descripción usa **la plantilla obligatoria** `pull_request_template.md`, completando todos sus
  campos: *Descripcion de los cambios* (qué, contrato, ciclos TDD, impacto en UML/docs/Postman,
  captura si hay endpoints nuevos), *Hay nuevas migraciones?* (Si + nombre / No), *Requerimiento* y
  *Trabajado con* (compañero de pairing; nunca como `Co-Authored-By`).
- Merge **solo con merge commit** (squash y rebase deshabilitados), con `Build`, `Test` y `Analysis`
  en verde y review técnica de otro integrante. Nunca squashear la evidencia RED/GREEN/REFACTOR.
- Reviews: "LGTM" no cuenta. Cada comentario como **Problema / Impacto / Sugerencia**, con severidad
  (bloqueante / importante / menor). Revisar: requisito y reglas de negocio, evidencia TDD, calidad
  de tests, contrato y status, arquitectura, SOLID/GRASP, Clean Code, seguridad, persistencia
  (migración coherente con la plantilla), que no se toque la configuración obligatoria y alcance.
  La aprobación la da y firma el integrante, no la IA.

**Entrega**: merge a `main` y release **antes de las 21:00** del día de entrega; no tocar `main`
después. Estructura: raíz (código), `Aplicacion` (build Release e instalables), `Datos` (scripts
`.sql`, colección Postman, guía de instalación).

## 10. Documentación, Postman y UML

- La documentación se actualiza **junto con el código**, no al final. Justificar el **por qué**.
- **Bitácora** (solo decisiones con impacto; lo trivial va como supuesto): problema, alternativas
  consideradas, decisión, justificación, conceptos del curso, evidencia, participación de IA. En la
  participación de IA, decir qué se aceptó, qué se rechazó o modificó y qué problemas aparecieron: no
  alcanza con "propuesta de la IA, aprobada".
- **Declaración de uso de IA**: herramientas y modelos, para qué, en qué partes, los archivos de
  contexto usados (incluido este `AGENTS.md`) y prompts representativos. En los informes se marca qué
  texto generó la IA, y se suma una reflexión propia. Todo lo generado debe ser entendido y defendible
  por el equipo.
- **Informes**: explicar decisiones (por qué), no describir ("X contiene…" no es aceptable), sin que una
  justificación contradiga a otra. Cada paquete con su responsabilidad y un diagrama de clases, y una
  sección de manejo de excepciones con la tabla tipo → status.
- **Postman** (Collection v2.1 en `Datos`): carpeta por recurso; variables `baseUrl`, tokens por rol
  e ids guardados con `pm.collectionVariables.set`; test de status en cada request y `errorCode` en
  los de error; en las funcionalidades con asterisco, casos de borde, 401/403/404 y cada regla 409.
  Debe correr de punta a punta con el Runner.
- **UML 2** alineado con el código real: paquetes con nesting, clases (multiplicidades: Torneo→Skills
  obligatorias 0..2, Batalla→Jugadores 2, Batalla→Lanzamientos 0..8, Batalla→Fallo 0..1,
  Torneo→Campeón 0..1), secuencia de las funcionalidades clave, componentes con lollipop (provee) /
  hook (requiere). Nada de diagramas autogenerados con notación incorrecta.
- Lo que piden los docentes sobre los diagramas:
  - Se hacen **antes del código** de cada funcionalidad; el de componentes puede hacerse después.
  - Para el Obligatorio 1 van clases, secuencia, paquetes y componentes. No van objetos, estados,
    actividad, despliegue ni casos de uso.
  - El modelo de tablas va en notación MER.
  - En componentes, todas las interfaces provistas y requeridas; donde no hay interfaz, una dependencia
    de uso.
  - Secuencia solo de funcionalidades relevantes (autenticación, inscripción, lanzar skill, fallar
    batalla, rondas); un CRUD como crear skill no hace falta.
- **Entrega**: solo se corrige lo que se sube a gestión. El release de GitHub incluye todo lo necesario
  para probar fuera del IDE (`Aplicacion`, y en `Datos` los scripts `.sql`, la colección de Postman y
  la guía). Todos los integrantes documentan y tocan todas las partes, cada uno con su cuenta; en la
  defensa pueden preguntarle a cualquiera por cualquier parte.

## 11. Definition of Done

- [ ] Requisito entendido y reglas de dominio respetadas
- [ ] Endpoint, DTOs y status codes según la especificación
- [ ] RED, GREEN y REFACTOR (evaluado) en commits separados
- [ ] `dotnet build`, `dotnet test` y `dotnet format` sin errores ni cambios pendientes en local
- [ ] Cobertura ≥ 90 % (líneas y ramas) en cada assembly
- [ ] Persistencia y migración correctas; operaciones con varios efectos atómicas
- [ ] Postman, UML y documentación actualizados
- [ ] PR a `develop` con la plantilla completa, `Build`/`Test`/`Analysis` en verde, review técnica
      y approval, merge con merge commit

## 12. Cómo reportar al integrante

Al terminar cada paso: qué se hizo, qué commit corresponde (con el mensaje sugerido en el formato de
la sección 9) y cuál es el próximo paso. No encadenar varios ciclos TDD sin mostrar cada RED: el
integrante tiene que poder revisar y commitear cada etapa. Si algo es ambiguo o contradice la letra,
preguntar y proponer registrarlo como supuesto en la documentación.
