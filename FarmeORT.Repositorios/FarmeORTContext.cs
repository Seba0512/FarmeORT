using FarmeORT.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios;

public sealed class FarmeORTContext(DbContextOptions<FarmeORTContext> options) : DbContext(options)
{
    private const int LargoMaximoEmail = 254;
    private const int LargoMaximoContrasenaHash = 128;
    private const int LargoMaximoRol = 20;

    public DbSet<Usuario> Usuarios => Set<Usuario>();

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
    }
}
