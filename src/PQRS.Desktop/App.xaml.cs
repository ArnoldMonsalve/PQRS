using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PQRS.Application.Interfaces;
using PQRS.Application.Services;
using PQRS.Desktop.Services;
using PQRS.Desktop.ViewModels;
using PQRS.Desktop.ViewModels.Admin;
using PQRS.Desktop.Views;
using PQRS.Infrastructure;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Desktop
{
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            // appsettings.json se busca junto al ejecutable, sin importar desde qué carpeta se lance la aplicación.
            _host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureServices((context, services) =>
                {
                    services.AddInfrastructure(context.Configuration);
                    services.AddSingleton<ISesionActual, SesionActual>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<RadicarSolicitudViewModel>();
                    services.AddSingleton<IDialogosArchivos, DialogosArchivos>();
                    services.AddTransient<ReportesViewModel>();
                    services.AddTransient<BandejaSolicitudesViewModel>();
                    services.AddTransient<DetalleSolicitudViewModel>();

                    services.AddTransient<AdministracionViewModel>();
                    services.AddTransient<UsuariosAdminViewModel>();
                    services.AddTransient<TiposAdminViewModel>();
                    services.AddTransient<CategoriasAdminViewModel>();
                    services.AddTransient<AreasAdminViewModel>();
                    services.AddTransient<OrganizacionAdminViewModel>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // El login es una ventana modal y el cierre de la app se controla a mano.
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                await _host.StartAsync();
                await _host.Services.GetRequiredService<DbInitializer>().EjecutarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo iniciar la aplicación:\n\n{ex.GetBaseException().Message}",
                    "PQRS", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            MostrarLogin();
        }

        private void MostrarLogin()
        {
            var login = _host.Services.GetRequiredService<LoginWindow>();
            if (login.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var principal = _host.Services.GetRequiredService<MainWindow>();
            var viewModel = (MainViewModel)principal.DataContext;
            var cerrandoSesion = false;

            viewModel.SesionCerrada += () =>
            {
                cerrandoSesion = true;
                principal.Close();
                MostrarLogin();
            };
            principal.Closed += (_, _) =>
            {
                if (!cerrandoSesion) Shutdown();
            };

            MainWindow = principal;
            principal.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
