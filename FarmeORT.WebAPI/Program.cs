using FarmeORT.Fabrica;
using FarmeORT.WebAPI;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FarmeORT")
    ?? throw new InvalidOperationException("Falta el connection string 'FarmeORT' en la configuración.");

builder.Services.AgregarControllersConFiltros();
builder.Services.AgregarDependencias(connectionString);

var app = builder.Build();

app.UsarManejoDeErrores();
app.MapControllers();

app.Run();
