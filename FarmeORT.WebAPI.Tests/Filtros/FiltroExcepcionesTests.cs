using FarmeORT.DTOs.Errores;
using FarmeORT.Excepciones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace FarmeORT.WebAPI.Tests.Filtros;

[TestClass]
public sealed class FiltroExcepcionesTests
{
    private FiltroExcepciones _filtro = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _filtro = new FiltroExcepciones();
    }

    // Una fila por entrada del diccionario del filtro, en el mismo orden y agrupadas por tema. El DataRow no puede
    // guardar una excepción ya creada: recibe su tipo y el test la crea con Activator.
    [TestMethod]
    [DataRow(typeof(CredencialesInvalidasException), StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "Email o contraseña incorrectos.")]
    [DataRow(typeof(UsuarioNoEncontradoException), StatusCodes.Status404NotFound, "USER_NOT_FOUND", "No existe un usuario con ese id.")]
    [DataRow(typeof(SesionNoEncontradaException), StatusCodes.Status404NotFound, "SESSION_NOT_FOUND", "No existe una sesión con ese token.")]
    [DataRow(typeof(NoEsDuenoDeLaSesionException), StatusCodes.Status403Forbidden, "NOT_SESSION_OWNER", "La sesión no es tuya.")]
    [DataRow(typeof(SkillNoEncontradaException), StatusCodes.Status404NotFound, "SKILL_NOT_FOUND", "No existe una skill con ese id.")]
    [DataRow(typeof(SkillNoDeclaradaException), StatusCodes.Status404NotFound, "DECLARED_SKILL_NOT_FOUND", "La skill no está en tu repertorio.")]
    [DataRow(typeof(TorneoNoEncontradoException), StatusCodes.Status404NotFound, "TOURNAMENT_NOT_FOUND", "No existe un torneo con ese id.")]
    [DataRow(typeof(EmailYaExisteException), StatusCodes.Status409Conflict, "EMAIL_ALREADY_EXISTS", "Ya existe un usuario con ese email.")]
    [DataRow(typeof(NicknameYaExisteException), StatusCodes.Status409Conflict, "NICKNAME_ALREADY_EXISTS", "Ya existe un usuario con ese nickname.")]
    [DataRow(typeof(CodigoSkillYaExisteException), StatusCodes.Status409Conflict, "SKILL_CODE_ALREADY_EXISTS", "Ya existe una skill con ese código.")]
    [DataRow(typeof(SkillInactivaException), StatusCodes.Status409Conflict, "SKILL_INACTIVE", "No es posible utilizar la skill porque está inactiva.")]
    [DataRow(typeof(SkillYaDeclaradaException), StatusCodes.Status409Conflict, "SKILL_ALREADY_DECLARED", "La skill ya está en tu repertorio.")]
    [DataRow(typeof(CodigoTorneoYaExisteException), StatusCodes.Status409Conflict, "TOURNAMENT_CODE_ALREADY_EXISTS", "Ya existe un torneo con ese código.")]
    [DataRow(typeof(TorneoNoModificableException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_MODIFIABLE", "Solo se puede modificar un torneo Abierto.")]
    [DataRow(typeof(TorneoNoCancelableException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_CANCELLABLE", "Solo se puede cancelar un torneo Abierto o En curso.")]
    [DataRow(typeof(OperacionSobreSiMismoException), StatusCodes.Status409Conflict, "SELF_OPERATION_NOT_ALLOWED", "Un usuario no puede modificarse ni eliminarse a sí mismo.")]
    [DataRow(typeof(NoEsOrganizadorDelTorneoException), StatusCodes.Status403Forbidden, "NOT_TOURNAMENT_ORGANIZER", "No sos el organizador asignado a este torneo.")]
    [DataRow(typeof(SkillObligatoriaYaAsociadaException), StatusCodes.Status409Conflict, "MANDATORY_SKILL_ALREADY_ADDED", "La skill ya es obligatoria en este torneo.")]
    [DataRow(typeof(LimiteSkillsObligatoriasException), StatusCodes.Status409Conflict, "MANDATORY_SKILLS_LIMIT_REACHED", "Un torneo puede tener como máximo 2 skills obligatorias.")]
    [DataRow(typeof(SkillObligatoriaNoEncontradaException), StatusCodes.Status404NotFound, "MANDATORY_SKILL_NOT_FOUND", "La skill no es obligatoria en este torneo.")]
    [DataRow(typeof(InscripcionNoEncontradaException), StatusCodes.Status404NotFound, "REGISTRATION_NOT_FOUND", "No existe una inscripción con ese id.")]
    [DataRow(typeof(NoEsDuenoDeLaInscripcionException), StatusCodes.Status403Forbidden, "NOT_REGISTRATION_OWNER", "La inscripción no es tuya.")]
    [DataRow(typeof(TorneoNoAbiertoException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_OPEN", "El torneo no está abierto.")]
    [DataRow(typeof(InscripcionYaExisteException), StatusCodes.Status409Conflict, "ALREADY_REGISTERED", "Ya tenés una inscripción aceptada en este torneo.")]
    [DataRow(typeof(TorneoCompletoException), StatusCodes.Status409Conflict, "TOURNAMENT_FULL", "El torneo ya alcanzó su cupo de inscripciones aceptadas.")]
    [DataRow(typeof(InscripcionNoActivaException), StatusCodes.Status409Conflict, "REGISTRATION_NOT_ACTIVE", "La inscripción ya no está aceptada.")]
    [DataRow(typeof(SkillObligatoriaEnUsoException), StatusCodes.Status409Conflict, "MANDATORY_SKILL_IN_USE", "No podés quitar la skill: es obligatoria en un torneo en el que estás inscripto.")]
    [DataRow(typeof(BatallaNoEncontradaException), StatusCodes.Status404NotFound, "BATTLE_NOT_FOUND", "No existe una batalla con ese id.")]
    [DataRow(typeof(BatallaNoCancelableException), StatusCodes.Status409Conflict, "BATTLE_NOT_CANCELLABLE", "Solo se puede cancelar una batalla Pendiente o En arbitraje.")]
    [DataRow(typeof(BatallaNoPendienteException), StatusCodes.Status409Conflict, "BATTLE_NOT_PENDING", "Solo se puede lanzar una skill en una batalla Pendiente.")]
    [DataRow(typeof(JugadorNoParticipaEnBatallaException), StatusCodes.Status409Conflict, "PLAYER_NOT_IN_BATTLE", "El jugador no participa de esta batalla.")]
    [DataRow(typeof(TurnoIncorrectoException), StatusCodes.Status409Conflict, "NOT_PLAYER_TURN", "No es tu turno para lanzar una skill.")]
    [DataRow(typeof(SkillFueraDelRepertorioException), StatusCodes.Status409Conflict, "SKILL_NOT_IN_REPERTOIRE", "La skill no está en tu repertorio.")]
    [DataRow(typeof(SkillYaLanzadaException), StatusCodes.Status409Conflict, "SKILL_ALREADY_THROWN", "Ya lanzaste esta skill en la batalla.")]
    [DataRow(typeof(BatallaModificadaException), StatusCodes.Status409Conflict, "BATTLE_CHANGED", "La batalla cambió mientras se procesaba el pedido. Volvé a consultarla.")]
    [DataRow(typeof(SkillObligatoriaPendienteException), StatusCodes.Status409Conflict, "MANDATORY_SKILL_PENDING", "Debés lanzar una skill obligatoria pendiente para completar tus cuatro turnos.")]
    [DataRow(typeof(UsuarioOrganizaTorneoVigenteException), StatusCodes.Status409Conflict, "USER_ORGANIZES_ACTIVE_TOURNAMENT", "El usuario organiza un torneo que no está Finalizado ni Cancelado.")]
    [DataRow(typeof(UsuarioConBatallasSinResolverException), StatusCodes.Status409Conflict, "USER_HAS_UNRESOLVED_BATTLES", "El usuario tiene batallas Pendientes o En arbitraje.")]
    [DataRow(typeof(UsuarioConInscripcionVigenteException), StatusCodes.Status409Conflict, "USER_HAS_ACTIVE_REGISTRATION", "El usuario está inscripto en un torneo que no está Finalizado ni Cancelado.")]
    [DataRow(typeof(BatallaNoFallableException), StatusCodes.Status409Conflict, "BATTLE_NOT_JUDGEABLE", "Solo se puede fallar una batalla Pendiente o En arbitraje.")]
    [DataRow(typeof(JugadorNoInscriptoEnTorneoException), StatusCodes.Status403Forbidden, "PLAYER_NOT_IN_TOURNAMENT", "No tenés una inscripción en este torneo.")]
    [DataRow(typeof(TorneoModificadoException), StatusCodes.Status409Conflict, "TOURNAMENT_CHANGED", "El torneo cambió mientras se procesaba el pedido. Volvé a consultarlo.")]
    [DataRow(typeof(TorneoIncompletoException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_FULL", "El torneo todavía no completó su cupo de inscripciones aceptadas.")]
    [DataRow(typeof(RondaNoEncontradaException), StatusCodes.Status404NotFound, "ROUND_NOT_FOUND", "El torneo no tiene una ronda con ese número.")]
    [DataRow(typeof(RondaSinResolverException), StatusCodes.Status409Conflict, "ROUND_NOT_RESOLVED", "La ronda anterior todavía tiene batallas sin resolver.")]
    [DataRow(typeof(TorneoListoParaFinalizarException), StatusCodes.Status409Conflict, "TOURNAMENT_READY_TO_FINISH", "Queda un solo jugador sin eliminar: corresponde finalizar el torneo.")]
    [DataRow(typeof(TorneoNoEnCursoException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_IN_PROGRESS", "El torneo no está en curso.")]
    [DataRow(typeof(TorneoConBatallasSinResolverException), StatusCodes.Status409Conflict, "TOURNAMENT_HAS_UNRESOLVED_BATTLES", "El torneo todavía tiene batallas sin resolver.")]
    [DataRow(typeof(TorneoSinCampeonException), StatusCodes.Status409Conflict, "TOURNAMENT_WITHOUT_CHAMPION", "Para finalizar el torneo tiene que quedar un solo jugador sin eliminar.")]
    public void OnException_ErrorMapeado_DevuelveSuStatusYCodigo(Type tipo, int status, string codigo, string mensaje)
    {
        var contexto = CrearContexto((Exception)Activator.CreateInstance(tipo)!);

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(status, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual(codigo, error.ErrorCode);
        Assert.AreEqual(mensaje, error.UserMessage);
    }

    [TestMethod]
    public void OnException_DatosInvalidos_Devuelve400ConElMensajeDeLaRegla()
    {
        var contexto = CrearContexto(new DatosInvalidosException("El usuario debe tener 13 años o más."));

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status400BadRequest, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_DATA", error.ErrorCode);
        Assert.AreEqual("El usuario debe tener 13 años o más.", error.UserMessage);
    }

    [TestMethod]
    public void OnException_SkillsObligatoriasFaltantes_Devuelve409ConLasSkillsEnDetalles()
    {
        var contexto = CrearContexto(new SkillsObligatoriasFaltantesException(["SIXSEVEN", "TRALALERO"]));

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status409Conflict, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("MISSING_MANDATORY_SKILLS", error.ErrorCode);
        Assert.AreEqual("No tenés declaradas todas las skills obligatorias del torneo.", error.UserMessage);
        Assert.IsNotNull(error.Detalles);
        CollectionAssert.AreEqual(new[] { "SIXSEVEN", "TRALALERO" }, error.Detalles.ToArray());
    }

    [TestMethod]
    public void OnException_JugadoresSinSkillObligatoria_Devuelve409ConLosNicknamesEnDetalles()
    {
        var contexto = CrearContexto(new JugadoresSinSkillObligatoriaException(["anaperez", "brunolopez"]));

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status409Conflict, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("PLAYERS_MISSING_MANDATORY_SKILL", error.ErrorCode);
        Assert.AreEqual("Hay jugadores inscriptos que no tienen declarada la skill.", error.UserMessage);
        Assert.IsNotNull(error.Detalles);
        CollectionAssert.AreEqual(new[] { "anaperez", "brunolopez" }, error.Detalles.ToArray());
    }

    [TestMethod]
    public void OnException_ExcepcionNoMapeada_Devuelve500ConMensajeFijo()
    {
        var contexto = CrearContexto(new InvalidOperationException("detalle interno con datos sensibles"));

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INTERNAL_ERROR", error.ErrorCode);
        Assert.AreEqual("Ocurrió un error inesperado.", error.UserMessage);
        Assert.IsNull(error.MoreInfo);
        Assert.IsNull(error.Detalles);
    }

    private static ExceptionContext CrearContexto(Exception excepcion)
    {
        var contextoAccion = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        return new ExceptionContext(contextoAccion, []) { Exception = excepcion };
    }
}
