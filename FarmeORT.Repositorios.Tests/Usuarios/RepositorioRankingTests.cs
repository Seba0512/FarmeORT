using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Usuarios;
using Microsoft.EntityFrameworkCore;

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

    [TestMethod]
    public void ListarRanking_EloEmpatado_DesempataPorIdDeMenorAMayor()
    {
        // Sin índice, SQLite devuelve los empates en el orden en que se guardaron y el test pasaría por casualidad.
        // Con un índice por ELO la base lo recorre al revés y los empates salen del id mayor al menor, como puede
        // pasar en SQL Server según el plan. Así el test solo pasa si la consulta pide el desempate.
        _baseDeDatos.Contexto.Database.ExecuteSqlRaw("CREATE INDEX IX_Usuarios_Elo_Prueba ON Usuarios (Elo);");
        AgregarUsuario("primero", 1300);
        AgregarUsuario("segundo", 1300);
        AgregarUsuario("tercero", 1300);
        GuardarYLimpiar();

        var ranking = _repositorio.ListarRanking(10);

        CollectionAssert.AreEqual(new[] { "primero", "segundo", "tercero" }, ranking.Select(u => u.Nickname).ToArray());
    }

    [TestMethod]
    public void ListarRanking_JugadorDadoDeBaja_NoAparece()
    {
        AgregarUsuario("activo", 1300);
        var dadoDeBaja = AgregarUsuario("debaja", 1500);
        dadoDeBaja.Eliminar();
        GuardarYLimpiar();

        var ranking = _repositorio.ListarRanking(10);

        CollectionAssert.AreEqual(new[] { "activo" }, ranking.Select(u => u.Nickname).ToArray());
    }

    [TestMethod]
    public void ListarRanking_MasJugadoresQueLaCantidad_DevuelveSoloLosPrimeros()
    {
        AgregarUsuario("tercero", 1300);
        AgregarUsuario("primero", 1500);
        AgregarUsuario("segundo", 1400);
        GuardarYLimpiar();

        var ranking = _repositorio.ListarRanking(2);

        CollectionAssert.AreEqual(new[] { "primero", "segundo" }, ranking.Select(u => u.Nickname).ToArray());
    }

    [TestMethod]
    public void ListarRankingDelTorneo_InscripcionesEnDistintosEstados_DevuelveSoloAceptadosYEliminadosDeMayorAMenorElo()
    {
        var organizador = AgregarUsuario("organizador", 1200, Rol.Organizador);
        var torneo = AgregarTorneo("COPAAURA", organizador);
        var otroTorneo = AgregarTorneo("COPAFARM", organizador);
        Inscribir(torneo, AgregarUsuario("aceptado", 1300));
        Inscribir(torneo, AgregarUsuario("eliminado", 1500)).Eliminar();
        Inscribir(torneo, AgregarUsuario("cancelado", 1600)).Cancelar();
        Inscribir(otroTorneo, AgregarUsuario("otrotorneo", 1700));
        AgregarUsuario("sininscripcion", 1800);
        var dadoDeBaja = AgregarUsuario("debaja", 1400);
        Inscribir(torneo, dadoDeBaja);
        dadoDeBaja.Eliminar();
        GuardarYLimpiar();

        var ranking = _repositorio.ListarRankingDelTorneo(torneo.Id, 10);

        CollectionAssert.AreEqual(new[] { "eliminado", "aceptado" }, ranking.Select(u => u.Nickname).ToArray());
    }

    private Torneo AgregarTorneo(string codigo, Usuario organizador)
    {
        var torneo = new Torneo(codigo, "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(30), 8, organizador, FechaActual);
        _baseDeDatos.Contexto.Torneos.Add(torneo);
        return torneo;
    }

    private Inscripcion Inscribir(Torneo torneo, Usuario jugador)
    {
        var inscripcion = new Inscripcion(torneo, jugador, new DateTime(2026, 10, 1, 12, 0, 0));
        _baseDeDatos.Contexto.Inscripciones.Add(inscripcion);
        return inscripcion;
    }

    private Usuario AgregarUsuario(string nickname, int elo, Rol rol = Rol.Jugador)
    {
        var usuario = new Usuario("Nombre", "Apellido", $"{nickname}@farmeort.com", nickname, new DateOnly(2000, 1, 15), rol, "hash", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(usuario);

        // El ELO solo cambia al fallar batallas: para preparar el ranking se fija directo en la base.
        _baseDeDatos.Contexto.Entry(usuario).Property(u => u.Elo).CurrentValue = elo;
        return usuario;
    }

    private void GuardarYLimpiar()
    {
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
    }
}
