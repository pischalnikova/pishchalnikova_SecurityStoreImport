namespace SecurityStoreImport.Models;

/// <summary>Результат одного запуска импорта.</summary>
public class ImportResult
{
    public string FileName { get; set; } = string.Empty;

    /// <summary>Количество строк с данными (без заголовка и пустых строк).</summary>
    public int TotalRows { get; set; }

    public int ImportedCount { get; set; }

    public List<ImportError> Errors { get; set; } = new();

    public int ErrorCount => Errors.Count(e => !e.IsWarning);

    public int WarningCount => Errors.Count(e => e.IsWarning);

    /// <summary>Кодировка, в которой прочитан файл (UTF-8 или Windows-1251).</summary>
    public string Encoding { get; set; } = string.Empty;

    public void AddError(int row, string message, string column = "")
    {
        Errors.Add(new ImportError { RowNumber = row, Message = message, Column = column });
    }

    public void AddWarning(int row, string message, string column = "")
    {
        Errors.Add(new ImportError { RowNumber = row, Message = message, Column = column, IsWarning = true });
    }

    /// <summary>Создаёт результат с одной ошибкой уровня файла.</summary>
    public static ImportResult Fail(string fileName, string message)
    {
        var result = new ImportResult { FileName = fileName };
        result.AddError(ImportRules.FileRow, message);
        return result;
    }
}
