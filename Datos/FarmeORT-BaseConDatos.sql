/*
    FarmeORT - Base de datos con datos de prueba (SQL Server)

    Crea la base FarmeORT con el mismo esquema que FarmeORT-BaseVacia.sql (las 11 migraciones de EF
    Core, registradas en __EFMigrationsHistory, y el administrador precargado) y al final le carga datos
    de prueba: organizadores, jugadores, skills, repertorios y torneos en los cuatro estados, con
    inscripciones, rondas, batallas, lanzamientos y fallos. El detalle está en la sección de datos.

    Si la base FarmeORT ya existe, se borra y se vuelve a crear: el script siempre deja el mismo
    estado y se puede correr más de una vez. Correrlo sobre una base con datos los pierde.

    El esquema se generó, parado en src/FarmeORT.Repositorios, con:
        dotnet ef migrations script --idempotent --startup-project ../FarmeORT.WebAPI
    (el porqué de --idempotent está en FarmeORT-BaseVacia.sql). Si se agrega una migración, hay que regenerarlo en los dos scripts.

    Ejecución con sqlcmd:
        sqlcmd -S localhost,1433 -U sa -P <contraseña> -C -f 65001 -i Datos/FarmeORT-BaseConDatos.sql
    En SSMS o Azure Data Studio: abrir el archivo y ejecutarlo completo (usa separadores GO).
    Pasos completos (SQL Server Express o Docker) en Datos/GuiaInstalacion.md.
*/

USE [master];
GO

IF DB_ID(N'FarmeORT') IS NOT NULL
BEGIN
    ALTER DATABASE [FarmeORT] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [FarmeORT];
END;
GO

CREATE DATABASE [FarmeORT];
GO

USE [FarmeORT];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930041829_CreaUsuariosYSesiones'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(254) NOT NULL,
        [Rol] nvarchar(20) NOT NULL,
        [ContrasenaHash] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930041829_CreaUsuariosYSesiones'
)
BEGIN
    CREATE TABLE [Sesiones] (
        [Token] uniqueidentifier NOT NULL,
        [UsuarioId] int NOT NULL,
        CONSTRAINT [PK_Sesiones] PRIMARY KEY ([Token]),
        CONSTRAINT [FK_Sesiones_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930041829_CreaUsuariosYSesiones'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ContrasenaHash', N'Email', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] ON;
    EXEC(N'INSERT INTO [Usuarios] ([Id], [ContrasenaHash], [Email], [Rol])
    VALUES (1, N''3LS7NmeR5ARDHCPv9u/dfA==.SXnTh+G3L29NXz1uN1cIe0KKW2S7Mpxc1vmT7g1RKw0='', N''admin@farmeort.com'', N''Administrador'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ContrasenaHash', N'Email', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930041829_CreaUsuariosYSesiones'
)
BEGIN
    CREATE INDEX [IX_Sesiones_UsuarioId] ON [Sesiones] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930041829_CreaUsuariosYSesiones'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930041829_CreaUsuariosYSesiones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930041829_CreaUsuariosYSesiones', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Apellido] nvarchar(25) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Elo] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [FechaNacimiento] date NOT NULL DEFAULT '0001-01-01';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Nickname] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Nombre] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    EXEC(N'UPDATE [Usuarios] SET [Apellido] = N''FarmeORT'', [Elo] = 1200, [FechaNacimiento] = ''2000-01-01'', [Nickname] = N''admin'', [Nombre] = N''Administrador''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Nickname] ON [Usuarios] ([Nickname]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930174615_AgregaDatosPersonalesAUsuarios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930174615_AgregaDatosPersonalesAUsuarios', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001020619_AgregaBajaLogicaAUsuarios'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001020619_AgregaBajaLogicaAUsuarios'
)
BEGIN
    EXEC(N'UPDATE [Usuarios] SET [Eliminado] = CAST(0 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001020619_AgregaBajaLogicaAUsuarios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001020619_AgregaBajaLogicaAUsuarios', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001060836_AgregaCatalogoDeSkills'
)
BEGIN
    CREATE TABLE [Skills] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(40) NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [Categoria] nvarchar(20) NOT NULL,
        [Dificultad] int NOT NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_Skills] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001060836_AgregaCatalogoDeSkills'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Skills_Codigo] ON [Skills] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001060836_AgregaCatalogoDeSkills'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001060836_AgregaCatalogoDeSkills', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001193556_AgregaTorneos'
)
BEGIN
    CREATE TABLE [Torneos] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(60) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [FechaInicio] date NOT NULL,
        [FechaFin] date NOT NULL,
        [Cupo] int NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [OrganizadorId] int NOT NULL,
        CONSTRAINT [PK_Torneos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Torneos_Usuarios_OrganizadorId] FOREIGN KEY ([OrganizadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001193556_AgregaTorneos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Torneos_Codigo] ON [Torneos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001193556_AgregaTorneos'
)
BEGIN
    CREATE INDEX [IX_Torneos_Estado] ON [Torneos] ([Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001193556_AgregaTorneos'
)
BEGIN
    CREATE INDEX [IX_Torneos_OrganizadorId] ON [Torneos] ([OrganizadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001193556_AgregaTorneos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001193556_AgregaTorneos', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001204555_AgregaRepertorioDeJugadores'
)
BEGIN
    CREATE TABLE [SkillsDeclaradas] (
        [JugadorId] int NOT NULL,
        [SkillId] int NOT NULL,
        [Nivel] int NOT NULL,
        [FechaDeclaracion] date NOT NULL,
        CONSTRAINT [PK_SkillsDeclaradas] PRIMARY KEY ([JugadorId], [SkillId]),
        CONSTRAINT [FK_SkillsDeclaradas_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SkillsDeclaradas_Usuarios_JugadorId] FOREIGN KEY ([JugadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001204555_AgregaRepertorioDeJugadores'
)
BEGIN
    CREATE INDEX [IX_SkillsDeclaradas_SkillId] ON [SkillsDeclaradas] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001204555_AgregaRepertorioDeJugadores'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001204555_AgregaRepertorioDeJugadores', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001220115_AgregaSkillsObligatoriasDeTorneos'
)
BEGIN
    CREATE TABLE [SkillsObligatorias] (
        [TorneoId] int NOT NULL,
        [SkillId] int NOT NULL,
        CONSTRAINT [PK_SkillsObligatorias] PRIMARY KEY ([TorneoId], [SkillId]),
        CONSTRAINT [FK_SkillsObligatorias_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SkillsObligatorias_Torneos_TorneoId] FOREIGN KEY ([TorneoId]) REFERENCES [Torneos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001220115_AgregaSkillsObligatoriasDeTorneos'
)
BEGIN
    CREATE INDEX [IX_SkillsObligatorias_SkillId] ON [SkillsObligatorias] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001220115_AgregaSkillsObligatoriasDeTorneos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001220115_AgregaSkillsObligatoriasDeTorneos', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE TABLE [Batallas] (
        [Id] int NOT NULL IDENTITY,
        [TorneoId] int NOT NULL,
        [NumeroRonda] int NOT NULL,
        [Jugador1Id] int NOT NULL,
        [Jugador2Id] int NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [JugadorInicialId] int NOT NULL,
        [JugadorTurnoId] int NULL,
        CONSTRAINT [PK_Batallas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Batallas_Torneos_TorneoId] FOREIGN KEY ([TorneoId]) REFERENCES [Torneos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Batallas_Usuarios_Jugador1Id] FOREIGN KEY ([Jugador1Id]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Batallas_Usuarios_Jugador2Id] FOREIGN KEY ([Jugador2Id]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE TABLE [Inscripciones] (
        [Id] int NOT NULL IDENTITY,
        [TorneoId] int NOT NULL,
        [JugadorId] int NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Inscripciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Inscripciones_Torneos_TorneoId] FOREIGN KEY ([TorneoId]) REFERENCES [Torneos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Inscripciones_Usuarios_JugadorId] FOREIGN KEY ([JugadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE TABLE [Lanzamientos] (
        [Id] int NOT NULL IDENTITY,
        [BatallaId] int NOT NULL,
        [Turno] int NOT NULL,
        [JugadorId] int NOT NULL,
        [SkillId] int NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Lanzamientos] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Lanzamientos_Turno] CHECK ([Turno] BETWEEN 1 AND 8),
        CONSTRAINT [FK_Lanzamientos_Batallas_BatallaId] FOREIGN KEY ([BatallaId]) REFERENCES [Batallas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Lanzamientos_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Lanzamientos_Usuarios_JugadorId] FOREIGN KEY ([JugadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE INDEX [IX_Batallas_Jugador1Id] ON [Batallas] ([Jugador1Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE INDEX [IX_Batallas_Jugador2Id] ON [Batallas] ([Jugador2Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE INDEX [IX_Batallas_TorneoId] ON [Batallas] ([TorneoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE INDEX [IX_Inscripciones_JugadorId] ON [Inscripciones] ([JugadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE INDEX [IX_Inscripciones_TorneoId_JugadorId] ON [Inscripciones] ([TorneoId], [JugadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lanzamientos_BatallaId_JugadorId_SkillId] ON [Lanzamientos] ([BatallaId], [JugadorId], [SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lanzamientos_BatallaId_Turno] ON [Lanzamientos] ([BatallaId], [Turno]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE INDEX [IX_Lanzamientos_JugadorId] ON [Lanzamientos] ([JugadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    CREATE INDEX [IX_Lanzamientos_SkillId] ON [Lanzamientos] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002023734_AgregaInscripcionesYBatallas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002023734_AgregaInscripcionesYBatallas', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002154826_AgregaFallosDeBatallas'
)
BEGIN
    ALTER TABLE [Batallas] ADD [CambioEloJugador1] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002154826_AgregaFallosDeBatallas'
)
BEGIN
    ALTER TABLE [Batallas] ADD [CambioEloJugador2] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002154826_AgregaFallosDeBatallas'
)
BEGIN
    ALTER TABLE [Batallas] ADD [Comentario] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002154826_AgregaFallosDeBatallas'
)
BEGIN
    ALTER TABLE [Batallas] ADD [GanadorId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002154826_AgregaFallosDeBatallas'
)
BEGIN
    CREATE INDEX [IX_Batallas_GanadorId] ON [Batallas] ([GanadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002154826_AgregaFallosDeBatallas'
)
BEGIN
    ALTER TABLE [Batallas] ADD CONSTRAINT [FK_Batallas_Usuarios_GanadorId] FOREIGN KEY ([GanadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002154826_AgregaFallosDeBatallas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002154826_AgregaFallosDeBatallas', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002194933_AgregaCampeonDeTorneos'
)
BEGIN
    ALTER TABLE [Torneos] ADD [CampeonId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002194933_AgregaCampeonDeTorneos'
)
BEGIN
    CREATE INDEX [IX_Torneos_CampeonId] ON [Torneos] ([CampeonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002194933_AgregaCampeonDeTorneos'
)
BEGIN
    ALTER TABLE [Torneos] ADD CONSTRAINT [FK_Torneos_Usuarios_CampeonId] FOREIGN KEY ([CampeonId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002194933_AgregaCampeonDeTorneos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002194933_AgregaCampeonDeTorneos', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002233651_AgregaVersionDeConcurrenciaDelTorneo'
)
BEGIN
    ALTER TABLE [Torneos] ADD [Version] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002233651_AgregaVersionDeConcurrenciaDelTorneo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002233651_AgregaVersionDeConcurrenciaDelTorneo', N'10.0.11');
END;

COMMIT;
GO


/*
    Datos de prueba

    Todo en una transacción: si un INSERT falla, la base queda solo con el esquema y el administrador.
    Los ids son fijos (IDENTITY_INSERT) para poder nombrarlos en la guía y en Postman.

    Los datos respetan las reglas del dominio como si se hubieran cargado por la API, en este orden:
    la Copa Primavera se juega completa, después arranca la Liga de Invierno. Los ELO y los cambios de
    cada fallo están calculados con CalculadorEloBase (K = 32, valores previos de ambos, redondeo) y el
    emparejamiento de cada ronda sigue EmparejamientoPorElo (mayor ELO contra menor, desempate por id).

    Contraseña de todos los usuarios de prueba (no del administrador precargado): FarmeORT.Prueba2026

    Usuarios
        2  lpereira   Organizador
        3  msilva     Organizador
        4  sofiar     Jugador  ELO 1200
        5  diegof     Jugador  ELO 1246  campeón de la Copa Primavera
        6  valeg      Jugador  ELO 1184
        7  mateol     Jugador  ELO 1170
        8  camim      Jugador  ELO 1200
        9  joaquind   Jugador  ELO 1200
        10 florsosa   Jugador  ELO 1200
        11 nicoa      Jugador  ELO 1200
        12 agusr      Jugador  ELO 1200  tiene en el repertorio la skill inactiva MEME03
        13 brunoc     Jugador  ELO 1200
        14 fedenunez  Jugador  ELO 1200  dado de baja (baja lógica): no inicia sesión ni aparece en el ranking

    Torneos
        1 COPAPRIM26  Finalizado  cupo 4, organiza lpereira, obligatoria GEST01, campeón diegof (2 rondas)
        2 LIGAINV26   EnCurso     cupo 8, organiza msilva, obligatoria VOCA01, ronda 1 en juego:
                                  batalla 4 Finalizada, 5 EnArbitraje, 6 Pendiente (turno de nicoa),
                                  7 Pendiente sin lanzamientos (turno de florsosa)
        3 RELAMP26    Abierto     cupo 4, organiza lpereira, obligatoria GEST02, 2 Aceptadas y 1 Cancelada
        4 VERANO26    Abierto     cupo 8, organiza msilva, sin obligatorias ni inscripciones
        5 EXHIB26     Cancelado   cupo 4, organiza lpereira, sus 2 inscripciones quedaron Canceladas
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Usuarios ------------------------------------------------------------------------------------------

SET IDENTITY_INSERT [Usuarios] ON;
INSERT INTO [Usuarios] ([Id], [Nombre], [Apellido], [Email], [Nickname], [FechaNacimiento], [Rol], [ContrasenaHash], [Elo], [Eliminado])
VALUES
    (2,  N'Lucía',     N'Pereira',   N'lucia.pereira@farmeort.com',    N'lpereira',  '1990-04-12', N'Organizador', N'F/YIOfxkCBatOEgRXFD0eQ==.3WV7tGKPqfDlbyT9gKXnuav+7Nt4oZ3diwVIj1tmj2Q=', 1200, CAST(0 AS bit)),
    (3,  N'Martín',    N'Silva',     N'martin.silva@farmeort.com',     N'msilva',    '1988-09-30', N'Organizador', N'PoujckXcpiAxLX2oVxKScw==.W6zZvkTswghBZvq6F+foVZwN05g3TIhkx6WdBjRj9jM=', 1200, CAST(0 AS bit)),
    (4,  N'Sofía',     N'Rodríguez', N'sofia.rodriguez@farmeort.com',  N'sofiar',    '2001-02-17', N'Jugador',     N'Cv1hwc2nwuxwj+8bwBU3jA==.gzjnAV+DTA8n6iJW2nSWNNM6lbimzui2MNQkgJ9EoLM=', 1200, CAST(0 AS bit)),
    (5,  N'Diego',     N'Fernández', N'diego.fernandez@farmeort.com',  N'diegof',    '1999-07-03', N'Jugador',     N'eGMV7jPL2G/VQqF8xyaeZQ==.I7ubmdz2y1MIOMEctWZealc1rglYBY2cVFkgFHYLo4U=', 1246, CAST(0 AS bit)),
    (6,  N'Valentina', N'Gómez',     N'valentina.gomez@farmeort.com',  N'valeg',     '2003-11-21', N'Jugador',     N'sGRJlNXeeVzwUd/G95L+Qg==.xwfH5e9Ib4NOpMz95NacA+uuOIwax4sJ/MpglT6Duug=', 1184, CAST(0 AS bit)),
    (7,  N'Mateo',     N'López',     N'mateo.lopez@farmeort.com',      N'mateol',    '2005-05-09', N'Jugador',     N'Juc0A8ICzYXRvEq3dWORpw==.oGKVghBvWNEbAXK8zOaISs/fNDgHe973e+jKvVg01kk=', 1170, CAST(0 AS bit)),
    (8,  N'Camila',    N'Martínez',  N'camila.martinez@farmeort.com',  N'camim',     '2000-08-14', N'Jugador',     N'BUn7f2kPjBBVrPsFXnrIjA==.Nv7r9Z8DhPZSseKLuZJRyxkh9TpuOxaMqhq8QVdWPh4=', 1200, CAST(0 AS bit)),
    (9,  N'Joaquín',   N'Díaz',      N'joaquin.diaz@farmeort.com',     N'joaquind',  '1997-01-25', N'Jugador',     N'+ziM2/9VDwIwSJtzJFVY+g==.h9sE1aoSwHg/IsKJ9jwQJWUSz8k00jD37gMmjldT2C0=', 1200, CAST(0 AS bit)),
    (10, N'Florencia', N'Sosa',      N'florencia.sosa@farmeort.com',   N'florsosa',  '2002-03-08', N'Jugador',     N'V30CRqBC8yxl1QlQ72zi4g==.JNicMvTJM2tqmW7qdOSiYsVJ3aLdf/yb/iOyYUMLs2Y=', 1200, CAST(0 AS bit)),
    (11, N'Nicolás',   N'Álvarez',   N'nicolas.alvarez@farmeort.com',  N'nicoa',     '2004-10-19', N'Jugador',     N'4pJyYUzPLxGm1+uz6qsCSg==.JHuoUaoBfufO6E8AQ84RE/x2xKhXIhRS+5L8sC16k3E=', 1200, CAST(0 AS bit)),
    (12, N'Agustina',  N'Romero',    N'agustina.romero@farmeort.com',  N'agusr',     '2006-06-11', N'Jugador',     N'q8tYee2LXfzggqlJzECBHw==.ye3+MC39U0Dw2cJV3JKYLX2xVFgaPMtyREFUHLfV2YI=', 1200, CAST(0 AS bit)),
    (13, N'Bruno',     N'Castro',    N'bruno.castro@farmeort.com',     N'brunoc',    '1998-12-02', N'Jugador',     N'bx+yjXA03YeNs7+6a1ahTw==.CowrEvKd8ijpkQ3/FWXnLaexFeUaszDjxMhqjeP+6uo=', 1200, CAST(0 AS bit)),
    (14, N'Federico',  N'Núñez',     N'federico.nunez@farmeort.com',   N'fedenunez', '1996-09-05', N'Jugador',     N'WOg5x2GuhfSZBGr0lEfnQA==.JpRoHJLGBTxfO4QoqyRb2oKJjcw5thRGL5NP7NHbUCI=', 1200, CAST(1 AS bit));
SET IDENTITY_INSERT [Usuarios] OFF;

-- Skills: MEME03 está inactiva (no se declara en repertorios nuevos, pero quien ya la tenía puede lanzarla).

SET IDENTITY_INSERT [Skills] ON;
INSERT INTO [Skills] ([Id], [Codigo], [Nombre], [Descripcion], [Categoria], [Dificultad], [Activa])
VALUES
    (1,  N'GEST01', N'Saludo militar',   N'Saludo con la mano en la frente y taconazo, sin perder la seriedad.',          N'Gestual', 1, CAST(1 AS bit)),
    (2,  N'GEST02', N'Palmas cruzadas',  N'Secuencia de palmas cruzadas a ritmo creciente durante diez segundos.',        N'Gestual', 2, CAST(1 AS bit)),
    (3,  N'VOCA01', N'Grito de guerra',  N'Grito corto y potente que tiene que escucharse en toda la sala.',              N'Vocal',   2, CAST(1 AS bit)),
    (4,  N'VOCA02', N'Beatbox básico',   N'Base de beatbox con bombo, caja y platillo sostenida durante ocho compases.',  N'Vocal',   3, CAST(1 AS bit)),
    (5,  N'BAIL01', N'Moonwalk',         N'Deslizamiento hacia atrás simulando caminar hacia adelante, sin cortes.',     N'Baile',   4, CAST(1 AS bit)),
    (6,  N'BAIL02', N'Robot',            N'Movimientos rígidos y entrecortados de brazos y cabeza al estilo robot.',      N'Baile',   3, CAST(1 AS bit)),
    (7,  N'MEME01', N'Dab',              N'Brazo extendido en diagonal y la cabeza escondida en el codo del otro brazo.', N'Meme',    1, CAST(1 AS bit)),
    (8,  N'MEME02', N'Cara de póker',    N'Mantener el rostro inexpresivo mientras el rival intenta hacerte reír.',       N'Meme',    2, CAST(1 AS bit)),
    (9,  N'BAIL03', N'Floss',            N'Balanceo de brazos de adelante hacia atrás cruzando la cadera, a buen ritmo.', N'Baile',   3, CAST(1 AS bit)),
    (10, N'MEME03', N'Gangnam style',    N'Paso del caballo con salto lateral y el lazo imaginario girando arriba.',       N'Meme',    4, CAST(0 AS bit));
SET IDENTITY_INSERT [Skills] OFF;

-- Repertorios: los jugadores de cada torneo tienen sus obligatorias (GEST01, VOCA01 o GEST02).
-- valeg no tiene GEST02: sirve para probar que no puede inscribirse en el torneo 3 (RELAMP26).

INSERT INTO [SkillsDeclaradas] ([JugadorId], [SkillId], [Nivel], [FechaDeclaracion])
VALUES
    (4, 1, 3, '2026-09-01'), (4, 2, 2, '2026-09-01'), (4, 3, 4, '2026-09-01'), (4, 5, 3, '2026-09-01'), (4, 7, 5, '2026-09-01'),
    (5, 1, 4, '2026-09-01'), (5, 3, 5, '2026-09-01'), (5, 4, 3, '2026-09-01'), (5, 6, 4, '2026-09-01'), (5, 8, 2, '2026-09-01'),
    (6, 1, 2, '2026-09-02'), (6, 3, 3, '2026-09-02'), (6, 5, 4, '2026-09-02'), (6, 7, 3, '2026-09-02'), (6, 9, 5, '2026-09-02'),
    (7, 1, 3, '2026-09-02'), (7, 3, 2, '2026-09-02'), (7, 4, 4, '2026-09-02'), (7, 8, 3, '2026-09-02'), (7, 9, 2, '2026-09-02'),
    (8, 2, 3, '2026-09-20'), (8, 3, 4, '2026-09-20'), (8, 5, 2, '2026-09-20'), (8, 6, 3, '2026-09-20'), (8, 7, 4, '2026-09-20'),
    (9, 3, 3, '2026-09-21'), (9, 4, 5, '2026-09-21'), (9, 6, 2, '2026-09-21'), (9, 8, 4, '2026-09-21'), (9, 9, 3, '2026-09-21'),
    (10, 1, 2, '2026-09-21'), (10, 3, 3, '2026-09-21'), (10, 5, 5, '2026-09-21'), (10, 7, 3, '2026-09-21'), (10, 8, 4, '2026-09-21'),
    (11, 2, 4, '2026-09-22'), (11, 3, 3, '2026-09-22'), (11, 4, 2, '2026-09-22'), (11, 6, 5, '2026-09-22'), (11, 9, 3, '2026-09-22'),
    (12, 1, 3, '2026-08-15'), (12, 2, 4, '2026-08-15'), (12, 4, 2, '2026-08-15'), (12, 5, 3, '2026-08-15'), (12, 10, 5, '2026-08-15'),
    (13, 2, 3, '2026-09-25'), (13, 3, 2, '2026-09-25'), (13, 6, 4, '2026-09-25'), (13, 7, 3, '2026-09-25'), (13, 8, 5, '2026-09-25'),
    (14, 1, 2, '2026-08-20'), (14, 7, 3, '2026-08-20');

-- Torneos: las fechas son informativas (no disparan cambios de estado).

SET IDENTITY_INSERT [Torneos] ON;
INSERT INTO [Torneos] ([Id], [Codigo], [Nombre], [Descripcion], [FechaInicio], [FechaFin], [Cupo], [Estado], [OrganizadorId], [CampeonId], [Version])
VALUES
    (1, N'COPAPRIM26', N'Copa Primavera 2026', N'Torneo de apertura de la temporada, eliminación directa a dos rondas.',          '2026-09-10', '2026-09-12', 4, N'Finalizado', 2, 5,    NEWID()),
    (2, N'LIGAINV26',  N'Liga de Invierno',    N'Ocho jugadores, eliminación directa. El grito de guerra es obligatorio.',        '2026-10-01', '2026-10-20', 8, N'EnCurso',    3, NULL, NEWID()),
    (3, N'RELAMP26',   N'Torneo Relámpago',    N'Torneo corto de cuatro jugadores en una sola tarde; palmas cruzadas obligatorias.', '2026-11-05', '2026-11-05', 4, N'Abierto',    2, NULL, NEWID()),
    (4, N'VERANO26',   N'Copa Verano Abierta', N'Torneo abierto a todos los jugadores, sin skills obligatorias.',                 '2026-12-10', '2026-12-20', 8, N'Abierto',    3, NULL, NEWID()),
    (5, N'EXHIB26',    N'Exhibición Benéfica', N'Exhibición suspendida por falta de sala; se canceló antes de empezar.',          '2026-10-15', '2026-10-15', 4, N'Cancelado',  2, NULL, NEWID());
SET IDENTITY_INSERT [Torneos] OFF;

INSERT INTO [SkillsObligatorias] ([TorneoId], [SkillId])
VALUES
    (1, 1),
    (2, 3),
    (3, 2);

-- Inscripciones: en un torneo jugado, cada perdedor queda Eliminada y el campeón sigue Aceptada.

SET IDENTITY_INSERT [Inscripciones] ON;
INSERT INTO [Inscripciones] ([Id], [TorneoId], [JugadorId], [Estado], [Fecha])
VALUES
    (1,  1, 4,  N'Eliminada', '2026-09-03T10:15:00'),
    (2,  1, 5,  N'Aceptada',  '2026-09-03T11:40:00'),
    (3,  1, 6,  N'Eliminada', '2026-09-04T09:05:00'),
    (4,  1, 7,  N'Eliminada', '2026-09-04T18:30:00'),
    (5,  2, 4,  N'Aceptada',  '2026-09-23T10:00:00'),
    (6,  2, 5,  N'Aceptada',  '2026-09-23T10:20:00'),
    (7,  2, 6,  N'Aceptada',  '2026-09-23T12:45:00'),
    (8,  2, 7,  N'Eliminada', '2026-09-24T08:10:00'),
    (9,  2, 8,  N'Aceptada',  '2026-09-24T15:00:00'),
    (10, 2, 9,  N'Aceptada',  '2026-09-25T09:30:00'),
    (11, 2, 10, N'Aceptada',  '2026-09-25T19:55:00'),
    (12, 2, 11, N'Aceptada',  '2026-09-26T11:25:00'),
    (13, 3, 12, N'Aceptada',  '2026-10-02T14:00:00'),
    (14, 3, 8,  N'Cancelada', '2026-10-03T09:45:00'),
    (15, 3, 13, N'Aceptada',  '2026-10-04T16:20:00'),
    (16, 5, 12, N'Cancelada', '2026-09-28T10:10:00'),
    (17, 5, 13, N'Cancelada', '2026-09-28T13:35:00');
SET IDENTITY_INSERT [Inscripciones] OFF;

/*
    Batallas. Jugador1 es el de mayor ELO del par al generar la ronda. Empieza el de mayor ELO y, si
    empatan, el que eligió el sorteo. CambioElo es la diferencia del ELO de cada jugador al fallar.

    Copa Primavera (todos en 1200):
        ronda 1: sofiar 1200 vs mateol 1200 -> gana sofiar (+16 / -16)
                 diegof 1200 vs valeg 1200  -> gana diegof (+16 / -16)
        ronda 2: sofiar 1216 vs diegof 1216 -> gana diegof (-16 / +16), campeón
    Liga de Invierno, ronda 1 (diegof 1232, sofiar 1200, camim, joaquind, florsosa y nicoa 1200,
    valeg 1184, mateol 1184):
        batalla 4: diegof vs mateol      -> gana diegof (+14 / -14)
        batalla 5: sofiar vs valeg       -> 8 lanzamientos, EnArbitraje (falta el fallo de msilva)
        batalla 6: camim vs nicoa        -> 3 lanzamientos, Pendiente, turno de nicoa
        batalla 7: joaquind vs florsosa  -> sin lanzamientos, Pendiente, empieza florsosa (sorteo)
*/

SET IDENTITY_INSERT [Batallas] ON;
INSERT INTO [Batallas] ([Id], [TorneoId], [NumeroRonda], [Jugador1Id], [Jugador2Id], [Estado], [JugadorInicialId], [JugadorTurnoId], [GanadorId], [Comentario], [CambioEloJugador1], [CambioEloJugador2])
VALUES
    (1, 1, 1, 4, 7,  N'Finalizada',  7, NULL, 4,    N'Sofía cerró con un dab impecable y dominó toda la batalla.',              16,  -16),
    (2, 1, 1, 5, 6,  N'Finalizada',  5, NULL, 5,    N'Diego sostuvo el beatbox sin errores; Valentina dudó en el floss.',     16,  -16),
    (3, 1, 2, 4, 5,  N'Finalizada',  4, NULL, 5,    N'Final muy pareja: el robot de Diego definió el campeonato.',            -16, 16),
    (4, 2, 1, 5, 7,  N'Finalizada',  5, NULL, 5,    N'Diego mantuvo la cara de póker hasta el final y Mateo no pudo responder.', 14, -14),
    (5, 2, 1, 4, 6,  N'EnArbitraje', 4, NULL, NULL, NULL,                                                                       NULL, NULL),
    (6, 2, 1, 8, 11, N'Pendiente',   8, 11,   NULL, NULL,                                                                       NULL, NULL),
    (7, 2, 1, 9, 10, N'Pendiente',   10, 10,  NULL, NULL,                                                                       NULL, NULL);
SET IDENTITY_INSERT [Batallas] OFF;

-- Lanzamientos: turnos alternados desde el jugador inicial, 4 por jugador, sin repetir skill, todas de su
-- repertorio y cada jugador lanza la obligatoria del torneo antes de quedarse sin turnos.

SET IDENTITY_INSERT [Lanzamientos] ON;
INSERT INTO [Lanzamientos] ([Id], [BatallaId], [Turno], [JugadorId], [SkillId], [Fecha])
VALUES
    (1,  1, 1, 7, 4, '2026-09-10T18:00:00-03:00'),
    (2,  1, 2, 4, 2, '2026-09-10T18:01:00-03:00'),
    (3,  1, 3, 7, 1, '2026-09-10T18:02:00-03:00'),
    (4,  1, 4, 4, 5, '2026-09-10T18:03:00-03:00'),
    (5,  1, 5, 7, 8, '2026-09-10T18:04:00-03:00'),
    (6,  1, 6, 4, 1, '2026-09-10T18:05:00-03:00'),
    (7,  1, 7, 7, 9, '2026-09-10T18:06:00-03:00'),
    (8,  1, 8, 4, 7, '2026-09-10T18:07:00-03:00'),
    (9,  2, 1, 5, 6, '2026-09-10T19:00:00-03:00'),
    (10, 2, 2, 6, 5, '2026-09-10T19:01:00-03:00'),
    (11, 2, 3, 5, 1, '2026-09-10T19:02:00-03:00'),
    (12, 2, 4, 6, 9, '2026-09-10T19:03:00-03:00'),
    (13, 2, 5, 5, 8, '2026-09-10T19:04:00-03:00'),
    (14, 2, 6, 6, 7, '2026-09-10T19:05:00-03:00'),
    (15, 2, 7, 5, 4, '2026-09-10T19:06:00-03:00'),
    (16, 2, 8, 6, 1, '2026-09-10T19:07:00-03:00'),
    (17, 3, 1, 4, 1, '2026-09-12T20:00:00-03:00'),
    (18, 3, 2, 5, 3, '2026-09-12T20:01:00-03:00'),
    (19, 3, 3, 4, 5, '2026-09-12T20:02:00-03:00'),
    (20, 3, 4, 5, 1, '2026-09-12T20:03:00-03:00'),
    (21, 3, 5, 4, 3, '2026-09-12T20:04:00-03:00'),
    (22, 3, 6, 5, 6, '2026-09-12T20:05:00-03:00'),
    (23, 3, 7, 4, 2, '2026-09-12T20:06:00-03:00'),
    (24, 3, 8, 5, 8, '2026-09-12T20:07:00-03:00'),
    (25, 4, 1, 5, 3, '2026-10-02T18:00:00-03:00'),
    (26, 4, 2, 7, 9, '2026-10-02T18:01:00-03:00'),
    (27, 4, 3, 5, 6, '2026-10-02T18:02:00-03:00'),
    (28, 4, 4, 7, 3, '2026-10-02T18:03:00-03:00'),
    (29, 4, 5, 5, 4, '2026-10-02T18:04:00-03:00'),
    (30, 4, 6, 7, 1, '2026-10-02T18:05:00-03:00'),
    (31, 4, 7, 5, 8, '2026-10-02T18:06:00-03:00'),
    (32, 4, 8, 7, 4, '2026-10-02T18:07:00-03:00'),
    (33, 5, 1, 4, 7, '2026-10-02T19:00:00-03:00'),
    (34, 5, 2, 6, 3, '2026-10-02T19:01:00-03:00'),
    (35, 5, 3, 4, 3, '2026-10-02T19:02:00-03:00'),
    (36, 5, 4, 6, 5, '2026-10-02T19:03:00-03:00'),
    (37, 5, 5, 4, 5, '2026-10-02T19:04:00-03:00'),
    (38, 5, 6, 6, 9, '2026-10-02T19:05:00-03:00'),
    (39, 5, 7, 4, 2, '2026-10-02T19:06:00-03:00'),
    (40, 5, 8, 6, 1, '2026-10-02T19:07:00-03:00'),
    (41, 6, 1, 8, 5, '2026-10-03T18:00:00-03:00'),
    (42, 6, 2, 11, 3, '2026-10-03T18:01:00-03:00'),
    (43, 6, 3, 8, 6, '2026-10-03T18:02:00-03:00');
SET IDENTITY_INSERT [Lanzamientos] OFF;

COMMIT;
GO
