using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Usuarios;

namespace FarmeORT.Repositorios.Tests.Usuarios;

[TestClass]
public sealed class RepositorioRankingTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioUsuarios _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioUsuarios(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void ListarRanking_UsuariosConDistintoElo_DevuelveSoloJugadoresDeMayorAMenorElo()
    {
        AgregarUsuario("bajo", 1300);
        AgregarUsuario("alto", 1500);
        AgregarUsuario("organizador", 2000, Rol.Organizador);
        AgregarUsuario("medio", 1400);
        GuardarYLimpiar();

        var ranking = _repositorio.ListarRanking(10);

        CollectionAssert.AreEqual(new[] { "alto", "medio", "bajo" }, ranking.Select(u => u.Nickname).ToArray());
    }

    private void AgregarUsuario(string nickname, int elo, Rol rol = Rol.Jugador)
    {
        var usuario = new Usuario("Nombre", "Apellido", $"{nickname}@farmeort.com", nickname, new DateOnly(2000, 1, 15), rol, "hash", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(usuario);

        // El ELO solo cambia al fallar batallas: para preparar el ranking se fija directo en la base.
        _baseDeDatos.Contexto.Entry(usuario).Property(u => u.Elo).CurrentValue = elo;
    }

    private void GuardarYLimpiar()
    {
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
    }
}
