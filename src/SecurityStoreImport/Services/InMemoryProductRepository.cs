using SecurityStoreImport.Models;

namespace SecurityStoreImport.Services;

/// <summary>
/// Хранение товаров в памяти приложения.
/// Словарь по артикулу нужен для быстрой проверки дубликатов,
/// список — чтобы товары выводились в том порядке, в котором загружены.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<string, Product> _bySku = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Product> _products = new();

    public int Count => _products.Count;

    public void Add(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (_bySku.ContainsKey(product.Sku))
            throw new InvalidOperationException("Товар с артикулом " + product.Sku + " уже есть");

        _bySku.Add(product.Sku, product);
        _products.Add(product);
    }

    public IReadOnlyList<Product> GetAll()
    {
        return _products;
    }

    public Product? GetBySku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        _bySku.TryGetValue(sku.Trim(), out var product);
        return product;
    }

    public bool Exists(string sku)
    {
        return GetBySku(sku) != null;
    }

    public void Clear()
    {
        _bySku.Clear();
        _products.Clear();
    }
}
