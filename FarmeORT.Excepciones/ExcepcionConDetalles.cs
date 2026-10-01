namespace FarmeORT.Excepciones;

// Para las reglas que tienen que indicar qué elementos no cumplen (letra 1.5 y 2.4): el filtro global
// copia la lista al campo "detalles" de la respuesta de error (D-41).
public abstract class ExcepcionConDetalles(string mensaje, IReadOnlyList<string> detalles) : Exception(mensaje)
{
    public IReadOnlyList<string> Detalles { get; } = detalles;
}
