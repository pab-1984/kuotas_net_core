// Kuotasmig.Core/Pages/DescuentoDocumentoCompuesto.cshtml.cs
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class DescuentoDocumentoCompuestoModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public DescuentoDocumentoCompuestoModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? DescuentoCalculado { get; set; }
        public string? ValorActualCalculado { get; set; }
        public int? DiasCalculados { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El Valor Nominal es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El Valor Nominal debe ser mayor a cero.")]
            [Display(Name = "Valor Nominal del Documento ($)")]
            public double? ValorNominal { get; set; }

            [Required(ErrorMessage = "La Tasa de Descuento Anual es obligatoria.")]
            [Display(Name = "Tasa de Descuento Anual (%)")]
            public double? TasaDescuentoAnual { get; set; } // Puede ser 0

            [Required(ErrorMessage = "La Fecha de Emisión es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha Emisión (Desde)")]
            public DateTime? FechaEmision { get; set; }

            [Required(ErrorMessage = "La Fecha de Vencimiento es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha Vencimiento (Hasta)")]
            public DateTime? FechaVencimiento { get; set; }
        }

        public void OnGet()
        {
            Input.FechaEmision = DateTime.Today;
            Input.FechaVencimiento = DateTime.Today.AddMonths(1); // Ejemplo
        }

        public IActionResult OnPostCalcular()
        {
            if (!ModelState.IsValid)
            {
                LimpiarResultados();
                return Page();
            }

            if (Input.FechaEmision!.Value > Input.FechaVencimiento!.Value)
            {
                ErrorMessage = "La Fecha de Vencimiento no puede ser anterior a la Fecha de Emisión.";
                LimpiarResultados();
                return Page();
            }

            var resultado = _calculoService.CalcularDescuentoDocumentoCompuesto(
                Input.ValorNominal!.Value,
                Input.TasaDescuentoAnual!.Value,
                Input.FechaEmision!.Value,
                Input.FechaVencimiento!.Value
            );

            DescuentoCalculado = resultado.Descuento.ToString("N2", CultureInfo.InvariantCulture);
            ValorActualCalculado = resultado.ValorActual.ToString("N2", CultureInfo.InvariantCulture);
            DiasCalculados = resultado.DiasAlVencimiento;
            ErrorMessage = null;

            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel { FechaEmision = DateTime.Today, FechaVencimiento = DateTime.Today.AddMonths(1) };
            LimpiarResultados();
            ErrorMessage = null;
            return Page();
        }

        private void LimpiarResultados()
        {
            DescuentoCalculado = null;
            ValorActualCalculado = null;
            DiasCalculados = null;
        }
    }
}
