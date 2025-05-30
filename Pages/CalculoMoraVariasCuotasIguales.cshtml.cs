// Kuotasmig.Core/Pages/CalculoMoraVariasCuotasIguales.cshtml.cs
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class CalculoMoraVariasCuotasIgualesModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CalculoMoraVariasCuotasIgualesModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ImporteAPagar { get; set; }
        public string? ErrorMessage { get; set; }


        public class InputModel
        {
            [Required(ErrorMessage = "El Monto de Cada Cuota es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
            [Display(Name = "Monto de Cada Cuota ($)")]
            public double? MontoCadaCuota { get; set; }

            [Required(ErrorMessage = "La Tasa de Interés por Mora Mensual es obligatoria.")]
            [Display(Name = "Tasa de Interés por Mora Mensual (%)")]
            public double? TasaInteresMoraMensual { get; set; } // Puede ser 0

            [Required(ErrorMessage = "El Número de Cuotas Vencidas e Impagas es obligatorio.")]
            [Range(1, int.MaxValue, ErrorMessage = "El número de cuotas debe ser al menos 1.")]
            [Display(Name = "Nº Cuotas Vencidas e Impagas")]
            public int? NumeroCuotasVencidasImpagas { get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPostCalcular()
        {
            LimpiarResultados();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = _calculoService.CalcularMoraVariasCuotasIguales(
                Input.MontoCadaCuota!.Value,
                Input.TasaInteresMoraMensual!.Value,
                Input.NumeroCuotasVencidasImpagas!.Value
            );

            ImporteAPagar = resultado.ImporteAPagar.ToString("N2", CultureInfo.InvariantCulture);
            ErrorMessage = null;
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
