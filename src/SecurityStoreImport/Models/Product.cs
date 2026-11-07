namespace SecurityStoreImport.Models;

/// <summary>
/// Товар магазина оборудования для систем безопасности.
/// Последние четыре поля относятся к варианту «Оборудование систем безопасности».
/// </summary>
public class Product
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "RUB";
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>Производитель (Hikvision, Болид, PERCo и т. д.).</summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>Тип системы: Видеонаблюдение, Охранная сигнализация или СКУД.</summary>
    public string SystemType { get; set; } = string.Empty;

    /// <summary>Модель устройства, может быть пустой.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Степень защиты корпуса (например, IP67), может быть пустой.</summary>
    public string IpRating { get; set; } = string.Empty;
}
