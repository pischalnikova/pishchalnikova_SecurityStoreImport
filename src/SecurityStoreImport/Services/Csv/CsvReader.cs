using System.Text;

namespace SecurityStoreImport.Services.Csv;

/// <summary>Одна запись CSV: номер строки в файле и значения полей.</summary>
public class CsvRecord
{
    public int LineNumber { get; set; }
    public List<string> Fields { get; set; } = new();

    /// <summary>Пустая строка (или строка из одних разделителей).</summary>
    public bool IsBlank => Fields.All(string.IsNullOrWhiteSpace);
}

/// <summary>
/// Разбор CSV. Текст читается посимвольно, поэтому поддерживаются:
/// значения в кавычках (внутри может быть разделитель или перевод строки)
/// и двойные кавычки "" внутри значения.
/// </summary>
public static class CsvReader
{
    private const char Quote = '"';

    /// <summary>Разбирает весь текст файла на записи.</summary>
    public static List<CsvRecord> ReadAll(string text, char delimiter)
    {
        var records = new List<CsvRecord>();
        var current = new CsvRecord { LineNumber = 1 };
        var value = new StringBuilder();
        var inQuotes = false;
        var line = 1;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            var next = i + 1 < text.Length ? text[i + 1] : '\0';

            if (inQuotes)
            {
                if (ch == Quote && next == Quote)
                {
                    value.Append(Quote); // "" внутри кавычек — это одна кавычка
                    i++;
                }
                else if (ch == Quote)
                {
                    inQuotes = false;
                }
                else
                {
                    if (ch == '\n') line++;
                    value.Append(ch);
                }
            }
            else if (ch == Quote && value.ToString().Trim().Length == 0)
            {
                inQuotes = true; // кавычка в начале значения открывает поле в кавычках
                value.Clear();
            }
            else if (ch == delimiter)
            {
                current.Fields.Add(value.ToString().Trim());
                value.Clear();
            }
            else if (ch == '\n' || ch == '\r')
            {
                if (ch == '\r' && next == '\n') i++;

                current.Fields.Add(value.ToString().Trim());
                records.Add(current);
                value.Clear();
                line++;
                current = new CsvRecord { LineNumber = line };
            }
            else
            {
                value.Append(ch); // в том числе кавычка в середине значения: Монитор 22"
            }
        }

        // Последняя строка, если в конце файла нет перевода строки
        if (value.Length > 0 || current.Fields.Count > 0)
        {
            current.Fields.Add(value.ToString().Trim());
            records.Add(current);
        }

        return records;
    }

    /// <summary>Определяет разделитель по заголовку: , ; или табуляция — что встречается чаще.</summary>
    public static char DetectDelimiter(string headerLine)
    {
        var commas = headerLine.Split(',').Length;
        var semicolons = headerLine.Split(';').Length;
        var tabs = headerLine.Split('\t').Length;

        if (semicolons > commas && semicolons >= tabs) return ';';
        if (tabs > commas && tabs > semicolons) return '\t';
        return ',';
    }

    /// <summary>Подготавливает значение для записи в CSV (берёт в кавычки при необходимости).</summary>
    public static string Escape(string value, char delimiter)
    {
        if (value.Contains(delimiter) || value.Contains(Quote) || value.Contains('\n'))
            return Quote + value.Replace("\"", "\"\"") + Quote;

        return value;
    }
}
