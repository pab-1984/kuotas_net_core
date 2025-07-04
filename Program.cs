using Kuotasmig.Core.Data;
// Quitamos el using a Kuotasmig.Core.Models ya que no lo necesitamos aquí.
using Kuotasmig.Core.Services; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuración de la Base de Datos con EF Core ---
var connectionString = builder.Configuration.GetConnectionString("P2015ConnectionString") ?? throw new InvalidOperationException("Connection string 'P2015ConnectionString' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// --- 2. Configuración de ASP.NET Core Identity ---

// CAMBIO 1: Eliminamos la línea AddScoped redundante.
// builder.Services.AddScoped<Kuotasmig.Core.Data.ApplicationDbContext>();

// CAMBIO 2: Cambiamos 'ApplicationUser' por 'IdentityUser' para que coincida con tu DbContext.
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    // Tu configuración de contraseña está bien, la dejamos como está.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 1;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Esta línea es importante para que las páginas de Login/Registro funcionen.
builder.Services.AddRazorPages();

// --- 3. Registrar tus Servicios de Cálculo ---
// Asumimos que ya has modificado estos servicios para que usen ApplicationDbContext
builder.Services.AddScoped<CalculadoraService>(); 
builder.Services.AddScoped<CalculoAmortizacionService>();


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-US") };
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddHttpContextAccessor();
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

var localizationOptions = app.Services.GetService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>()?.Value;
if (localizationOptions != null) { app.UseRequestLocalization(localizationOptions); }

app.UseRouting();

// Mantenemos la sesión aquí si la usas para algo más que el login.
app.UseSession(); 

// --- 5. Habilitar Autenticación y Autorización de Identity ---
// ¡El orden de estos es crucial y está correcto!
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();