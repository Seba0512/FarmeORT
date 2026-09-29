using FarmeORT.Dominio.Sesiones;
using FarmeORT.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios;

public sealed class FarmeORTContext(DbContextOptions<FarmeORTContext> options) : DbContext(options)
{
    private const int LargoMaximoEmail = 254;
    private const int LargoMaximoContrasenaHash = 128;
    private const int LargoMaximoRol = 20;

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Sesion> Sesiones => Set<Sesion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(usuario =>
        {
            usuario.HasKey(u => u.Id);
            usuario.Property(u => u.Email).HasMaxLength(LargoMaximoEmail).IsRequired();
            usuario.HasIndex(u => u.Email).IsUnique();
            usuario.Property(u => u.ContrasenaHash).HasMaxLength(LargoMaximoContrasenaHash).IsRequired();
            usuario.Property(u => u.Rol).HasConversion<string>().HasMaxLength(LargoMaximoRol);
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
