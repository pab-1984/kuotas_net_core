// Kuotasmig.Core/Pages/CancelacionAnticipada.cshtml.cs
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class CancelacionAnticipadaModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CancelacionAnticipadaModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ImporteAPagar { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "La Tasa de Interés Mensual es obligatoria.")]
            [Display(Name = "Tasa de Interés Mensual (%)")]
            public double? TasaInteresMensual { get; set; }

            [Required(ErrorMessage = "El Número Total de Cuotas es obligatorio.")]
            [Range(1, int.MaxValue, ErrorMessage = "El número total de cuotas debe ser al menos 1.")]
            [Display(Name = "Número Total de Cuotas")]
            public int? NumeroTotalCuotas { get; set; }

            [Required(ErrorMessage = "El Monto de Cada Cuota es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El monto de la cuota debe ser mayor a cero.")]
            [Display(Name = "Monto de Cada Cuota ($)")]
            public double? MontoDeCadaCuota { get; set; }

            [Required(ErrorMessage = "Las Cuotas Ya Pagas son obligatorias.")]
            [Range(0, int.MaxValue, ErrorMessage = "Las cuotas ya pagas no pueden ser negativas.")]
            [Display(Name = "Cuotas Ya Pagas")]
            public int? CuotasYaPagas { get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPostCalcular()
        {
            LimpiarResultados();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.CuotasYaPagas >= Input.NumeroTotalCuotas)
            {
                ErrorMessage = "Las cuotas ya pagas no pueden ser iguales o mayores al número total de cuotas.";
                return Page();
            }

            double resultado = _calculoService.CalcularImporteCancelacionAnticipada(
                Input.TasaInteresMensual!.Value,
                Input.NumeroTotalCuotas!.Value,
                Input.MontoDeCadaCuota!.Value,
                Input.CuotasYaPagas!.Value
            );

            ImporteAPagar = resultado.ToString("N2", CultureInfo.InvariantCulture);
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel();
            LimpiarResultados();
            return Page();
        }

        private void LimpiarResultados()
        {
            ImporteAPagar = null;
            ErrorMessage = null;
        }
    }
}