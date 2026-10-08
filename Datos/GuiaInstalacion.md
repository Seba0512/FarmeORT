# Guía de instalación de la base de datos — FarmeORT

Esta guía explica cómo crear la base de datos de FarmeORT en SQL Server, con o sin datos de prueba, y cómo
conectar la API a esa base. Los comandos se escriben desde la raíz del repositorio (o desde la carpeta que
contiene `Datos`).

## 1. Qué hay en `Datos`

| Archivo | Para qué sirve |
|---|---|
| `FarmeORT-BaseVacia.sql` | Crea la base `FarmeORT` con todas las tablas y solo el administrador precargado. |
| `FarmeORT-BaseConDatos.sql` | Crea la misma base y le carga datos de prueba: usuarios, skills, repertorios y un torneo en cada estado (sección 6). |
| `FarmeORT.postman_collection.json` | Colección de Postman para probar la API de punta a punta. |
| `FarmeORT-Evidencia.postman_collection.json` | Colección de Postman de la evidencia de las funcionalidades. |

Los dos scripts son independientes: se corre **uno solo**. Los dos se generaron con las migraciones de EF
Core y registran cada una en la tabla `__EFMigrationsHistory`, así que la API (y `dotnet ef`) encuentran la
base al día.

> **Atención:** cada script **borra la base `FarmeORT` si ya existe** y la vuelve a crear. Así se puede
> correr más de una vez y siempre deja el mismo estado, pero se pierden los datos que hubiera en esa base.

## 2. Requisitos

- **SQL Server**: SQL Server Express (instancia `.\SQLEXPRESS`) o el contenedor de Docker del repositorio
  (`docker-compose.yml`, SQL Server 2025 en `localhost,1433`).
- **Una herramienta para ejecutar el script**: `sqlcmd` (viene con SQL Server o con las herramientas de línea de
  comandos) o SQL Server Management Studio (SSMS) / Azure Data Studio.
- **Solo para la opción B o para correr la API desde el código**: el SDK de .NET 10 (el repositorio lo fija
  en `global.json`).

### Levantar SQL Server con Docker (opcional)

Si no se tiene SQL Server Express, crear en la raíz del repositorio un archivo `.env` (está en el
`.gitignore`) con la contraseña del usuario `sa`. SQL Server exige al menos 8 caracteres, con mayúsculas,
minúsculas, números y símbolos:

```text
MSSQL_SA_PASSWORD=<contraseña de sa>
```

Y levantar el contenedor:

```bash
docker compose up -d
```

## 3. Opción A: crear la base con un script (recomendada)

Elegir `FarmeORT-BaseVacia.sql` o `FarmeORT-BaseConDatos.sql` y ejecutarlo con **una** de estas formas.

**sqlcmd con SQL Server Express** (autenticación de Windows):

```bash
sqlcmd -S .\SQLEXPRESS -E -C -i Datos/FarmeORT-BaseConDatos.sql
```

**sqlcmd con el contenedor de Docker** (usuario `sa`):

```bash
sqlcmd -S localhost,1433 -U sa -P "<contraseña de sa>" -C -i Datos/FarmeORT-BaseConDatos.sql
```

- `-C` confía en el certificado del servidor (las instalaciones locales usan uno autofirmado).
- Para la base vacía, cambiar el nombre del archivo por `Datos/FarmeORT-BaseVacia.sql`.
- Al terminar no tiene que aparecer ningún mensaje `Msg ... Level 16`. Las líneas con números
  (`(1 rows affected)`) son normales.

**SSMS o Azure Data Studio:** conectarse al servidor, abrir el archivo (*File → Open → File...*) y
ejecutarlo completo (*Execute* o F5). El script cambia solo a la base `master` y después a `FarmeORT`; no
hace falta elegir una base antes.

Comprobación rápida (debe devolver 11 migraciones; con datos, 14 usuarios, y en la base vacía, 1):

```bash
sqlcmd -S .\SQLEXPRESS -E -C -d FarmeORT -Q "SELECT COUNT(*) FROM __EFMigrationsHistory; SELECT COUNT(*) FROM Usuarios"
```

## 4. Opción B: crear la base vacía con `dotnet ef`

Es equivalente a `FarmeORT-BaseVacia.sql`: crea la base si no existe y aplica las migraciones. Necesita el
connection string configurado (sección 5).

```bash
dotnet tool restore
cd src/FarmeORT.Repositorios
dotnet ef database update --startup-project ../FarmeORT.WebAPI
```

## 5. Conectar la API a la base

La API lee el connection string `FarmeORT`. **No está en `appsettings.json`**, porque ahí quedaría versionada
la contraseña (decisión D-09 de la bitácora). Si falta, la API no arranca y muestra
`Falta el connection string 'FarmeORT' en la configuración.` Hay dos formas de darlo:

**a) Correr desde el código** (`dotnet run`, Visual Studio o Rider; entorno `Development`): crear
`src/FarmeORT.WebAPI/appsettings.Development.json` (está en el `.gitignore`).

Con SQL Server Express:

```json
{
  "ConnectionStrings": {
    "FarmeORT": "Server=.\\SQLEXPRESS;Database=FarmeORT;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Con el contenedor de Docker:

```json
{
  "ConnectionStrings": {
    "FarmeORT": "Server=localhost,1433;Database=FarmeORT;User Id=sa;Password=<contraseña de sa>;Encrypt=False;TrustServerCertificate=True;"
  }
}
```

Y levantar la API con el perfil `http`, que escucha en `http://localhost:5294` (el `baseUrl` de Postman):

```bash
dotnet run --project src/FarmeORT.WebAPI --launch-profile http
```

**b) Correr el build publicado** (carpeta `Aplicacion`, `FarmeORT.WebAPI.exe`): corre en entorno
`Production` y **no lee** `appsettings.Development.json`. El connection string se pasa con la variable de
entorno `ConnectionStrings__FarmeORT` (dos guiones bajos). En PowerShell:

```powershell
$env:ConnectionStrings__FarmeORT = "Server=.\SQLEXPRESS;Database=FarmeORT;Trusted_Connection=True;TrustServerCertificate=True;"
.\FarmeORT.WebAPI.exe --urls http://localhost:5294
```

> **Al publicar:** `dotnet publish` copia a la carpeta de salida el `appsettings.Development.json` que haya en
> la máquina, con su contraseña. Antes de subir `Aplicacion` al release hay que borrar ese archivo de la carpeta
> publicada.

## 6. Usuarios y datos de prueba

### Administrador precargado (las dos bases)

| Email | Contraseña | Rol |
|---|---|---|
| `admin@farmeort.com` | `Administrador67*` | Administrador |

Lo crea la primera migración (decisión D-11), así que existe en las dos bases y también con la opción B.

### Usuarios de prueba (solo `FarmeORT-BaseConDatos.sql`)

Todos tienen la contraseña **`FarmeORT.Prueba2026`**.

| Id | Email | Nickname | Rol | ELO | Observación |
|---|---|---|---|---|---|
| 2 | `lucia.pereira@farmeort.com` | `lpereira` | Organizador | — | Organiza los torneos 1, 3 y 5 |
| 3 | `martin.silva@farmeort.com` | `msilva` | Organizador | — | Organiza los torneos 2 y 4 |
| 4 | `sofia.rodriguez@farmeort.com` | `sofiar` | Jugador | 1200 | |
| 5 | `diego.fernandez@farmeort.com` | `diegof` | Jugador | 1246 | Campeón del torneo 1 |
| 6 | `valentina.gomez@farmeort.com` | `valeg` | Jugador | 1184 | No tiene `GEST02` (no puede inscribirse en el torneo 3) |
| 7 | `mateo.lopez@farmeort.com` | `mateol` | Jugador | 1170 | |
| 8 | `camila.martinez@farmeort.com` | `camim` | Jugador | 1200 | |
| 9 | `joaquin.diaz@farmeort.com` | `joaquind` | Jugador | 1200 | |
| 10 | `florencia.sosa@farmeort.com` | `florsosa` | Jugador | 1200 | |
| 11 | `nicolas.alvarez@farmeort.com` | `nicoa` | Jugador | 1200 | |
| 12 | `agustina.romero@farmeort.com` | `agusr` | Jugador | 1200 | Tiene en el repertorio la skill inactiva `MEME03` |
| 13 | `bruno.castro@farmeort.com` | `brunoc` | Jugador | 1200 | |
| 14 | `federico.nunez@farmeort.com` | `fedenunez` | Jugador | 1200 | Dado de baja: no inicia sesión ni aparece en el ranking |

### Torneos

| Id | Código | Estado | Cupo | Organiza | Obligatoria | Qué permite probar |
|---|---|---|---|---|---|---|
| 1 | `COPAPRIM26` | Finalizado | 4 | `lpereira` | `GEST01` | Dos rondas, fallos y campeón (`diegof`) |
| 2 | `LIGAINV26` | EnCurso | 8 | `msilva` | `VOCA01` | Ronda 1 en juego: batalla 4 Finalizada, 5 EnArbitraje (fallar con `msilva`), 6 Pendiente con turno de `nicoa`, 7 Pendiente con turno de `florsosa` |
| 3 | `RELAMP26` | Abierto | 4 | `lpereira` | `GEST02` | Inscribirse: 2 Aceptadas y 1 Cancelada |
| 4 | `VERANO26` | Abierto | 8 | `msilva` | — | Inscribirse sin obligatorias, editar el torneo |
| 5 | `EXHIB26` | Cancelado | 4 | `lpereira` | — | El anónimo no lo ve; sus 2 inscripciones están Canceladas |

Skills: `GEST01`, `GEST02`, `VOCA01`, `VOCA02`, `BAIL01`, `BAIL02`, `BAIL03`, `MEME01` y `MEME02` activas;
`MEME03` inactiva. Los ELO y los cambios de cada fallo se calcularon con las mismas reglas que usa la API
(K = 32, valores previos de ambos jugadores) y cada ronda sigue el emparejamiento por ELO.

## 7. Comprobar que funciona

Con la API levantada en `http://localhost:5294`:

```bash
curl -X POST http://localhost:5294/api/v1/sesiones -H "Content-Type: application/json" -d "{\"email\":\"diego.fernandez@farmeort.com\",\"contrasena\":\"FarmeORT.Prueba2026\"}"
```

Debe responder `201` con un `token`. En Postman, importar la colección: ya trae cargadas las variables
`baseUrl` (`http://localhost:5294`) y `adminContrasena` (la del administrador precargado), así que corre
con el Collection Runner sin completar nada. Si la API escucha en otro puerto, cambiar `baseUrl`.

## 8. Problemas frecuentes

| Síntoma | Causa y solución |
|---|---|
| Nombres con acentos rotos (`LucÃ­a`) | El script se cargó con otra codificación. Los archivos son UTF-8 con BOM, y `sqlcmd` y SSMS los leen bien. Si se copió el contenido a otro editor, guardarlo como UTF-8 o agregar `-f 65001` a `sqlcmd`. |
| `Falta el connection string 'FarmeORT' en la configuración.` | No se configuró el connection string (sección 5). Con el build publicado, `appsettings.Development.json` no se lee: usar la variable de entorno. |
| `Login failed for user 'sa'` | La contraseña no coincide con la del `.env` con el que se creó el contenedor. Si se cambió el `.env` después, el volumen conserva la anterior. |
| `The certificate chain was issued by an authority that is not trusted` | Falta `-C` en `sqlcmd` o `TrustServerCertificate=True` en el connection string. |
| `Cannot open database "FarmeORT"` | La base no existe todavía: correr uno de los scripts (sección 3) o la opción B. |

Si el equipo agrega una migración, los dos scripts se regeneran como indica su encabezado
(`dotnet ef migrations script --idempotent`).
