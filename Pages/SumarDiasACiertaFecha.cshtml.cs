// Kuotasmig.Core/Pages/SumarDiasACiertaFecha.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;

namespace Kuotasmig.Core.Pages
{
    public class SumarDiasACiertaFechaModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? FechaResultado { get; set; } // Para mostrar la fecha calculada
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha Desde")]
            public DateTime? FechaInicio { get; set; }

            [Required(ErrorMessage = "El número de días a sumar es obligatorio.")]
            [Range(0, int.MaxValue, ErrorMessage = "Los días a sumar deben ser un número positivo.")]
            [Display(Name = "Días a Sumar")]
            public int? DiasASumar { get; set; }
        }

        public void OnGet()
        {
            // Inicializar con valores por defecto si es necesario, como en el original
            Input.FechaInicio = DateTime.Today;
            Input.DiasASumar = 0; // O un valor por defecto que prefieras
        }

        public IActionResult OnPostCalcular()
        {
            if (!ModelState.IsValid)
            {
                FechaResultado = null; // Limpiar resultado si hay error de validación
                return Page();
            }

            // ModelState.IsValid ya ha verificado que FechaInicio y DiasASumar no son null
            // debido a [Required], por lo que podemos usar el operador null-forgiving (!)
            DateTime fechaCalculada = Input.FechaInicio!.Value.AddDays(Input.DiasASumar!.Value);
            FechaResultado = fechaCalculada.ToShortDateString();
            ErrorMessage = null; // Limpiar cualquier mensaje de error previo

            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear(); // Limpia los errores de validación
            Input = new InputModel { FechaInicio = DateTime.Today, DiasASumar = 0 }; // Restablece a valores por defecto
            FechaResultado = null;
            ErrorMessage = null;
            return Page();
        }
    }
}