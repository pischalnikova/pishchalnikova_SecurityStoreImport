using System.Globalization;
using SecurityStoreImport.Models;

namespace SecurityStoreImport.Services;

/// <summary>
/// Проверка одной строки CSV и создание товара.
/// Проверяются все поля сразу, чтобы пользователь увидел все ошибки строки.
/// </summary>
public class ProductRowValidator
{
    /// <summary>
    /// Возвращает товар, если ошибок нет. Иначе возвращает null,
    /// а ошибки (колонка, текст) добавляются в список errors.
    /// </summary>
    public Product? ValidateRow(ProductImportRow row, List<(string Column, string Message)> errors)
    {
        var errorsBefore = errors.Count;
        var product = new Product();

        product.Sku = row.Sku.Trim();
        if (product.Sku == "")
            errors.Add((CsvColumns.Sku, ValidationMessages.SkuEmpty));

        product.Name = row.Name.Trim();
        if (product.Name == "")
            errors.Add((CsvColumns.Name, ValidationMessages.NameEmpty));

        // Цена: допускаем точку и запятую, например 1290.50 и 1290,50
        var priceText = row.Price.Trim().Replace(',', '.');
        if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            errors.Add((CsvColumns.Price, ValidationMessages.PriceInvalid));
        else if (price <= 0)
            errors.Add((CsvColumns.Price, ValidationMessages.PriceNotPositive));
        else
            product.Price = price;

        if (!int.TryParse(row.Stock.Trim(), out var stock))
            errors.Add((CsvColumns.Stock, ValidationMessages.StockInvalid));
        else if (stock < 0)
            errors.Add((CsvColumns.Stock, ValidationMessages.StockNegative));
        else
            product.StockQuantity = stock;

        switch (row.IsActive.Trim().ToLower())
        {
            case "true":
            case "1":
                product.IsActive = true;
                break;
            case "false":
            case "0":
                product.IsActive = false;
                break;
            default:
                errors.Add((CsvColumns.IsActive, ValidationMessages.IsActiveInvalid));
                break;
        }

        product.Currency = row.Currency.Trim().ToUpper();
        if (product.Currency == "")
            product.Currency = ImportRules.DefaultCurrency;
        else if (!IsCurrencyCode(product.Currency))
            errors.Add((CsvColumns.Currency, ValidationMessages.CurrencyInvalid));

        // Поля варианта «Оборудование систем безопасности»
        product.Brand = row.Brand.Trim();
        if (product.Brand == "")
            errors.Add((CsvColumns.Brand, ValidationMessages.BrandEmpty));

        product.SystemType = FindSystemType(row.SystemType);
        if (product.SystemType == "")
            errors.Add((CsvColumns.SystemType, ValidationMessages.SystemTypeInvalid));

        product.IpRating = row.IpRating.Trim().ToUpper();
        if (product.IpRating != "" && !IsIpRating(product.IpRating))
            errors.Add((CsvColumns.IpRating, ValidationMessages.IpRatingInvalid));

        // Необязательные поля
        product.Description = row.Description.Trim();
        product.ImageUrl = row.ImageUrl.Trim();
        product.Model = row.Model.Trim();
        product.CategoryName = row.CategoryName.Trim() == "" ? ImportRules.DefaultCategory : row.CategoryName.Trim();

        return errors.Count == errorsBefore ? product : null;
    }

    /// <summary>Ищет тип системы среди допустимых без учёта регистра. Пустая строка — не найден.</summary>
    private static string FindSystemType(string value)
    {
        foreach (var type in ImportRules.SystemTypes)
        {
            if (string.Equals(type, value.Trim(), StringComparison.CurrentCultureIgnoreCase))
                return type;
        }
        return "";
    }

    private static bool IsCurrencyCode(string value)
    {
        return value.Length == ImportRules.CurrencyLength && value.All(c => c >= 'A' && c <= 'Z');
    }

    /// <summary>Формат IPXX: первая цифра 0–6 (защита от пыли), вторая 0–9 (от воды).</summary>
    private static bool IsIpRating(string value)
    {
        return value.Length == 4
               && value.StartsWith("IP")
               && value[2] >= '0' && value[2] <= '6'
               && char.IsDigit(value[3]);
    }
}
