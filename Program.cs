// Kuotasmig.Core/Program.cs
using Kuotasmig.Core.Data;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Http;
using System.Globalization; // Necesario para CultureInfo
using Microsoft.AspNetCore.Localization; // Necesario para RequestLocalizationOptions

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// --- INICIO DE CAMBIOS DE LOCALIZACIÓN ---

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US") // Usaremos en-US como cultura por defecto para el parseo de números
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// --- FIN DE CAMBIOS DE LOCALIZACIÓN ---


// Registrar tus servicios para Inyección de Dependencias
builder.Services.AddScoped<BDService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CalculadoraService>();
builder.Services.AddScoped<CalculoAmortizacionService>();

builder.Services.AddHttpContextAccessor();

// Configuración para el estado de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(600);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// --- AÑADIR MIDDLEWARE DE LOCALIZACIÓN ---
var localizationOptions = app.Services.GetService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>()?.Value;
if (localizationOptions != null)
{
    app.UseRequestLocalization(localizationOptions);
}
// --- FIN DE MIDDLEWARE ---

app.UseRouting();

// app.UseAuthentication(); // Descomentar cuando implementes ASP.NET Core Identity
// app.UseAuthorization();   // Descomentar cuando implementes ASP.NET Core Identity

app.UseSession(); 

app.MapRazorPages();

app.Run();
