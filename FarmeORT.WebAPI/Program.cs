using FarmeORT.Repositorios;
using FarmeORT.Servicios;
using FarmeORT.WebAPI.Filtros;
using FarmeORT.WebAPI.Validaciones;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FarmeORT")
    ?? throw new InvalidOperationException("Falta el connection string 'FarmeORT' en la configuración.");

builder.Services
    .AddControllers(opciones => opciones.Filters.Add<FiltroExcepciones>())
    .ConfigureApiBehaviorOptions(opciones => opciones.InvalidModelStateResponseFactory = RespuestaValidacion.Crear);
builder.Services
    .AgregarServicios()
    .AgregarRepositorios(connectionString);

var app = builder.Build();

app.MapControllers();

app.Run();
