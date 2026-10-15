namespace BodegaApp.Domain;

/// <summary>
/// Tipos de proveedor que se ofrecen en los formularios. Si el proveedor no encaja
/// en ninguno, el usuario puede seleccionar <see cref="Other"/> y escribirlo a mano.
/// </summary>
public static class SupplierTypes
{
    public static readonly string[] All =
    {
        "Uva",
        "Insumos",
        "Barricas",
        "Botellas",
        "Corchos",
        "Etiquetas"
    };

    /// <summary>Valor del selector que habilita el campo de texto libre.</summary>
    public const string Other = "__otro__";

    public static bool IsPredefined(string? type) =>
        !string.IsNullOrWhiteSpace(type) && All.Contains(type);
}
