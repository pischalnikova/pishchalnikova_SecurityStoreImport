using SecurityStoreImport.Models;

namespace SecurityStoreImport.Services;

/// <summary>Поиск, фильтр и сортировка списка товаров.</summary>
public static class CatalogFilter
{
    public static List<Product> Apply(IEnumerable<Product> products, string? search, string? system, string? sort)
    {
        var query = products;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase)
                || p.Brand.Contains(text, StringComparison.CurrentCultureIgnoreCase)
                || p.Sku.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(system))
            query = query.Where(p => p.SystemType == system);

        query = sort switch
        {
            "price_up" => query.OrderBy(p => p.Price),
            "price_down" => query.OrderByDescending(p => p.Price),
            "stock_up" => query.OrderBy(p => p.StockQuantity),
            _ => query
        };

        return query.ToList();
    }
}
