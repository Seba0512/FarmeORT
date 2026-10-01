using FarmeORT.Dominio.Sesiones;
using FarmeORT.Dominio.Skills;
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
    }
}
