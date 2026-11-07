namespace SecurityStoreImport.Models;

/// <summary>Данные для страницы списка товаров.</summary>
public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public int TotalInCatalog { get; set; }

    public string? Search { get; set; }
    public string? System { get; set; }
    public string? Sort { get; set; }

    public decimal TotalValue => Products.Sum(p => p.Price * p.StockQuantity);
}

/// <summary>Данные для страницы импорта.</summary>
public class ImportPageModel
{
    public string FilePath { get; set; } = string.Empty;
    public ImportResult? Result { get; set; }
}
