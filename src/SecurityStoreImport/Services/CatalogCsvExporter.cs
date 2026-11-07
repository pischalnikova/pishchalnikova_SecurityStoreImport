using System.Globalization;
using System.Text;
using SecurityStoreImport.Models;
using SecurityStoreImport.Services.Csv;

namespace SecurityStoreImport.Services;

/// <summary>
/// Выгрузка товаров в CSV через точку с запятой в UTF-8 с BOM,
/// чтобы файл правильно открывался в Excel. Файл можно снова импортировать.
/// </summary>
public static class CatalogCsvExporter
{
    private const char Delimiter = ';';

    public static byte[] Export(IEnumerable<Product> products)
    {
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(Delimiter, CsvColumns.All));

        foreach (var p in products)
        {
            var values = new[]
            {
                p.Sku, p.Name, p.Description, p.Price.ToString(CultureInfo.InvariantCulture),
                p.Currency, p.CategoryName, p.StockQuantity.ToString(), p.ImageUrl,
                p.IsActive ? "true" : "false", p.Brand, p.SystemType, p.Model, p.IpRating
            };
            csv.AppendLine(string.Join(Delimiter, values.Select(v => CsvReader.Escape(v, Delimiter))));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }
}
