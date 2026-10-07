using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.Repositorios.Torneos;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Torneos;

// Skills obligatorias del torneo: se leen y se guardan a través de RepositorioTorneos (son parte del torneo).
[TestClass]
public sealed class RepositorioSkillsObligatoriasTests
{
    private const string Descripcion = "Torneo eliminatorio de batallas de aura.";

    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioTorneos _repositorio = null!;
    private int _organizadorId;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioTorneos(_baseDeDatos.Contexto);
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(organizador);
        _baseDeDatos.Contexto.SaveChanges();
        _organizadorId = organizador.Id;
        _baseDeDatos.Contexto.ChangeTracker.Clear();
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void ObtenerPorId_DosTorneosConLaMismaObligatoria_CadaUnoLaTrae()
    {
        var skillId = GuardarSkill("SIXSEVEN");
        var primero = GuardarTorneo("COPAAURA");
        var segundo = GuardarTorneo("COPAFUEGO");
        AsociarYGuardar(primero, skillId);
        AsociarYGuardar(segundo, skillId);

        var resultadoPrimero = _repositorio.ObtenerPorId(primero);
        var resultadoSegundo = _repositorio.ObtenerPorId(segundo);

        Assert.IsNotNull(resultadoPrimero);
        Assert.IsNotNull(resultadoSegundo);
        Assert.AreEqual("SIXSEVEN", resultadoPrimero.SkillsObligatorias.Single().Codigo);
        Assert.AreEqual("SIXSEVEN", resultadoSegundo.SkillsObligatorias.Single().Codigo);
    }

    [TestMethod]
    public void Actualizar_ObligatoriaAsociada_QuedaGuardada()
    {
        var skillId = GuardarSkill("SIXSEVEN");
        var torneo = CargarConObligatorias(GuardarTorneo("COPAAURA"));
        torneo.AsociarSkillObligatoria(_baseDeDatos.Contexto.Skills.Single(s => s.Id == skillId));

        _repositorio.Actualizar(torneo);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.AreEqual(skillId, CargarConObligatorias(torneo.Id).SkillsObligatorias.Single().Id);
    }

    [TestMethod]
    public void Actualizar_ObligatoriaQuitada_SeBorraSinBorrarLaSkill()
    {
        var skillId = GuardarSkill("SIXSEVEN");
        var otraId = GuardarSkill("TRALALERO");
        var torneoId = GuardarTorneo("COPAAURA");
        AsociarYGuardar(torneoId, skillId);
        AsociarYGuardar(torneoId, otraId);
        var torneo = CargarConObligatorias(torneoId);
        torneo.QuitarSkillObligatoria(skillId);

        _repositorio.Actualizar(torneo);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.AreEqual(otraId, CargarConObligatorias(torneoId).SkillsObligatorias.Single().Id);
        Assert.IsTrue(_baseDeDatos.Contexto.Skills.Any(s => s.Id == skillId));
    }

    [TestMethod]
    public void Listar_FiltroPorSkillObligatoria_DevuelveSoloLosTorneosQueLaExigen()
    {
        var skillId = GuardarSkill("SIXSEVEN");
        var otraId = GuardarSkill("TRALALERO");
        var conLaSkill = GuardarTorneo("COPAAURA");
        var conOtra = GuardarTorneo("COPAFUEGO");
        GuardarTorneo("COPAHIELO");
        AsociarYGuardar(conLaSkill, skillId);
        AsociarYGuardar(conOtra, otraId);

        var torneos = _repositorio.Listar(new FiltroTorneos(null, null, null, null, skillId, false, 20, 0));

        CollectionAssert.AreEqual(new[] { conLaSkill }, torneos.Select(t => t.Id).ToArray());
    }

    private Torneo CargarConObligatorias(int torneoId)
    {
        return _baseDeDatos.Contexto.Torneos
            .Include(t => t.Organizador)
            .Include(t => t.SkillsObligatorias)
            .Single(t => t.Id == torneoId);
    }

    // Arma los datos directamente con el contexto, sin usar métodos del repositorio (FIRST).
    private void AsociarYGuardar(int torneoId, int skillId)
    {
        var torneo = _baseDeDatos.Contexto.Torneos.Include(t => t.SkillsObligatorias).Single(t => t.Id == torneoId);
        torneo.AsociarSkillObligatoria(_baseDeDatos.Contexto.Skills.Single(s => s.Id == skillId));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
    }

    private int GuardarSkill(string codigo)
    {
        var skill = new Skill(codigo, "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2);
        _baseDeDatos.Contexto.Skills.Add(skill);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return skill.Id;
    }

    private int GuardarTorneo(string codigo)
    {
        var organizador = _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == _organizadorId);
        var torneo = new Torneo(codigo, "Copa Aura", Descripcion, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 8, organizador, FechaActual);
        _baseDeDatos.Contexto.Torneos.Add(torneo);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return torneo.Id;
    }
}
