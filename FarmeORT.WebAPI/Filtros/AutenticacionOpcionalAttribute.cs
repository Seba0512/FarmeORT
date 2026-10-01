namespace FarmeORT.WebAPI.Filtros;

internal sealed class AutenticacionOpcionalAttribute : AutenticacionAttribute
{
    protected override bool PermiteAnonimo => true;
}
