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

    [TestMethod]
    public void OnException_JugadorSinInscripcion_Devuelve403ConCodigoPropio()
    {
        var contexto = CrearContexto(new JugadorNoInscriptoEnTorneoException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status403Forbidden, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("PLAYER_NOT_IN_TOURNAMENT", error.ErrorCode);
        Assert.AreEqual("No tenés una inscripción en este torneo.", error.UserMessage);
    }

    [TestMethod]
    public void OnException_CredencialesInvalidas_Devuelve401ConErrorInvalidCredentials()
    {
        var contexto = CrearContexto(new CredencialesInvalidasException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_CREDENTIALS", error.ErrorCode);
        Assert.AreEqual("Email o contraseña incorrectos.", error.UserMessage);
    }

    [TestMethod]
    public void OnException_UsuarioNoEncontrado_Devuelve404ConUserNotFound()
    {
        var contexto = CrearContexto(new UsuarioNoEncontradoException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status404NotFound, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("USER_NOT_FOUND", error.ErrorCode);
        Assert.AreEqual("No existe un usuario con ese id.", error.UserMessage);
    }

    [TestMethod]
    public void OnException_SkillNoEncontrada_Devuelve404ConSkillNotFound()
    {
        var contexto = CrearContexto(new SkillNoEncontradaException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status404NotFound, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("SKILL_NOT_FOUND", error.ErrorCode);
        Assert.AreEqual("No existe una skill con ese id.", error.UserMessage);
    }

    [TestMethod]
    [DataRow(typeof(TorneoNoEncontradoException), StatusCodes.Status404NotFound, "TOURNAMENT_NOT_FOUND", "No existe un torneo con ese id.")]
    [DataRow(typeof(CodigoTorneoYaExisteException), StatusCodes.Status409Conflict, "TOURNAMENT_CODE_ALREADY_EXISTS", "Ya existe un torneo con ese código.")]
    [DataRow(typeof(TorneoNoModificableException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_MODIFIABLE", "Solo se puede modificar un torneo Abierto.")]
    [DataRow(typeof(TorneoNoCancelableException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_CANCELLABLE", "Solo se puede cancelar un torneo Abierto o En curso.")]
    [DataRow(typeof(SkillInactivaException), StatusCodes.Status409Conflict, "SKILL_INACTIVE", "No es posible utilizar la skill porque está inactiva.")]
    [DataRow(typeof(SkillYaDeclaradaException), StatusCodes.Status409Conflict, "SKILL_ALREADY_DECLARED", "La skill ya está en tu repertorio.")]
    [DataRow(typeof(SkillNoDeclaradaException), StatusCodes.Status404NotFound, "DECLARED_SKILL_NOT_FOUND", "La skill no está en tu repertorio.")]
    [DataRow(typeof(NoEsOrganizadorDelTorneoException), StatusCodes.Status403Forbidden, "NOT_TOURNAMENT_ORGANIZER", "No sos el organizador asignado a este torneo.")]
    [DataRow(typeof(SkillObligatoriaYaAsociadaException), StatusCodes.Status409Conflict, "MANDATORY_SKILL_ALREADY_ADDED", "La skill ya es obligatoria en este torneo.")]
    [DataRow(typeof(LimiteSkillsObligatoriasException), StatusCodes.Status409Conflict, "MANDATORY_SKILLS_LIMIT_REACHED", "Un torneo puede tener como máximo 2 skills obligatorias.")]
    [DataRow(typeof(SkillObligatoriaNoEncontradaException), StatusCodes.Status404NotFound, "MANDATORY_SKILL_NOT_FOUND", "La skill no es obligatoria en este torneo.")]
    [DataRow(typeof(InscripcionNoEncontradaException), StatusCodes.Status404NotFound, "REGISTRATION_NOT_FOUND", "No existe una inscripción con ese id.")]
    [DataRow(typeof(NoEsDuenoDeLaInscripcionException), StatusCodes.Status403Forbidden, "NOT_REGISTRATION_OWNER", "La inscripción no es tuya.")]
    [DataRow(typeof(TorneoNoAbiertoException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_OPEN", "El torneo no está abierto a inscripciones.")]
    [DataRow(typeof(InscripcionYaExisteException), StatusCodes.Status409Conflict, "ALREADY_REGISTERED", "Ya tenés una inscripción aceptada en este torneo.")]
    [DataRow(typeof(TorneoCompletoException), StatusCodes.Status409Conflict, "TOURNAMENT_FULL", "El torneo ya alcanzó su cupo de inscripciones aceptadas.")]
    [DataRow(typeof(InscripcionNoActivaException), StatusCodes.Status409Conflict, "REGISTRATION_NOT_ACTIVE", "La inscripción ya no está aceptada.")]
    [DataRow(typeof(SkillObligatoriaEnUsoException), StatusCodes.Status409Conflict, "MANDATORY_SKILL_IN_USE", "No podés quitar la skill: es obligatoria en un torneo en el que estás inscripto.")]
    [DataRow(typeof(UsuarioOrganizaTorneoVigenteException), StatusCodes.Status409Conflict, "USER_ORGANIZES_ACTIVE_TOURNAMENT", "El usuario organiza un torneo que no está Finalizado ni Cancelado.")]
    [DataRow(typeof(UsuarioConBatallasSinResolverException), StatusCodes.Status409Conflict, "USER_HAS_UNRESOLVED_BATTLES", "El usuario tiene batallas Pendientes o En arbitraje.")]
    [DataRow(typeof(UsuarioConInscripcionVigenteException), StatusCodes.Status409Conflict, "USER_HAS_ACTIVE_REGISTRATION", "El usuario está inscripto en un torneo que no está Finalizado ni Cancelado.")]
    [DataRow(typeof(BatallaNoFallableException), StatusCodes.Status409Conflict, "BATTLE_NOT_JUDGEABLE", "Solo se puede fallar una batalla Pendiente o En arbitraje.")]
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
        CollectionAssert.AreEqual(new[] { "SIXSEVEN", "TRALALERO" }, error.Detalles!.ToArray());
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
        CollectionAssert.AreEqual(new[] { "anaperez", "brunolopez" }, error.Detalles!.ToArray());
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
    [DataRow(typeof(EmailYaExisteException), "EMAIL_ALREADY_EXISTS", "Ya existe un usuario con ese email.")]
    [DataRow(typeof(NicknameYaExisteException), "NICKNAME_ALREADY_EXISTS", "Ya existe un usuario con ese nickname.")]
    [DataRow(typeof(CodigoSkillYaExisteException), "SKILL_CODE_ALREADY_EXISTS", "Ya existe una skill con ese código.")]
    public void OnException_DatoDuplicado_Devuelve409ConSuCodigo(Type tipoExcepcion, string codigo, string mensaje)
    {
        var excepcion = (Exception)Activator.CreateInstance(tipoExcepcion)!;
        var contexto = CrearContexto(excepcion);

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status409Conflict, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual(codigo, error.ErrorCode);
        Assert.AreEqual(mensaje, error.UserMessage);
    }

    [TestMethod]
    public void OnException_OperacionSobreSiMismo_Devuelve409ConSelfOperationNotAllowed()
    {
        var contexto = CrearContexto(new OperacionSobreSiMismoException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status409Conflict, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("SELF_OPERATION_NOT_ALLOWED", error.ErrorCode);
        Assert.AreEqual("Un usuario no puede modificarse ni eliminarse a sí mismo.", error.UserMessage);
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
