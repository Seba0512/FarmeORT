using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Sesiones;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios;

public sealed class FarmeORTContext(DbContextOptions<FarmeORTContext> options) : DbContext(options)
{
    private const int LargoMaximoNombre = 50;
    private const int LargoMaximoApellido = 25;
    private const int LargoMaximoEmail = 254;
    private const int LargoMaximoNickname = 20;
    private const int LargoMaximoContrasenaHash = 128;
    private const int LargoMaximoRol = 20;
    private const int LargoMaximoCodigoSkill = 20;
    private const int LargoMaximoNombreSkill = 40;
    private const int LargoMaximoDescripcionSkill = 300;
    private const int LargoMaximoCategoriaSkill = 20;
    private const int LargoMaximoCodigoTorneo = 20;
    private const int LargoMaximoNombreTorneo = 60;
    private const int LargoMaximoDescripcionTorneo = 500;
    private const int LargoMaximoEstadoTorneo = 20;

    // Objeto anónimo porque HasData necesita fijar el Id, que el constructor de Usuario no recibe.
    // Hash PBKDF2 (formato de HasheadorContrasenas) calculado una sola vez, para que la migración no cambie en cada build.
    private static readonly object AdministradorPrecargado = new
    {
        Id = 1,
        Nombre = "Administrador",
        Apellido = "FarmeORT",
        Email = "admin@farmeort.com",
        Nickname = "admin",
        FechaNacimiento = new DateOnly(2000, 1, 1),
        Rol = Rol.Administrador,
        ContrasenaHash = "3LS7NmeR5ARDHCPv9u/dfA==.SXnTh+G3L29NXz1uN1cIe0KKW2S7Mpxc1vmT7g1RKw0=",
        Elo = 1200,
        Eliminado = false,
    };

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Sesion> Sesiones => Set<Sesion>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<Torneo> Torneos => Set<Torneo>();

    // Persona B: batallas y lanzamientos.
    public DbSet<Batalla> Batallas => Set<Batalla>();

    public DbSet<Lanzamiento> Lanzamientos => Set<Lanzamiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(usuario =>
        {
            usuario.HasKey(u => u.Id);
            usuario.Property(u => u.Nombre).HasMaxLength(LargoMaximoNombre).IsRequired();
            usuario.Property(u => u.Apellido).HasMaxLength(LargoMaximoApellido).IsRequired();
            usuario.Property(u => u.Email).HasMaxLength(LargoMaximoEmail).IsRequired();
            usuario.HasIndex(u => u.Email).IsUnique();
            usuario.Property(u => u.Nickname).HasMaxLength(LargoMaximoNickname).IsRequired();
            usuario.HasIndex(u => u.Nickname).IsUnique();
            usuario.Property(u => u.ContrasenaHash).HasMaxLength(LargoMaximoContrasenaHash).IsRequired();
            usuario.Property(u => u.Rol).HasConversion<string>().HasMaxLength(LargoMaximoRol);
            usuario.HasData(AdministradorPrecargado);

            // El repertorio se lee y se escribe a través del campo: la colección pública es de solo lectura.
            usuario.Navigation(u => u.Repertorio).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SkillDeclarada>(declarada =>
        {
            // La clave compuesta impide declarar dos veces la misma skill también en la base.
            declarada.ToTable("SkillsDeclaradas");
            declarada.HasKey(d => new { d.JugadorId, d.SkillId });
            declarada.HasOne<Usuario>()
                .WithMany(u => u.Repertorio)
                .HasForeignKey(d => d.JugadorId)
                .OnDelete(DeleteBehavior.Cascade);
            declarada.HasOne(d => d.Skill)
                .WithMany()
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sesion>(sesion =>
        {
            sesion.HasKey(s => s.Token);
            sesion.Property(s => s.Token).ValueGeneratedNever();
            sesion.HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Skill>(skill =>
        {
            skill.HasKey(s => s.Id);
            skill.Property(s => s.Codigo).HasMaxLength(LargoMaximoCodigoSkill).IsRequired();
            skill.HasIndex(s => s.Codigo).IsUnique();
            skill.Property(s => s.Nombre).HasMaxLength(LargoMaximoNombreSkill).IsRequired();
            skill.Property(s => s.Descripcion).HasMaxLength(LargoMaximoDescripcionSkill).IsRequired();
            skill.Property(s => s.Categoria).HasConversion<string>().HasMaxLength(LargoMaximoCategoriaSkill);
        });

        modelBuilder.Entity<Torneo>(torneo =>
        {
            torneo.HasKey(t => t.Id);
            torneo.Property(t => t.Codigo).HasMaxLength(LargoMaximoCodigoTorneo).IsRequired();
            torneo.HasIndex(t => t.Codigo).IsUnique();
            torneo.Property(t => t.Nombre).HasMaxLength(LargoMaximoNombreTorneo).IsRequired();
            torneo.Property(t => t.Descripcion).HasMaxLength(LargoMaximoDescripcionTorneo).IsRequired();
            torneo.Property(t => t.Estado).HasConversion<string>().HasMaxLength(LargoMaximoEstadoTorneo);
            torneo.HasIndex(t => t.Estado);

            // Restrict: un organizador con torneos no se borra físicamente (la baja de usuarios es lógica, D-27).
            torneo.HasOne(t => t.Organizador)
                .WithMany()
                .HasForeignKey(t => t.OrganizadorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        ConfigurarSkillsObligatorias(modelBuilder);
        ConfigurarBatallas(modelBuilder);
    }

    // Muchos a muchos sin datos propios: una skill puede ser obligatoria en varios torneos (D-40).
    private static void ConfigurarSkillsObligatorias(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Torneo>()
            .HasMany(t => t.SkillsObligatorias)
            .WithMany()
            .UsingEntity(
                "SkillsObligatorias",
                skill => skill.HasOne(typeof(Skill)).WithMany().HasForeignKey("SkillId").OnDelete(DeleteBehavior.Restrict),
                torneo => torneo.HasOne(typeof(Torneo)).WithMany().HasForeignKey("TorneoId").OnDelete(DeleteBehavior.Cascade),
                obligatoria => obligatoria.HasKey("TorneoId", "SkillId"));
        modelBuilder.Entity<Torneo>()
            .Navigation(t => t.SkillsObligatorias)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    // Persona B: configuración independiente de las entidades de A y C.
    private static void ConfigurarBatallas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Batalla>(batalla =>
        {
            batalla.HasKey(b => b.Id);
            batalla.Property(b => b.Estado).HasConversion<string>().HasMaxLength(20);
            batalla.HasOne(b => b.Torneo)
                .WithMany()
                .HasForeignKey(b => b.TorneoId)
                .OnDelete(DeleteBehavior.Restrict);
            batalla.HasOne(b => b.Jugador1)
                .WithMany()
                .HasForeignKey(b => b.Jugador1Id)
                .OnDelete(DeleteBehavior.Restrict);
            batalla.HasOne(b => b.Jugador2)
                .WithMany()
                .HasForeignKey(b => b.Jugador2Id)
                .OnDelete(DeleteBehavior.Restrict);
            batalla.Navigation(b => b.Lanzamientos).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Lanzamiento>(lanzamiento =>
        {
            lanzamiento.ToTable("Lanzamientos", tabla =>
                tabla.HasCheckConstraint("CK_Lanzamientos_Turno", "[Turno] BETWEEN 1 AND 8"));
            lanzamiento.HasKey(l => l.Id);
            lanzamiento.HasIndex(l => new { l.BatallaId, l.Turno }).IsUnique();
            lanzamiento.HasIndex(l => new { l.BatallaId, l.JugadorId, l.SkillId }).IsUnique();
            lanzamiento.HasOne(l => l.Batalla)
                .WithMany(b => b.Lanzamientos)
                .HasForeignKey(l => l.BatallaId)
                .OnDelete(DeleteBehavior.Cascade);
            lanzamiento.HasOne(l => l.Jugador)
                .WithMany()
                .HasForeignKey(l => l.JugadorId)
                .OnDelete(DeleteBehavior.Restrict);
            lanzamiento.HasOne(l => l.Skill)
                .WithMany()
                .HasForeignKey(l => l.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
