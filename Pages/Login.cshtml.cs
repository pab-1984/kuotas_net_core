// Kuotasmig.Core/Pages/Login.cshtml.cs
using Kuotasmig.Core.Models;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Http; // Para HttpContext.Session
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks; // Para Task<IActionResult>

namespace Kuotasmig.Core.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UsuarioService _usuarioService;

        public LoginModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El campo Usuario es obligatorio.")]
            public string Usuario { get; set; } = string.Empty;

            [Required(ErrorMessage = "El campo Contraseña es obligatorio.")]
            [DataType(DataType.Password)]
            public string Contraseña { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            // Si el usuario ya está autenticado (usando ASP.NET Core Identity o cookies de sesión),
            // redirigirlo a la página principal.
            // Esto es solo un ejemplo de cómo podrías leer la sesión.
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("loginUSUARIO")))
            {
                return RedirectToPage("/Default"); // O tu página principal
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ADVERTENCIA DE SEGURIDAD:
            // El siguiente código es para propósitos de migración y NO ES SEGURO para producción
            // porque compara contraseñas en texto plano.
            // DEBES implementar ASP.NET Core Identity y comparar contraseñas hasheadas.
            Usuario usuarioEncontrado = _usuarioService.Buscar(Input.Usuario, Input.Contraseña);

            if (usuarioEncontrado != null)
            {
                // Autenticación exitosa (LÓGICA TEMPORAL DE SESIÓN)
                // En una aplicación real, aquí usarías SignInManager de ASP.NET Core Identity
                // para crear una cookie de autenticación segura.
                HttpContext.Session.SetString("loginUSUARIO", usuarioEncontrado.USUARIO);
                HttpContext.Session.SetString("loginNOMBRE", usuarioEncontrado.NOMBRE);
                HttpContext.Session.SetString("loginTIPOUSUARIO", usuarioEncontrado.TIPO);

                // Ejemplo de cómo sería con Identity (simplificado):
                // var result = await _signInManager.PasswordSignInAsync(Input.Usuario, Input.Contraseña, isPersistent: false, lockoutOnFailure: false);
                // if (result.Succeeded) { return RedirectToPage("/Default"); }

                return RedirectToPage("/Default"); // Redirigir a la página principal después del login
            }
            else
            {
                ErrorMessage = "**USUARIO Y/O CONTRASEÑA INCORRECTOS";
                return Page();
            }
        }
    }
}