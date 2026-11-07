using System.Text;
using SecurityStoreImport.Models;
using SecurityStoreImport.Services.Csv;

namespace SecurityStoreImport.Services;

/// <summary>
/// Сервис импорта товаров из CSV:
/// 1) читает файл и определяет кодировку; 2) разбирает записи;
/// 3) проверяет заголовок; 4) проверяет строки; 5) добавляет товары в репозиторий.
/// Повторы артикулов не добавляются и выводятся как предупреждения.
/// </summary>
public class ProductImportService : IProductImportService
{
    private readonly IProductRepository _repository;
    private readonly ProductRowValidator _validator;

    public ProductImportService(IProductRepository repository, ProductRowValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public ImportResult ImportFromCsv(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        if (!File.Exists(filePath))
            return ImportResult.Fail(fileName, ValidationMessages.FileNotFound);

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            return ImportFromBytes(bytes, fileName);
        }
        catch (IOException ex)
        {
            return ImportResult.Fail(fileName, ValidationMessages.FileReadFailed + ": " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ImportResult.Fail(fileName, ValidationMessages.FileReadFailed + ": " + ex.Message);
        }
    }

    public ImportResult ImportFromStream(Stream stream, string fileName)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return ImportFromBytes(memory.ToArray(), fileName);
    }

    /// <summary>Импорт из готового текста (используется в тестах).</summary>
    public ImportResult ImportFromText(string text, string fileName)
    {
        var result = new ImportResult { FileName = fileName, Encoding = "UTF-8" };
        ImportText(text, result);
        return result;
    }

    private ImportResult ImportFromBytes(byte[] bytes, string fileName)
    {
        var result = new ImportResult { FileName = fileName };
        var text = DecodeText(bytes, result);
        ImportText(text, result);
        return result;
    }

    /// <summary>
    /// Сначала читаем файл как UTF-8. Если появились символы-замены «�»,
    /// значит, файл в другой кодировке — читаем его как Windows-1251.
    /// </summary>
    private static string DecodeText(byte[] bytes, ImportResult result)
    {
        var text = Encoding.UTF8.GetString(bytes);
        if (!text.Contains('�'))
        {
            result.Encoding = "UTF-8";
            return text;
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        result.Encoding = "Windows-1251";
        return Encoding.GetEncoding(1251).GetString(bytes);
    }

    private void ImportText(string text, ImportResult result)
    {
        text = text.TrimStart('﻿'); // убираем BOM

        if (string.IsNullOrWhiteSpace(text))
        {
            result.AddError(ImportRules.FileRow, ValidationMessages.FileEmpty);
            return;
        }

        var headerLine = text.Split('\n').First(l => l.Trim() != "");
        var delimiter = CsvReader.DetectDelimiter(headerLine);
        var records = CsvReader.ReadAll(text, delimiter).Where(r => !r.IsBlank).ToList();

        if (records.Count == 0)
        {
            result.AddError(ImportRules.FileRow, ValidationMessages.FileEmpty);
            return;
        }

        // Заголовок: запоминаем номер каждой колонки
        var header = records[0];
        var columns = new Dictionary<string, int>();
        for (var i = 0; i < header.Fields.Count; i++)
            columns[header.Fields[i].Trim().ToLower()] = i;

        foreach (var column in CsvColumns.All)
        {
            if (!columns.ContainsKey(column))
            {
                result.AddError(header.LineNumber, string.Format(ValidationMessages.WrongHeaders, column));
                return;
            }
        }

        if (records.Count == 1)
        {
            result.AddError(ImportRules.FileRow, ValidationMessages.NoDataRows);
            return;
        }

        // Артикул → номер строки, где он встретился впервые
        var skusInFile = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in records.Skip(1))
        {
            result.TotalRows++;
            ImportRecord(record, columns, header.Fields.Count, skusInFile, result);
        }
    }

    private void ImportRecord(CsvRecord record, Dictionary<string, int> columns, int headerSize,
        Dictionary<string, int> skusInFile, ImportResult result)
    {
        var line = record.LineNumber;

        if (record.Fields.Count != headerSize)
        {
            result.AddError(line, string.Format(ValidationMessages.FieldCount, record.Fields.Count, headerSize));
            return;
        }

        var row = ToImportRow(record, columns);
        var errors = new List<(string Column, string Message)>();
        var product = _validator.ValidateRow(row, errors);

        if (product == null)
        {
            foreach (var error in errors)
                result.AddError(line, error.Message, error.Column);
            return;
        }

        if (skusInFile.TryGetValue(product.Sku, out var firstLine))
        {
            result.AddWarning(line, string.Format(ValidationMessages.DuplicateInFile, product.Sku, firstLine), CsvColumns.Sku);
            return;
        }

        skusInFile.Add(product.Sku, line);

        if (_repository.Exists(product.Sku))
        {
            result.AddWarning(line, string.Format(ValidationMessages.DuplicateInCatalog, product.Sku), CsvColumns.Sku);
            return;
        }

        _repository.Add(product);
        result.ImportedCount++;
    }

    /// <summary>Переносит значения из записи CSV в DTO по номерам колонок.</summary>
    private static ProductImportRow ToImportRow(CsvRecord record, Dictionary<string, int> columns)
    {
        string Get(string column) => record.Fields[columns[column]];

        return new ProductImportRow
        {
            LineNumber = record.LineNumber,
            Sku = Get(CsvColumns.Sku),
            Name = Get(CsvColumns.Name),
            Description = Get(CsvColumns.Description),
            Price = Get(CsvColumns.Price),
            Currency = Get(CsvColumns.Currency),
            CategoryName = Get(CsvColumns.Category),
            Stock = Get(CsvColumns.Stock),
            ImageUrl = Get(CsvColumns.ImageUrl),
            IsActive = Get(CsvColumns.IsActive),
            Brand = Get(CsvColumns.Brand),
            SystemType = Get(CsvColumns.SystemType),
            Model = Get(CsvColumns.Model),
            IpRating = Get(CsvColumns.IpRating)
        };
    }
}
