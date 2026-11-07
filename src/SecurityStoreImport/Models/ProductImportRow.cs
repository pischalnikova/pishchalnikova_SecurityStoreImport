namespace SecurityStoreImport.Models;

/// <summary>
/// Промежуточная структура (DTO): значения одной строки CSV в виде текста,
/// ещё до проверки и преобразования типов.
/// </summary>
public class ProductImportRow
{
    public int LineNumber { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Price { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string Stock { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string IsActive { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string SystemType { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string IpRating { get; init; } = string.Empty;
}
