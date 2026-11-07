# SecurityStoreImport — импорт товаров магазина оборудования безопасности из CSV

Производственная практика **ПП.02.01 «Осуществление интеграции программных модулей»**
(ПМ.02, специальность 09.02.07 «Информационные системы и программирование», квалификация — программист).

**Студент:** Пищальникова Виктория
**Место практики:** ГК «Гардиан», г. Пермь — проектирование, монтаж и обслуживание систем безопасности.
**Тема:** разработка модуля импорта товаров в электронный магазин из CSV-файлов с хранением данных в коллекциях в памяти.
**Вариант:** «Оборудование систем безопасности» — дополнительные поля `brand`, `system_type`, `model`, `ip_rating`.

Каталог содержит оборудование, с которым работает сервисная служба компании: IP-камеры и видеорегистраторы
(Hikvision, Dahua, Trassir), приборы охранной сигнализации (Болид, Теко), оборудование СКУД (Sigur, Parsec, PERCo, CAME).
Цены и остатки условные.

## Запуск

Нужен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

- **Visual Studio 2022:** открыть `SecurityStoreImport.sln`, запускаемый проект — `SecurityStoreImport`, F5.
- **Командная строка:** `dotnet run --project src/SecurityStoreImport` → открыть http://localhost:5240.
- **Тесты:** `dotnet test`.

Быстрая проверка: «Импорт из CSV» → в поле пути ввести `/csv/samples/catalog.csv` → «Импортировать товары» → «Товары».

## Возможности

Обязательные: чтение CSV по пути, разбор строк в объекты `Product`, проверка данных,
список ошибок с номерами строк, хранение товаров в памяти, страница импорта и страница списка товаров,
повторный импорт без падения приложения.

Дополнительные (п. 11.2 задания):

| № | Функция | Где реализовано |
|---|---------|-----------------|
| 1 | Загрузка файла через браузер | `ProductsController.Import` (POST), `Import.cshtml` |
| 2 | Поиск по названию, бренду, артикулу | `CatalogFilter.Apply` |
| 3 | Фильтр по типу системы | `CatalogFilter.Apply` |
| 4 | Сортировка по цене и остатку | `CatalogFilter.Apply` |
| 5 | Страница деталей товара | `ProductsController.Details` |
| 6 | Количество товаров и стоимость остатков | `ProductListViewModel` |
| 7 | Экспорт в CSV | `CatalogCsvExporter` |
| 8 | Unit-тесты (xUnit) | `tests/SecurityStoreImport.Tests` |
| 10 | Разделители `,` `;` и табуляция | `CsvReader.DetectDelimiter` |

Также: определение кодировки UTF-8 / Windows-1251, очистка списка товаров.

## Архитектура

```
CSV-файл (путь или загрузка через браузер)
   ↓
ProductsController        — принимает форму, передаёт файл сервису
   ↓
ProductImportService      — кодировка → CsvReader (разбор) → проверка заголовка
   ↓
ProductImportRow → ProductRowValidator → Product
   ↓
InMemoryProductRepository — Dictionary по артикулу + List для порядка (singleton)
   ↓
ProductsController → Views (Index, Import, Details)
```

```
src/SecurityStoreImport/
├── Controllers/  ProductsController (Index, Details, Import GET/POST, Export, Clear), HomeController
├── Models/       Product, ProductImportRow, ImportResult, ImportError, ImportRequest,
│                 ImportRules, CsvColumns, ValidationMessages, ViewModels
├── Services/     IProductRepository, InMemoryProductRepository, IProductImportService,
│                 ProductImportService, ProductRowValidator, CatalogFilter, CatalogCsvExporter, Csv/CsvReader
├── Views/        Products/Index, Import, Details; Shared/_Layout
└── wwwroot/csv/samples/  примеры и тестовые CSV-файлы
tests/SecurityStoreImport.Tests/  RepositoryTests, CsvReaderTests, ProductImportServiceTests
```

Репозиторий и сервис зарегистрированы как **singleton** — товары хранятся между запросами
и пропадают только после перезапуска приложения (это допускается заданием).

## Формат CSV

```
sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,system_type,model,ip_rating
GRD-1001,IP-камера уличная 4 Мп,Цилиндрическая камера с ИК-подсветкой до 30 м,8490,RUB,IP-камеры,18,/images/hik-2043.jpg,true,Hikvision,Видеонаблюдение,DS-2CD2043G2-I,IP67
```

| Поле | Тип | Обязательно | Правило |
|------|-----|-------------|---------|
| sku | string | да | не пустой, уникальный |
| name | string | да | не пустое |
| description | string | нет | — |
| price | decimal | да | число > 0, дробная часть через точку или запятую |
| currency | string | нет | 3 латинские буквы, пусто → RUB |
| category_name | string | нет | пусто → «Без категории» |
| stock | int | да | целое ≥ 0 |
| image_url | string | нет | — |
| is_active | bool | да | true / false / 1 / 0 |
| brand | string | да | производитель |
| system_type | string | да | Видеонаблюдение, Охранная сигнализация, СКУД |
| model | string | нет | — |
| ip_rating | string | нет | формат IPXX: IP00…IP69 |

Все колонки должны быть в заголовке, порядок колонок любой.

Принятые решения:
- **Дубликаты артикула** (в файле или уже в каталоге) пропускаются и выводятся как *предупреждение* — рекомендованный заданием вариант.
- В строке проверяются все поля сразу, пользователь видит все ошибки строки.
- Номер строки — номер строки в файле (заголовок — строка 1); у ошибок всего файла вместо номера «—».
- Пустые строки пропускаются и не входят в «Всего строк».

## Тестовые сценарии

| № | Сценарий | Файл | Ожидаемый результат | Факт |
|---|----------|------|---------------------|------|
| 1 | Успешный импорт | catalog.csv | 16 строк, 16 импортировано, 0 ошибок | Выполнено |
| 2 | Корректный файл | valid_cameras.csv | 2 / 2 / 0 | Выполнено |
| 3 | Неверная цена (-500) | invalid_price.csv | товар не добавлен, строка 2: «Цена должна быть больше нуля» | Выполнено |
| 4 | Пустой файл | empty_file.csv | не падает, «Файл пуст: данные для импорта отсутствуют» | Выполнено |
| 5 | Только заголовок | header_only.csv | «данные для импорта отсутствуют» | Выполнено |
| 6 | Дубликат артикула | duplicate_sku.csv | 1 импортирован, предупреждение по строке 3 | Выполнено |
| 7 | Отрицательный остаток | negative_stock.csv | «Остаток на складе не может быть отрицательным» | Выполнено |
| 8 | Кириллица Windows-1251 | cyrillic_windows1251.csv | названия без «кракозябр» | Выполнено |
| 9 | Неверный is_active («да») | bad_active.csv | ошибка is_active | Выполнено |
| 10 | Неизвестный тип системы | unknown_system_type.csv | ошибка system_type | Выполнено |
| 11 | Нет производителя | missing_brand.csv | ошибка brand | Выполнено |
| 12 | Неверная степень защиты (IP-66) | bad_ip_rating.csv | ошибка ip_rating | Выполнено |
| 13 | Неверные заголовки | wrong_headers.csv | «Неверные заголовки CSV: нет колонок …» | Выполнено |
| 14 | Разделитель `;`, цена 1290,50 | semicolon_delimiter.csv | 2 импортировано | Выполнено |
| 15 | Разделитель TAB | tab_delimiter.csv | 1 импортирован | Выполнено |
| 16 | Смешанные ошибки | mixed_errors.csv | 8 строк, 3 импортировано, 6 ошибок, 1 предупреждение | Выполнено |
| 17 | Повторный импорт | catalog.csv дважды | не падает, 16 предупреждений | Выполнено |
| 18 | Файл не найден / пустой путь / файл не выбран | — | понятное сообщение | Выполнено |

Автотесты (`dotnet test`): 35 проверок — репозиторий, разбор CSV и кодировок, сервис импорта.

## Самопроверка кода

| Критерий | Как выполнено |
|----------|---------------|
| Имена | `ProductImportService.ImportFromCsv`, `ProductRowValidator.ValidateRow`, `CsvReader.DetectDelimiter` |
| Обработка ошибок | ошибки чтения файла перехватываются в сервисе, непредвиденные — в контроллере; приложение не падает |
| Разделение ответственности | контроллер не читает файл и не валидирует; разбор, проверка и хранение — в отдельных классах |
| Комментарии | XML-комментарии у классов и ключевых методов |
| Магические числа | `ImportRules`, `CsvColumns`, `ValidationMessages` |
| Git | осмысленные сообщения коммитов (`git log`) |
