// Kuotasmig.Core/Program.cs
using Kuotasmig.Core.Data;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Http; // Necesario para AddHttpContextAccessor

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Registrar tus servicios para Inyección de Dependencias
builder.Services.AddScoped<BDService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CalculadoraService>();
builder.Services.AddScoped<CalculoAmortizacionService>();

// Necesario para acceder a HttpContext desde servicios o clases que no son PageModel/Controller directamente (como _Layout.cshtml)
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

app.UseRouting();

// app.UseAuthentication(); // Descomentar cuando implementes ASP.NET Core Identity
// app.UseAuthorization();   // Descomentar cuando implementes ASP.NET Core Identity

app.UseSession(); // Habilitar el middleware de sesión ¡IMPORTANTE que esté ANTES de MapRazorPages/MapControllers!

app.MapRazorPages();

app.Run();