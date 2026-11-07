using SecurityStoreImport.Models;

namespace SecurityStoreImport.Services;

/// <summary>Импорт товаров из CSV.</summary>
public interface IProductImportService
{
    /// <summary>Импорт из файла на диске.</summary>
    ImportResult ImportFromCsv(string filePath);

    /// <summary>Импорт файла, загруженного через браузер.</summary>
    ImportResult ImportFromStream(Stream stream, string fileName);
}
