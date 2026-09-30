using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests;

internal sealed class BaseDeDatosEnMemoria : IDisposable
{
    private readonly SqliteConnection _conexion;

    public BaseDeDatosEnMemoria()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        var opciones = new DbContextOptionsBuilder<FarmeORTContext>()
            .UseSqlite(_conexion)
            .Options;
        Contexto = new FarmeORTContext(opciones);
        Contexto.Database.EnsureCreated();
    }

    public FarmeORTContext Contexto { get; }

    public void Dispose()
    {
        Contexto.Dispose();
        _conexion.Dispose();
    }
}
