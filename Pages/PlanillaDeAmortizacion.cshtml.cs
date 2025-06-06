// Kuotasmig.Core/Pages/PlanillaDeAmortizacion.cshtml.cs
using Kuotasmig.Core.Models;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

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
            [Range(0.000001, double.MaxValue, ErrorMessage = "La tasa debe ser mayor a cero.")]
            [Display(Name = "Tasa de Interés Anual (%)")]
            public double? TasaAnual { get; set; } // ***** VUELVE A SER double? *****

            [Required(ErrorMessage = "La cantidad de cuotas es obligatoria.")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad de cuotas debe ser al menos 1.")]
            [Display(Name = "Cantidad de Cuotas (Meses)")]
            public int? CantidadCuotas { get; set; }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostCalcular()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // El model binder, con la nueva configuración de Program.cs, debería haber parseado
            // correctamente el valor si venía con un punto. El JS en la vista asegura esto.
            // Por lo tanto, podemos usar .Value de forma segura después de la validación.
            var resultado = _calculoService.GenerarPlanilla(
                Input.Capital!.Value,
                Input.TasaAnual!.Value,
                Input.CantidadCuotas!.Value
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
                CuotaCalculada = resultado.CuotaCalculada.ToString("N2", CultureInfo.InvariantCulture);
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
        
        public IActionResult OnPostExportarExcel()
        {
            // Lógica pendiente
            MensajeError = "Funcionalidad de exportar a Excel pendiente de implementación.";
            // Para que la UI se actualice, podrías necesitar recargar los datos
            // si la exportación se hace en el mismo post.
            // Por ahora, solo mostramos el mensaje.
            if (ModelState.IsValid)
            {
                 OnPostCalcular(); // Volver a calcular para que la tabla no desaparezca
            }
            return Page();
        }
    }
}
