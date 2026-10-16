using Microsoft.EntityFrameworkCore;
using PQRS.Domain.Entities;

namespace PQRS.Infrastructure.Persistence;

public class PqrsDbContext : DbContext
{
    public PqrsDbContext(DbContextOptions<PqrsDbContext> options) : base(options) { }

    public DbSet<Area> Areas => Set<Area>();
    public DbSet<TipoSolicitud> TiposSolicitud => Set<TipoSolicitud>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Solicitante> Solicitantes => Set<Solicitante>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<Seguimiento> Seguimientos => Set<Seguimiento>();
    public DbSet<Respuesta> Respuestas => Set<Respuesta>();
    public DbSet<Adjunto> Adjuntos => Set<Adjunto>();
    public DbSet<Parametro> Parametros => Set<Parametro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Las tablas del script SQL usan el nombre de la clase en singular.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);

        // Propiedades calculadas: no existen en la base de datos.
        modelBuilder.Entity<Solicitud>().Ignore(s => s.EstaVencida);
        modelBuilder.Entity<Solicitante>().Ignore(s => s.NombreCompleto);

        // Sin borrado en cascada: el historial de PQRS no debe perderse.
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        modelBuilder.Entity<Solicitud>().HasIndex(s => s.Radicado).IsUnique();
        modelBuilder.Entity<Usuario>().HasIndex(u => u.NombreUsuario).IsUnique();
        modelBuilder.Entity<Parametro>().HasIndex(p => p.Clave).IsUnique();
    }
}
