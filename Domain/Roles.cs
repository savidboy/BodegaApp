namespace BodegaApp.Domain;

// Admin es superusuario y no pasa por la tabla de permisos, para no quedarse
// sin forma de administrar si se quita un permiso por error.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Warehouse = "Almacen";

    public static readonly string[] All = [Admin, Warehouse];
}
