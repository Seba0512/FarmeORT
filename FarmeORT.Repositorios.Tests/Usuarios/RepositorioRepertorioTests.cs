using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.Repositorios.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Usuarios;

// Repertorio del jugador: se lee y se guarda a través de RepositorioUsuarios (el repertorio es parte del usuario).
[TestClass]
public sealed class RepositorioRepertorioTests
{
    private const string Descripcion = "Gesto alternando las manos hacia arriba y abajo.";

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
    public void ObtenerConRepertorio_JugadorConSkills_LasTraeConSuSkill()
    {
        var skill = GuardarSkill("SIXSEVEN");
        var jugador = GuardarJugador("ana@farmeort.com", "anaperez");
        DeclararYGuardar(jugador.Id, skill.Id, 4);

        var resultado = _repositorio.ObtenerConRepertorio(jugador.Id);

        Assert.IsNotNull(resultado);
        var declarada = resultado.Repertorio.Single();
        Assert.AreEqual(skill.Id, declarada.SkillId);
        Assert.AreEqual("SIXSEVEN", declarada.Skill.Codigo);
        Assert.AreEqual(4, declarada.Nivel);
        Assert.AreEqual(FechaActual, declarada.FechaDeclaracion);
    }

    [TestMethod]
    public void ObtenerConRepertorio_UsuarioEliminadoOInexistente_DevuelveNull()
    {
        var eliminado = GuardarJugador("ana@farmeort.com", "anaperez");
        var cargado = _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == eliminado.Id);
        cargado.Eliminar();
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsNull(_repositorio.ObtenerConRepertorio(eliminado.Id));
        Assert.IsNull(_repositorio.ObtenerConRepertorio(99));
    }

    [TestMethod]
    public void GuardarRepertorio_SkillDeclaradaNueva_QuedaGuardada()
    {
        var skill = GuardarSkill("SIXSEVEN");
        var jugador = CargarConRepertorio(GuardarJugador("ana@farmeort.com", "anaperez").Id);
        jugador.DeclararSkill(_baseDeDatos.Contexto.Skills.Single(s => s.Id == skill.Id), 5, FechaActual);

        _repositorio.GuardarRepertorio(jugador);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var guardada = _baseDeDatos.Contexto.Set<SkillDeclarada>().Single(d => d.JugadorId == jugador.Id);
        Assert.AreEqual(skill.Id, guardada.SkillId);
        Assert.AreEqual(5, guardada.Nivel);
        Assert.AreEqual(FechaActual, guardada.FechaDeclaracion);
    }

    [TestMethod]
    public void GuardarRepertorio_SkillQuitada_SeBorraDelRepertorio()
    {
        var skill = GuardarSkill("SIXSEVEN");
        var otra = GuardarSkill("TRALALERO");
        var jugadorId = GuardarJugador("ana@farmeort.com", "anaperez").Id;
        DeclararYGuardar(jugadorId, skill.Id, 4);
        DeclararYGuardar(jugadorId, otra.Id, 2);
        var jugador = CargarConRepertorio(jugadorId);
        jugador.QuitarSkill(skill.Id);

        _repositorio.GuardarRepertorio(jugador);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var restantes = _baseDeDatos.Contexto.Set<SkillDeclarada>().Where(d => d.JugadorId == jugadorId).Select(d => d.SkillId).ToList();
        CollectionAssert.AreEqual(new[] { otra.Id }, restantes);
    }

    [TestMethod]
    public void GuardarRepertorio_SkillDeclaradaPorOtroPedidoDespuesDeCargar_LanzaSkillYaDeclaradaYDejaElContextoUsable()
    {
        var skill = GuardarSkill("SIXSEVEN");
        var otra = GuardarSkill("TRALALERO");
        var jugador = CargarConRepertorio(GuardarJugador("ana@farmeort.com", "anaperez").Id);
        jugador.DeclararSkill(_baseDeDatos.Contexto.Skills.Single(s => s.Id == skill.Id), 4, FechaActual);
        InsertarDeclaracionDeOtroPedido(jugador.Id, skill.Id);

        Assert.ThrowsExactly<SkillYaDeclaradaException>(() => _repositorio.GuardarRepertorio(jugador));
        Assert.AreEqual(1, _baseDeDatos.Contexto.Set<SkillDeclarada>().AsNoTracking().Count(d => d.JugadorId == jugador.Id));
        var recargado = CargarConRepertorioSinSeguimientoPrevio(jugador.Id);
        recargado.DeclararSkill(_baseDeDatos.Contexto.Skills.Single(s => s.Id == otra.Id), 2, FechaActual);
        _repositorio.GuardarRepertorio(recargado);
        Assert.AreEqual(2, _baseDeDatos.Contexto.Set<SkillDeclarada>().AsNoTracking().Count(d => d.JugadorId == jugador.Id));
    }

    // Simula otro pedido que declaró la misma skill entre la carga y el guardado.
    private void InsertarDeclaracionDeOtroPedido(int jugadorId, int skillId)
    {
        _baseDeDatos.Contexto.Database.ExecuteSql(
            $"INSERT INTO SkillsDeclaradas (JugadorId, SkillId, Nivel, FechaDeclaracion) VALUES ({jugadorId}, {skillId}, 3, '2026-10-01')");
    }

    private Usuario CargarConRepertorioSinSeguimientoPrevio(int jugadorId)
    {
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return CargarConRepertorio(jugadorId);
    }

    private Usuario CargarConRepertorio(int jugadorId)
    {
        return _baseDeDatos.Contexto.Usuarios.Include(u => u.Repertorio).Single(u => u.Id == jugadorId);
    }

    // Arma el repertorio directamente con el contexto, sin usar métodos del repositorio (FIRST).
    private void DeclararYGuardar(int jugadorId, int skillId, int nivel)
    {
        var jugador = _baseDeDatos.Contexto.Usuarios.Include(u => u.Repertorio).Single(u => u.Id == jugadorId);
        var skill = _baseDeDatos.Contexto.Skills.Single(s => s.Id == skillId);
        jugador.DeclararSkill(skill, nivel, FechaActual);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
    }

    private Skill GuardarSkill(string codigo)
    {
        var skill = new Skill(codigo, "Six-Seven", Descripcion, CategoriaSkill.Gestual, 2);
        _baseDeDatos.Contexto.Skills.Add(skill);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return skill;
    }

    private Usuario GuardarJugador(string email, string nickname)
    {
        var jugador = new Usuario("Ana", "Pérez", email, nickname, new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(jugador);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return jugador;
    }
}
