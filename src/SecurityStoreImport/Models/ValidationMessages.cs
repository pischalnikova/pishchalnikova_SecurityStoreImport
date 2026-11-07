namespace SecurityStoreImport.Models;

/// <summary>Тексты сообщений об ошибках.</summary>
public static class ValidationMessages
{
    // Ошибки файла
    public const string PathRequired = "Укажите путь к CSV-файлу";
    public const string FileNotSelected = "Выберите CSV-файл для загрузки";
    public const string FileNotFound = "Файл не найден";
    public const string FileReadFailed = "Не удалось прочитать файл";
    public const string FileEmpty = "Файл пуст: данные для импорта отсутствуют";
    public const string NoDataRows = "В файле только заголовок, данные для импорта отсутствуют";
    public const string NotCsv = "Можно загружать только файлы .csv";
    public const string FileTooLarge = "Файл больше 2 МБ";
    public const string WrongHeaders = "Неверные заголовки CSV: нет колонки {0}";
    public const string Unexpected = "Непредвиденная ошибка при импорте";

    // Ошибки строк
    public const string FieldCount = "В строке {0} полей, а в заголовке {1}";
    public const string SkuEmpty = "Поле sku не может быть пустым";
    public const string NameEmpty = "Название товара не может быть пустым";
    public const string PriceInvalid = "Цена должна быть числом";
    public const string PriceNotPositive = "Цена должна быть больше нуля";
    public const string StockInvalid = "Остаток на складе должен быть целым числом";
    public const string StockNegative = "Остаток на складе не может быть отрицательным";
    public const string IsActiveInvalid = "Поле is_active принимает только true, false, 1 или 0";
    public const string CurrencyInvalid = "Код валюты должен состоять из 3 латинских букв";
    public const string BrandEmpty = "Не указан производитель (brand)";
    public const string SystemTypeInvalid = "Неизвестный тип системы. Допустимо: Видеонаблюдение, Охранная сигнализация, СКУД";
    public const string IpRatingInvalid = "Степень защиты должна быть в формате IPXX, например IP66";

    // Предупреждения
    public const string DuplicateInFile = "Артикул {0} уже был в строке {1}, строка пропущена";
    public const string DuplicateInCatalog = "Артикул {0} уже есть в каталоге, строка пропущена";
}
