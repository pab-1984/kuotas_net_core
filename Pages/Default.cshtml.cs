// Kuotasmig.Core/Pages/Default.cshtml.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kuotasmig.Core.Pages
{
    public class DefaultModel : PageModel
    {
        public string NombreUsuario { get; private set; }
        public string TipoUsuario { get; private set; }
        public bool IsUserLoggedIn { get; private set; }


        public IActionResult OnGet()
        {
            // Leer de la sesión (esto es temporal hasta que implementes Identity)
            NombreUsuario = HttpContext.Session.GetString("loginNOMBRE");
            TipoUsuario = HttpContext.Session.GetString("loginTIPOUSUARIO");
            IsUserLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("loginUSUARIO"));

            if (!IsUserLoggedIn)
            {
                // Si no hay sesión, redirigir al Login
                return RedirectToPage("/Login");
            }
            return Page();
        }

        public IActionResult OnPostLogout()
        {
            // Lógica para cerrar sesión (temporal)
            HttpContext.Session.Clear();
            // Con Identity sería: await _signInManager.SignOutAsync();
            return RedirectToPage("/Login");
        }
    }
}