namespace BodegaApp.Domain;

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

    // Valor del selector que habilita el campo de texto libre
    public const string Other = "__otro__";

    public static bool IsPredefined(string? type) =>
        !string.IsNullOrWhiteSpace(type) && All.Contains(type);
}
