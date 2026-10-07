/*
    FarmeORT - Base de datos vacía (SQL Server)

    Crea la base FarmeORT con el esquema de las 11 migraciones de EF Core, desde
    CreaUsuariosYSesiones hasta AgregaVersionDeConcurrenciaDelTorneo. Registra cada migración en
    __EFMigrationsHistory, así que después `dotnet ef database update` no intenta aplicarlas de nuevo.

    "Vacía" quiere decir sin datos de prueba: solo queda el administrador precargado (Id 1,
    admin@farmeort.com), que la migración CreaUsuariosYSesiones inserta con HasData (D-11). Sin él
    nadie podría crear usuarios. Su contraseña inicial está en la guía de instalación.

    Si la base FarmeORT ya existe, se borra y se vuelve a crear: el script siempre deja el mismo
    estado y se puede correr más de una vez. Correrlo sobre una base con datos los pierde.

    El esquema se generó, parado en FarmeORT.Repositorios, con:
        dotnet ef migrations script --idempotent --startup-project ../FarmeORT.WebAPI
    --idempotent hace falta aunque la base sea nueva: sin él, el ALTER TABLE que agrega una columna y
    el UPDATE que la completa quedan en el mismo lote y SQL Server rechaza el lote al compilarlo.
    Si se agrega una migración, hay que regenerarlo (y también el de datos de prueba).

    Ejecución con sqlcmd:
        sqlcmd -S localhost,1433 -U sa -P <contraseña> -C -f 65001 -i Datos/FarmeORT-BaseVacia.sql
    En SSMS o Azure Data Studio: abrir el archivo y ejecutarlo completo (usa separadores GO).
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

