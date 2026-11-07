namespace SecurityStoreImport.Models;

/// <summary>Названия колонок CSV-файла.</summary>
public static class CsvColumns
{
    public const string Sku = "sku";
    public const string Name = "name";
    public const string Description = "description";
    public const string Price = "price";
    public const string Currency = "currency";
    public const string Category = "category_name";
    public const string Stock = "stock";
    public const string ImageUrl = "image_url";
    public const string IsActive = "is_active";
    public const string Brand = "brand";
    public const string SystemType = "system_type";
    public const string Model = "model";
    public const string IpRating = "ip_rating";

    /// <summary>Все колонки в том порядке, в котором они идут в файле и при экспорте.</summary>
    public static readonly string[] All =
    {
        Sku, Name, Description, Price, Currency, Category, Stock,
        ImageUrl, IsActive, Brand, SystemType, Model, IpRating
    };
}
