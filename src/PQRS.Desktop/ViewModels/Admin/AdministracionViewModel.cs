namespace PQRS.Desktop.ViewModels.Admin;

/// <summary>Agrupa las pantallas de administración (una pestaña por ViewModel).</summary>
public class AdministracionViewModel
{
    public AdministracionViewModel(
        UsuariosAdminViewModel usuarios,
        TiposAdminViewModel tipos,
        CategoriasAdminViewModel categorias,
        AreasAdminViewModel areas,
        OrganizacionAdminViewModel organizacion)
    {
        Usuarios = usuarios;
        Tipos = tipos;
        Categorias = categorias;
        Areas = areas;
        Organizacion = organizacion;
    }

    public UsuariosAdminViewModel Usuarios { get; }
    public TiposAdminViewModel Tipos { get; }
    public CategoriasAdminViewModel Categorias { get; }
    public AreasAdminViewModel Areas { get; }
    public OrganizacionAdminViewModel Organizacion { get; }
}
