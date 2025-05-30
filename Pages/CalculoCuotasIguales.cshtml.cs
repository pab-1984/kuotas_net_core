// Kuotasmig.Core/Pages/CalculoCuotasIguales.cshtml.cs
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization; // Necesario para CultureInfo si se usa en el PageModel

namespace Kuotasmig.Core.Pages
{
    public class CalculoCuotasIgualesModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CalculoCuotasIgualesModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        // Propiedad para almacenar el resultado que se mostrará en la tabla
        public CalculoAmortizacionService.ResultadoCalculoCuota? ResultadoCuota { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El Capital Solicitado es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El capital debe ser mayor a cero.")]
            [Display(Name = "Capital Solicitado ($)")]
            public double? CapitalSolicitado { get; set; }

            [Required(ErrorMessage = "La Tasa de Interés Anual es obligatoria.")]
            [Display(Name = "Tasa de Interés Anual (%)")]
            public double? TasaInteresAnual { get; set; } // Puede ser 0

            [Required(ErrorMessage = "La Cantidad de Meses es obligatoria.")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad de meses debe ser al menos 1.")]
            [Display(Name = "Cantidad de Meses")]
            public int? CantidadMeses { get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPostCalcular()
        {
            ResultadoCuota = null; // Limpiar resultado previo
            ErrorMessage = null;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = _calculoService.CalcularCuotaFijaMensual(
                Input.CapitalSolicitado!.Value,
                Input.TasaInteresAnual!.Value,
                Input.CantidadMeses!.Value
            );

            if (resultado.Error)
            {
                ErrorMessage = resultado.MensajeError;
            }
            else
            {
                ResultadoCuota = resultado;
            }
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel();
            ResultadoCuota = null;
            ErrorMessage = null;
            return Page();
        }
    }
}