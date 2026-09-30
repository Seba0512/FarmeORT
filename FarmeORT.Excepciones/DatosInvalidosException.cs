namespace FarmeORT.Excepciones;

public sealed class DatosInvalidosException(string mensaje) : Exception(mensaje)
{
}
