using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Log-Ordner sicherstellen ---
var logPath = Path.Combine(AppContext.BaseDirectory, "logs");
if (!Directory.Exists(logPath))
    Directory.CreateDirectory(logPath);

// --- Serilog konfigurieren ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: Path.Combine(logPath, "moviehall.log"),
        rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// --- Windows Service aktivieren ---
builder.Host.UseWindowsService();

// --- Kestrel konfigurieren ---
builder.WebHost.UseUrls("http://localhost:5000");

// --- Connection-String prüfen ---
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
Log.Information("Using connection string: {ConnStr}", connStr);

// --- MVC + DbContext ---
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connStr));

var app = builder.Build();

// --- HTTP Pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    Log.Information("MovieHall Service is starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MovieHall terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}
