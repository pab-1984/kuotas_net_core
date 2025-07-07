// Kuotasmig.Core/Pages/CalculoMoraCapital.cshtml.cs
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class CalculoMoraCapitalModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CalculoMoraCapitalModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public int? DiasDeAtraso { get; set; }
        public string? InteresesMoratorios { get; set; }
        public string? TotalAPagar { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El Capital Adeudado es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El capital debe ser mayor a cero.")]
            [Display(Name = "Capital Adeudado ($)")]
            public double? CapitalAdeudado { get; set; }

            [Required(ErrorMessage = "La Tasa de Interés por Mora Anual es obligatoria.")]
            [Display(Name = "Tasa de Interés por Mora Anual (%)")]
            public double? TasaInteresMoraAnual { get; set; } // Puede ser 0

            [Required(ErrorMessage = "La Fecha de Vencimiento es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha de Vencimiento")]
            public DateTime? FechaVencimiento { get; set; }

            [Required(ErrorMessage = "La Fecha de Pago es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha de Pago")]
            public DateTime? FechaPago { get; set; }
        }

        public void OnGet()
        {
            Input.FechaVencimiento = DateTime.Today.AddDays(-10); // Ejemplo
            Input.FechaPago = DateTime.Today;
        }

        public IActionResult OnPostCalcular()
        {
            LimpiarResultados();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // No es necesario verificar si Input.FechaPago < Input.FechaVencimiento aquí,
            // ya que el servicio lo maneja y devuelve 0 días de atraso.
            // Pero si quieres mostrar un mensaje específico en la UI, podrías hacerlo.
            if (Input.FechaPago <= Input.FechaVencimiento)
            {
                 ErrorMessage = "La fecha de pago es igual o anterior al vencimiento. No hay mora.";
                 // Opcionalmente, podrías llamar al servicio para que devuelva DiasDeAtraso = 0, etc.
                 // y mostrar esos resultados de "no mora". Por ahora, solo un mensaje.
            }


            var resultado = _calculoService.CalcularMoraDeUnCapital(
                Input.CapitalAdeudado!.Value,
                Input.TasaInteresMoraAnual!.Value,
                Input.FechaVencimiento!.Value,
                Input.FechaPago!.Value
            );

            DiasDeAtraso = resultado.DiasDeAtraso;
            InteresesMoratorios = resultado.InteresesMoratorios.ToString("N2", CultureInfo.InvariantCulture);
            TotalAPagar = resultado.TotalAPagar.ToString("N2", CultureInfo.InvariantCulture);
            
            // Limpiar ErrorMessage si el cálculo fue exitoso y no hubo error previo
            if (string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = null;


            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel { FechaVencimiento = DateTime.Today.AddDays(-10), FechaPago = DateTime.Today };
            LimpiarResultados();
            ErrorMessage = null;
            return Page();
        }

        private void LimpiarResultados()
        {
            DiasDeAtraso = null;
            InteresesMoratorios = null;
            TotalAPagar = null;
        }
    }
}
