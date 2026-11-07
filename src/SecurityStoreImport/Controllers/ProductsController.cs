using Microsoft.AspNetCore.Mvc;
using SecurityStoreImport.Models;
using SecurityStoreImport.Services;

namespace SecurityStoreImport.Controllers;

/// <summary>
/// Страницы товаров и импорта. Контроллер не читает файл и не проверяет данные —
/// он только передаёт файл сервису импорта и показывает результат.
/// </summary>
public class ProductsController : Controller
{
    private readonly IProductRepository _repository;
    private readonly IProductImportService _importService;
    private readonly IWebHostEnvironment _environment;

    public ProductsController(IProductRepository repository, IProductImportService importService,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _importService = importService;
        _environment = environment;
    }

    /// <summary>Список товаров с поиском, фильтром по системе и сортировкой.</summary>
    public IActionResult Index(string? search, string? system, string? sort)
    {
        var all = _repository.GetAll();

        var model = new ProductListViewModel
        {
            Products = CatalogFilter.Apply(all, search, system, sort),
            TotalInCatalog = all.Count,
            Search = search,
            System = system,
            Sort = sort
        };

        return View(model);
    }

    public IActionResult Details(string id)
    {
        var product = _repository.GetBySku(id);
        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View(new ImportPageModel());
    }

    /// <summary>Импорт: загруженный файл (Mode = upload) или путь к файлу (Mode = path).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Import(ImportRequest request)
    {
        var model = new ImportPageModel { FilePath = request.FilePath ?? "" };

        try
        {
            if (request.Mode == ImportRequest.PathMode)
                model.Result = ImportByPath(request.FilePath);
            else
                model.Result = ImportUploadedFile(request.File);
        }
        catch (Exception ex)
        {
            // Приложение не должно падать ни при каком файле
            model.Result = ImportResult.Fail("", ValidationMessages.Unexpected + ": " + ex.Message);
        }

        return View(model);
    }

    /// <summary>Скачать все товары в CSV.</summary>
    public IActionResult Export()
    {
        var bytes = CatalogCsvExporter.Export(_repository.GetAll());
        return File(bytes, "text/csv", "products.csv");
    }

    /// <summary>Очистить список, чтобы повторить импорт с нуля.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _repository.Clear();
        TempData["Notice"] = "Все товары удалены из памяти";
        return RedirectToAction(nameof(Index));
    }

    private ImportResult ImportByPath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return ImportResult.Fail("", ValidationMessages.PathRequired);

        return _importService.ImportFromCsv(ResolvePath(filePath));
    }

    private ImportResult ImportUploadedFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return ImportResult.Fail("", ValidationMessages.FileNotSelected);

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return ImportResult.Fail(file.FileName, ValidationMessages.NotCsv);

        if (file.Length > ImportRules.MaxFileSize)
            return ImportResult.Fail(file.FileName, ValidationMessages.FileTooLarge);

        using var stream = file.OpenReadStream();
        return _importService.ImportFromStream(stream, file.FileName);
    }

    /// <summary>
    /// Если по указанному пути файл есть — берём его (например, D:\data\goods.csv).
    /// Иначе считаем путь относительно папки wwwroot (например, /csv/catalog.csv).
    /// </summary>
    private string ResolvePath(string filePath)
    {
        var path = filePath.Trim().Trim('"');
        if (System.IO.File.Exists(path))
            return path;

        return Path.Combine(_environment.WebRootPath, path.TrimStart('/', '\\'));
    }
}
