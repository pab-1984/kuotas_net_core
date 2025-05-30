// Kuotasmig.Core/Pages/PlanillaDeAmortizacion.cshtml.cs
using Kuotasmig.Core.Models;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kuotasmig.Core.Pages
{
    public class PlanillaDeAmortizacionModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public PlanillaDeAmortizacionModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public List<FilaAmortizacion>? Planilla { get; set; }
        public string? CuotaCalculada { get; set; }
        public string? MensajeError { get; set; }


        public class InputModel
        {
            [Required(ErrorMessage = "El capital es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El capital debe ser mayor a cero.")]
            [Display(Name = "Capital ($ o U.I.)")]
            public double? Capital { get; set; }

            [Required(ErrorMessage = "La tasa anual es obligatoria.")]
            [Range(0.000001, double.MaxValue, ErrorMessage = "La tasa debe ser mayor a cero.")] // No puede ser 0 exacto para la fórmula estándar
            [Display(Name = "Tasa de Interés Anual (%)")]
            public double? TasaAnual { get; set; }

            [Required(ErrorMessage = "La cantidad de cuotas es obligatoria.")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad de cuotas debe ser al menos 1.")]
            [Display(Name = "Cantidad de Cuotas (Meses)")]
            public int? CantidadCuotas { get; set; }
        }

        public void OnGet()
        {
            // Lógica para cuando la página se carga por GET, si es necesaria
        }

        public IActionResult OnPostCalcular()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = _calculoService.GenerarPlanilla(
                Input.Capital.Value,
                Input.TasaAnual.Value,
                Input.CantidadCuotas.Value
            );

            if (resultado.Error)
            {
                MensajeError = resultado.MensajeError;
                Planilla = null;
                CuotaCalculada = null;
            }
            else
            {
                Planilla = resultado.Planilla;
                CuotaCalculada = resultado.CuotaCalculada.ToString("N2");
                MensajeError = null;
            }
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel();
            Planilla = null;
            CuotaCalculada = null;
            MensajeError = null;
            return Page();
        }

        // La exportación a Excel requerirá una biblioteca de terceros o un enfoque diferente.
        // No se puede replicar directamente el método ExportToExcel de Web Forms.
        // Podrías generar un archivo CSV o usar una biblioteca como EPPlus o ClosedXML.
        // Por ahora, esta funcionalidad quedará pendiente.
        public IActionResult OnPostExportarExcel()
        {
            // Lógica de exportación a implementar
            // Ejemplo rápido de CSV (requeriría más trabajo para formato Excel):
            if (Planilla == null || !Planilla.Any())
            {
                 TempData["ErrorExportacion"] = "No hay datos para exportar.";
                return Page();
            }
            // Esta es una implementación muy básica.
            // En una app real, usarías una librería para Excel (EPPlus, ClosedXML) o CSVHelper
            // ... (Lógica para generar el archivo) ...
            // return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlanillaAmortizacion.xlsx");

            MensajeError = "Funcionalidad de exportar a Excel pendiente de implementación completa.";
            return Page();
        }
    }
}