namespace SecurityStoreImport.Models;

/// <summary>Правила и значения по умолчанию для импорта (вместо «магических» чисел в коде).</summary>
public static class ImportRules
{
    public const string DefaultCurrency = "RUB";
    public const string DefaultCategory = "Без категории";

    /// <summary>Длина кода валюты (RUB, USD).</summary>
    public const int CurrencyLength = 3;

    /// <summary>Максимальный размер загружаемого файла — 2 МБ.</summary>
    public const long MaxFileSize = 2 * 1024 * 1024;

    /// <summary>Номер строки для ошибок, которые относятся ко всему файлу.</summary>
    public const int FileRow = 0;

    /// <summary>Допустимые типы систем — как в справочнике system_types базы данных компании.</summary>
    public static readonly string[] SystemTypes = { "Видеонаблюдение", "Охранная сигнализация", "СКУД" };
}
