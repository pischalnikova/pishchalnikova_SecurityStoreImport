namespace SecurityStoreImport.Models;

/// <summary>Данные формы импорта: либо загруженный файл, либо путь к файлу.</summary>
public class ImportRequest
{
    public const string UploadMode = "upload";
    public const string PathMode = "path";

    /// <summary>Способ импорта — какая вкладка формы была отправлена.</summary>
    public string Mode { get; set; } = UploadMode;

    public IFormFile? File { get; set; }

    public string? FilePath { get; set; }
}
