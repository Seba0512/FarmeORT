using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Rondas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.DTOs.Rondas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Rondas;

namespace FarmeORT.Servicios.Rondas;

internal sealed class ServicioRondas(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioBatallas repositorioBatallas,
    IEstrategiaEmparejamiento estrategiaEmparejamiento,
    ISorteo sorteo) : IServicioRondas
{
    public RondaResponse Generar(int torneoId, int usuarioId, string rol)
    {
        var torneo = ObtenerTorneoDelOrganizador(torneoId, usuarioId, rol);
        var jugadores = repositorioInscripciones.ListarAceptadasDelTorneo(torneoId).Select(i => i.Jugador).ToList();
        var numero = PrepararRonda(torneo, jugadores.Count);
        var batallas = CrearBatallas(torneo, numero, estrategiaEmparejamiento.Emparejar(jugadores));

        // Un solo guardado: las batallas nuevas y el cambio de estado del torneo quedan juntos o no queda nada.
        repositorioBatallas.GuardarRonda(torneo, batallas);
        return RespuestasRondas.Crear(numero, batallas);
    }

    // Una ronda son las batallas del torneo con el mismo número; el repositorio ya las trae ordenadas (D-62).
    public IReadOnlyList<RondaResponse> Listar(int torneoId, bool esAnonimo)
    {
        ValidarTorneoVisible(torneoId, esAnonimo);
        return RespuestasRondas.Agrupar(repositorioBatallas.ListarDelTorneo(torneoId));
    }

    public RondaResponse ObtenerPorNumero(int torneoId, int numero, bool esAnonimo)
    {
        var ronda = Listar(torneoId, esAnonimo).SingleOrDefault(r => r.Numero == numero);
        if(ronda == null)
        {
            throw new RondaNoEncontradaException();
        }

        return ronda;
    }

    private Torneo ObtenerTorneoExistente(int torneoId)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        return torneo;
    }

    // El filtro solo sabe el rol: si un Organizador es el asignado a este torneo lo decide el servicio (D-40).
    // Un Administrador puede generar rondas aunque no lo sea (D-63).
    private Torneo ObtenerTorneoDelOrganizador(int torneoId, int usuarioId, string rol)
    {
        var torneo = ObtenerTorneoExistente(torneoId);
        if(rol != Roles.Administrador && torneo.OrganizadorId != usuarioId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        return torneo;
    }

    // Para el anónimo un torneo Cancelado no existe (D-36).
    private void ValidarTorneoVisible(int torneoId, bool esAnonimo)
    {
        var torneo = ObtenerTorneoExistente(torneoId);
        if(esAnonimo && !torneo.EsVisibleParaAnonimos)
        {
            throw new TorneoNoEncontradoException();
        }
    }

    // La primera ronda inicia el torneo. Las siguientes exigen que la anterior esté resuelta y que queden al menos dos
    // jugadores sin eliminar: con uno solo ya hay campeón y corresponde finalizar el torneo.
    private int PrepararRonda(Torneo torneo, int jugadoresQueSiguen)
    {
        if(torneo.Estado != EstadoTorneo.EnCurso)
        {
            torneo.Iniciar(jugadoresQueSiguen);
            return 1;
        }

        // Sin batallas sería la primera ronda, que la letra exige con el torneo Abierto y el cupo completo.
        var anteriores = repositorioBatallas.ListarDelTorneo(torneo.Id);
        if(anteriores.Count == 0)
        {
            throw new TorneoNoAbiertoException();
        }

        var ultima = anteriores.Max(b => b.NumeroRonda);

        if(anteriores.Any(b => b.NumeroRonda == ultima && !EstaResuelta(b)))
        {
            throw new RondaSinResolverException();
        }

        if(jugadoresQueSiguen < 2)
        {
            throw new TorneoListoParaFinalizarException();
        }

        return ultima + 1;
    }

    // Una batalla está resuelta si ya se falló o si se canceló con el torneo.
    private static bool EstaResuelta(Batalla batalla)
    {
        return batalla.Estado == EstadoBatalla.Finalizada || batalla.Estado == EstadoBatalla.Cancelada;
    }

    // Crea una batalla Pendiente por par; se guardan después, todas juntas con el torneo.
    private List<Batalla> CrearBatallas(Torneo torneo, int numeroRonda, IReadOnlyList<ParDeJugadores> pares)
    {
        List<Batalla> batallas = [];
        foreach(var par in pares)
        {
            batallas.Add(new Batalla(torneo, numeroRonda, par.Jugador1, par.Jugador2, sorteo));
        }

        return batallas;
    }
}
