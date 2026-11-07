using SecurityStoreImport.Models;

namespace SecurityStoreImport.Services;

/// <summary>Хранилище товаров.</summary>
public interface IProductRepository
{
    int Count { get; }

    /// <summary>Добавляет товар. Повтор артикула — исключение.</summary>
    void Add(Product product);

    IReadOnlyList<Product> GetAll();

    /// <summary>Товар по артикулу (без учёта регистра) или null.</summary>
    Product? GetBySku(string sku);

    bool Exists(string sku);

    void Clear();
}
