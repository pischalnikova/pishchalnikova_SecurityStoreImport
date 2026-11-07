using System.Globalization;
using SecurityStoreImport.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Singleton: один экземпляр на всё приложение, поэтому товары
// сохраняются между запросами (до перезапуска приложения).
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<ProductRowValidator>();
builder.Services.AddSingleton<IProductImportService, ProductImportService>();

var app = builder.Build();

// Русский формат чисел: 8 490,00
var culture = CultureInfo.GetCultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
