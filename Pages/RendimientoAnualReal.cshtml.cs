// Kuotasmig.Core/Pages/RendimientoAnualReal.cshtml.cs
using Kuotasmig.Core.Services; // O el namespace de tu servicio financiero
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class RendimientoAnualRealModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService; // Usa el nombre de tu servicio

        public RendimientoAnualRealModel(CalculoAmortizacionService calculoService) // Usa el nombre de tu servicio
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ResultadoRendimientoAnual { get; set; }
        public string? ErrorMessage {get; set;}

        public class InputModel
        {
            [Required(ErrorMessage = "La Tasa Anual Facial es obligatoria.")]
            [Display(Name = "Tasa Anual Facial (%)")]
            public double? TasaAnualFacial { get; set; }

            [Required(ErrorMessage = "El Número de Pagos es obligatorio.")]
            [Range(1, double.MaxValue, ErrorMessage = "El número de pagos debe ser al menos 1.")]
            [Display(Name = "Número de Pagos en el Año")]
            public double? NumeroDePagos { get; set; } // Aunque sea "número de pagos", el original lo trataba como double.
        }

        public void OnGet()
        {
            // Puedes inicializar valores por defecto si lo deseas
            // Input.TasaAnualFacial = 0;
            // Input.NumeroDePagos = 12; // Por ejemplo
        }

        public IActionResult OnPostCalcular()
        {
            if (!ModelState.IsValid)
            {
                ResultadoRendimientoAnual = null;
                return Page();
            }

            if (Input.NumeroDePagos.HasValue && Input.NumeroDePagos.Value == 0)
            {
                ErrorMessage = "El número de pagos no puede ser cero para este cálculo.";
                ResultadoRendimientoAnual = null;
                return Page();
            }

            double resultado = _calculoService.CalcularRendimientoAnualReal(
                Input.TasaAnualFacial!.Value, // Usamos ! porque ModelState es válido
                Input.NumeroDePagos!.Value
            );
            ResultadoRendimientoAnual = resultado.ToString("N", CultureInfo.InvariantCulture) + "%"; // "N" para formato numérico general
            ErrorMessage = null;
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel();
            ResultadoRendimientoAnual = null;
            ErrorMessage = null;
            return Page();
        }
    }
}