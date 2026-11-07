global using Xunit;
global using SecurityStoreImport.Models;
global using SecurityStoreImport.Services;

namespace SecurityStoreImport.Tests;

internal static class TestHelpers
{
    public const string Header =
        "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,system_type,model,ip_rating";

    public const string GoodRow =
        "GRD-1001,IP-камера уличная 4 Мп,Камера,8490,RUB,IP-камеры,18,/images/cam.jpg,true,Hikvision,Видеонаблюдение,DS-2CD2043G2-I,IP67";

    public static (ProductImportService Service, InMemoryProductRepository Repository) CreateService()
    {
        var repository = new InMemoryProductRepository();
        var service = new ProductImportService(repository, new ProductRowValidator());
        return (service, repository);
    }

    public static string DataFile(string name) => Path.Combine(AppContext.BaseDirectory, "data", name);

    public static Product MakeProduct(string sku) => new()
    {
        Sku = sku,
        Name = "Товар " + sku,
        Price = 100,
        Brand = "Тест",
        SystemType = "Видеонаблюдение"
    };

    /// <summary>Строка CSV на основе GoodRow, в которой заменено одно поле.</summary>
    public static string RowWith(int column, string value)
    {
        var fields = GoodRow.Split(',');
        fields[column] = value;
        return string.Join(',', fields);
    }
}
