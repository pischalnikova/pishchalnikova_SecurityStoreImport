using static SecurityStoreImport.Tests.TestHelpers;

namespace SecurityStoreImport.Tests;

/// <summary>Сценарии из раздела 8 задания и проверки варианта «Оборудование систем безопасности».</summary>
public class ProductImportServiceTests
{
    // Номера колонок в строке TestHelpers.GoodRow
    private const int PriceColumn = 3;
    private const int StockColumn = 6;
    private const int ActiveColumn = 8;
    private const int BrandColumn = 9;
    private const int SystemColumn = 10;
    private const int IpColumn = 12;

    [Fact]
    public void Scenario1_ValidFile_AllRowsImported()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("valid_cameras.csv"));

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(2, result.ImportedCount);
        Assert.Empty(result.Errors);
        Assert.Equal(2, repository.Count);
    }

    [Fact]
    public void Scenario2_NegativePrice_RowSkippedWithErrorOnLine2()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("invalid_price.csv"));

        var error = Assert.Single(result.Errors);
        Assert.Equal(2, error.RowNumber);
        Assert.Equal(ValidationMessages.PriceNotPositive, error.Message);
        Assert.Equal(0, repository.Count);
    }

    [Fact]
    public void Scenario3_EmptyFile_NoCrashAndMessage()
    {
        var (service, _) = CreateService();

        var result = service.ImportFromCsv(DataFile("empty_file.csv"));

        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(ValidationMessages.FileEmpty, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public void Scenario4_DuplicateSku_FirstImportedSecondWarning()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("duplicate_sku.csv"));

        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(0, result.ErrorCount);
        var warning = Assert.Single(result.Errors);
        Assert.True(warning.IsWarning);
        Assert.Equal(3, warning.RowNumber);
        Assert.Equal("IP-камера уличная 4 Мп", repository.GetBySku("GRD-1001")!.Name);
    }

    [Fact]
    public void Scenario5_NegativeStock_RowSkipped()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("negative_stock.csv"));

        Assert.Equal(ValidationMessages.StockNegative, Assert.Single(result.Errors).Message);
        Assert.Equal(0, repository.Count);
    }

    [Fact]
    public void Scenario6_Windows1251File_CyrillicIsReadable()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("cyrillic_windows1251.csv"));

        Assert.Equal(2, result.ImportedCount);
        Assert.Equal("Windows-1251", result.Encoding);
        Assert.Equal("Извещатель охранный Фотон-12", repository.GetBySku("GRD-2201")!.Name);
    }

    [Fact]
    public void RepeatedImport_DoesNotCrash_AllRowsAreCatalogDuplicates()
    {
        var (service, repository) = CreateService();
        service.ImportFromCsv(DataFile("valid_cameras.csv"));

        var second = service.ImportFromCsv(DataFile("valid_cameras.csv"));

        Assert.Equal(0, second.ImportedCount);
        Assert.Equal(2, second.WarningCount);
        Assert.Equal(2, repository.Count);
    }

    [Fact]
    public void MissingFile_ReturnsFileNotFound()
    {
        var (service, _) = CreateService();

        var result = service.ImportFromCsv(DataFile("no_such_file.csv"));

        Assert.Equal(ValidationMessages.FileNotFound, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public void WrongHeaders_FileRejected()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("wrong_headers.csv"));

        Assert.StartsWith("Неверные заголовки CSV", Assert.Single(result.Errors).Message);
        Assert.Equal(0, repository.Count);
    }

    [Fact]
    public void HeaderOnly_NoDataMessage()
    {
        var (service, _) = CreateService();

        var result = service.ImportFromText(Header + "\n", "test");

        Assert.Equal(ValidationMessages.NoDataRows, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public void SemicolonFile_DelimiterDetectedAndCommaDecimalAccepted()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("semicolon_delimiter.csv"));

        Assert.Equal(2, result.ImportedCount);
        Assert.Equal(1290.50m, repository.GetBySku("GRD-1201")!.Price);
    }

    [Fact]
    public void MixedErrors_CountsAndLineNumbers()
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromCsv(DataFile("mixed_errors.csv"));

        Assert.Equal(8, result.TotalRows);
        Assert.Equal(3, result.ImportedCount);
        Assert.Equal(6, result.ErrorCount);
        Assert.Equal(1, result.WarningCount);
        Assert.Equal(3, result.Errors.Count(e => e.RowNumber == 5));   // цена, остаток, is_active
        Assert.Contains(result.Errors, e => e.RowNumber == 8);         // не хватает поля
        Assert.Equal("Контроллер \"Gate-8000\"", repository.GetBySku("GRD-5006")!.Name);
        Assert.Equal("СКУД", repository.GetBySku("GRD-5006")!.SystemType);
    }

    [Theory]
    [InlineData(PriceColumn, "0", ValidationMessages.PriceNotPositive)]
    [InlineData(PriceColumn, "дорого", ValidationMessages.PriceInvalid)]
    [InlineData(StockColumn, "2.5", ValidationMessages.StockInvalid)]
    [InlineData(ActiveColumn, "yes", ValidationMessages.IsActiveInvalid)]
    [InlineData(BrandColumn, "", ValidationMessages.BrandEmpty)]
    [InlineData(IpColumn, "IP99", ValidationMessages.IpRatingInvalid)]
    public void InvalidField_RowRejectedWithMessage(int column, string value, string expectedMessage)
    {
        var (service, repository) = CreateService();

        var result = service.ImportFromText(Header + "\n" + RowWith(column, value), "test");

        Assert.Equal(expectedMessage, Assert.Single(result.Errors).Message);
        Assert.Equal(0, repository.Count);
    }

    [Fact]
    public void UnknownSystemType_RowRejected()
    {
        var (service, _) = CreateService();

        var result = service.ImportFromText(Header + "\n" + RowWith(SystemColumn, "Пожаротушение"), "test");

        var error = Assert.Single(result.Errors);
        Assert.Equal(CsvColumns.SystemType, error.Column);
    }

    [Fact]
    public void SystemType_CaseIgnored()
    {
        var (service, repository) = CreateService();

        service.ImportFromText(Header + "\n" + RowWith(SystemColumn, "скуд"), "test");

        Assert.Equal("СКУД", repository.GetBySku("GRD-1001")!.SystemType);
    }

    [Fact]
    public void EmptyOptionalFields_DefaultsApplied()
    {
        var (service, repository) = CreateService();
        var row = "GRD-7,Кабель,,99,,,1,,1,Rexant,Видеонаблюдение,,";

        service.ImportFromText(Header + "\n" + row, "test");

        var product = repository.GetBySku("GRD-7")!;
        Assert.Equal("RUB", product.Currency);
        Assert.Equal("Без категории", product.CategoryName);
        Assert.True(product.IsActive);
    }
}
