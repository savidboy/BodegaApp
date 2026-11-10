namespace BodegaApp.Domain;

// Códigos de permiso. Se usan como constante para que el seeder y los
// controladores no se desincronicen: un string distinto da 403 sin avisar.
public static class Permissions
{
    public const string HomeShow = "home.show";

    public const string SuppliersShow = "suppliers.show";
    public const string SuppliersCreate = "suppliers.create";
    public const string SuppliersUpdate = "suppliers.update";
    public const string SuppliersDelete = "suppliers.delete";

    public const string UsersShow = "users.show";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";

    // Sin roles.delete: borrar un rol deja usuarios huérfanos, se le quitan permisos
    public const string RolesShow = "roles.show";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";

    public const string ProductsShow = "products.show";
    public const string ProductsCreate = "products.create";
    public const string ProductsUpdate = "products.update";
    public const string ProductsDelete = "products.delete";

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

    public static IEnumerable<string> Modules =>
        All.Select(permission => permission.Module).Distinct();

    public static IEnumerable<PermissionSeed> ByModule(string module) =>
        All.Where(permission => permission.Module == module);
}

public sealed record PermissionSeed(string Code, string Description, string Module);
