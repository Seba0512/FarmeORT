using System.Net.Mail;
using System.Text.RegularExpressions;
using FarmeORT.Dominio.Skills;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Usuarios;

public sealed class Usuario
{
    private const int EloInicial = 1200;
    private const int EloMinimo = 100;
    private const string PatronNickname = "^[a-zA-Z0-9]{3,20}$";
    private const int LargoMaximoNombre = 50;
    private const int LargoMinimoApellido = 3;
    private const int LargoMaximoApellido = 25;
    private const int EdadMinima = 13;

    private readonly List<SkillDeclarada> _repertorio = [];

    public Usuario(
        string nombre,
        string apellido,
        string email,
        string nickname,
        DateOnly fechaNacimiento,
        Rol rol,
        string contrasenaHash,
        DateOnly fechaActual)
    {
        Modificar(nombre, apellido, email, nickname, fechaNacimiento, rol, contrasenaHash, fechaActual);
        Elo = EloInicial;
    }

    // EF Core materializa el usuario con este constructor y completa las propiedades después.
    private Usuario()
    {
    }

    public int Id { get; init; }

    public string Nombre { get; private set; } = string.Empty;

    public string Apellido { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Nickname { get; private set; } = string.Empty;

    public DateOnly FechaNacimiento { get; private set; }

    public Rol Rol { get; private set; }

    public string ContrasenaHash { get; private set; } = string.Empty;

    public int Elo { get; private set; }

    public bool Eliminado { get; private set; }

    public IReadOnlyCollection<SkillDeclarada> Repertorio => _repertorio;

    public void Modificar(
        string nombre,
        string apellido,
        string email,
        string nickname,
        DateOnly fechaNacimiento,
        Rol rol,
        string contrasenaHash,
        DateOnly fechaActual)
    {
        ValidarNombre(nombre);
        ValidarApellido(apellido);
        ValidarEmail(email);
        ValidarNickname(nickname);
        ValidarEdad(fechaNacimiento, fechaActual);

        Nombre = nombre;
        Apellido = apellido;
        Email = email;
        Nickname = nickname;
        FechaNacimiento = fechaNacimiento;
        Rol = rol;
        ContrasenaHash = contrasenaHash;
    }

    public SkillDeclarada DeclararSkill(Skill skill, int nivel, DateOnly fechaActual)
    {
        // Una skill inactiva no entra en repertorios nuevos; las ya declaradas se conservan (no es retroactivo).
        if(!skill.Activa)
        {
            throw new SkillInactivaException();
        }

        if(_repertorio.Any(d => d.SkillId == skill.Id))
        {
            throw new SkillYaDeclaradaException();
        }

        var declarada = new SkillDeclarada(this, skill, nivel, fechaActual);
        _repertorio.Add(declarada);
        return declarada;
    }

    public void QuitarSkill(int skillId)
    {
        var declarada = _repertorio.SingleOrDefault(d => d.SkillId == skillId);
        if(declarada == null)
        {
            throw new SkillNoDeclaradaException();
        }

        _repertorio.Remove(declarada);
    }

    // Las skills pedidas que el jugador no tiene en su repertorio, en el mismo orden en que se pidieron.
    public IReadOnlyList<Skill> SkillsFaltantes(IEnumerable<Skill> requeridas)
    {
        return requeridas
            .Where(skill => !_repertorio.Any(d => d.SkillId == skill.Id))
            .ToList();
    }

    // Baja lógica: se conservan los datos para el historial de batallas y torneos.
    public void Eliminar()
    {
        Eliminado = true;
    }

    public void ActualizarElo(int elo)
    {
        if(elo < EloMinimo)
        {
            throw new DatosInvalidosException("El ELO no puede ser menor que 100.");
        }

        Elo = elo;
    }

    private static void ValidarNombre(string nombre)
    {
        if(string.IsNullOrWhiteSpace(nombre))
        {
            throw new DatosInvalidosException("El nombre es obligatorio.");
        }

        if(nombre.Length > LargoMaximoNombre)
        {
            throw new DatosInvalidosException("El nombre no puede tener más de 50 caracteres.");
        }
    }

    private static void ValidarApellido(string apellido)
    {
        if(apellido.Length < LargoMinimoApellido || apellido.Length > LargoMaximoApellido)
        {
            throw new DatosInvalidosException("El apellido debe tener entre 3 y 25 caracteres.");
        }
    }

    private static void ValidarEmail(string email)
    {
        if(!MailAddress.TryCreate(email, out var direccion) || direccion.Address != email)
        {
            throw new DatosInvalidosException("El email no tiene un formato válido.");
        }
    }

    private static void ValidarNickname(string nickname)
    {
        if(!Regex.IsMatch(nickname, PatronNickname))
        {
            throw new DatosInvalidosException("El nickname debe ser alfanumérico, sin espacios, de 3 a 20 caracteres.");
        }
    }

    private static void ValidarEdad(DateOnly fechaNacimiento, DateOnly fechaActual)
    {
        if(fechaNacimiento > fechaActual)
        {
            throw new DatosInvalidosException("La fecha de nacimiento no puede ser futura.");
        }

        if(fechaNacimiento.AddYears(EdadMinima) > fechaActual)
        {
            throw new DatosInvalidosException("El usuario debe tener 13 años o más.");
        }
    }
}
