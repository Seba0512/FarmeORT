using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

internal sealed class FiltroExcepciones : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        throw new NotImplementedException();
    }
}
