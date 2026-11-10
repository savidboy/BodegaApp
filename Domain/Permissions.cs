namespace BodegaApp.Domain;

/// <summary>
/// Contrato único de códigos de permiso. No se escribe un código a mano en ningún
/// controlador: se usa la constante, así el seeder y las rutas no se desincronizan.
/// Un string distinto por una letra no da error: simplemente nadie tiene el permiso
/// y la ruta responde 403 sin explicación.
/// </summary>
public static class Permissions
{
    // Inicio
    public const string HomeShow = "home.show";

    // Proveedores
    public const string SuppliersShow = "suppliers.show";
    public const string SuppliersCreate = "suppliers.create";
    public const string SuppliersUpdate = "suppliers.update";
    public const string SuppliersDelete = "suppliers.delete";

    // Usuarios
    public const string UsersShow = "users.show";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";

    // Roles: no hay roles.delete a propósito. La rúbrica pide crear, consultar y
    // editar. Borrar un rol deja usuarios huérfanos, así que se le quitan permisos.
    public const string RolesShow = "roles.show";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";

    // Productos: declarados para el módulo que viene, todavía sin controlador
    public const string ProductsShow = "products.show";
    public const string ProductsCreate = "products.create";
    public const string ProductsUpdate = "products.update";
    public const string ProductsDelete = "products.delete";

    /// <summary>
    /// Catálogo completo. El seeder crea una fila por cada elemento de esta lista.
    /// </summary>
    public static readonly PermissionSeed[] All =
    [
        new(HomeShow, "Acceder al inicio", "Inicio"),

        new(SuppliersShow, "Ver proveedores", "Proveedores"),
        new(SuppliersCreate, "Crear proveedores", "Proveedores"),
        new(SuppliersUpdate, "Editar proveedores", "Proveedores"),
        new(SuppliersDelete, "Desactivar y eliminar proveedores", "Proveedores"),

        new(UsersShow, "Ver usuarios", "Usuarios"),
        new(UsersCreate, "Crear usuarios", "Usuarios"),
        new(UsersUpdate, "Editar usuarios y asignar roles", "Usuarios"),
        new(UsersDelete, "Desactivar usuarios", "Usuarios"),

        new(RolesShow, "Ver roles y sus permisos", "Roles"),
        new(RolesCreate, "Crear roles con sus permisos", "Roles"),
        new(RolesUpdate, "Editar los permisos de un rol", "Roles"),

        new(ProductsShow, "Ver productos", "Productos"),
        new(ProductsCreate, "Crear productos", "Productos"),
        new(ProductsUpdate, "Editar productos", "Productos"),
        new(ProductsDelete, "Desactivar y eliminar productos", "Productos")
    ];

    /// <summary>Módulos distintos, para agrupar la matriz de permisos por módulo.</summary>
    public static IEnumerable<string> Modules =>
        All.Select(permission => permission.Module).Distinct();

    public static IEnumerable<PermissionSeed> ByModule(string module) =>
        All.Where(permission => permission.Module == module);
}

public sealed record PermissionSeed(string Code, string Description, string Module);
