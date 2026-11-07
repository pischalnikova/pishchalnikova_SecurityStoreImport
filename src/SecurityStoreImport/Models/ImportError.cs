namespace SecurityStoreImport.Models;

/// <summary>Ошибка или предупреждение импорта с номером строки файла.</summary>
public class ImportError
{
    /// <summary>Номер строки в файле. 0 — ошибка относится ко всему файлу.</summary>
    public int RowNumber { get; set; }

    /// <summary>Колонка, в которой найдена ошибка (может быть пустой).</summary>
    public string Column { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>true — строка пропущена, но это не ошибка данных (повтор артикула).</summary>
    public bool IsWarning { get; set; }
}
