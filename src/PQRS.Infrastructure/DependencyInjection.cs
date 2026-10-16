using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PQRS.Application.Interfaces;
using PQRS.Infrastructure.Persistence;
using PQRS.Infrastructure.Security;
using PQRS.Infrastructure.Services;

namespace PQRS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PqrsDb")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'PqrsDb' en appsettings.json.");

        // En escritorio no hay "petición" que delimite un scope: cada servicio/ViewModel recibe su propio
        // DbContext (Transient) para no arrastrar datos desactualizados durante toda la sesión.
        services.AddDbContext<PqrsDbContext>(options => options.UseSqlServer(connectionString),
            ServiceLifetime.Transient, ServiceLifetime.Transient);

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<ISolicitudService, SolicitudService>();
        services.AddTransient<ICatalogoService, CatalogoService>();
        // En un uso con varios equipos esta carpeta debe ser una ruta compartida de red accesible para todos.
        var rutaAdjuntos = configuration["Almacenamiento:RutaAdjuntos"];
        if (string.IsNullOrWhiteSpace(rutaAdjuntos))
            rutaAdjuntos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PQRS", "Adjuntos");
        services.AddSingleton(new AlmacenamientoOptions(rutaAdjuntos));

        services.AddTransient<IAdjuntoService, AdjuntoService>();
        services.AddTransient<IReporteService, ReporteService>();
        services.AddSingleton<IExportadorExcel, ExportadorExcel>();
        services.AddTransient<IUsuarioAdminService, UsuarioAdminService>();
        services.AddTransient<ICatalogoAdminService, CatalogoAdminService>();
        services.AddTransient<DbInitializer>();

        return services;
    }
}
