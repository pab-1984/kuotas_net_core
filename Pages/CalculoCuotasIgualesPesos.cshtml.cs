// Kuotasmig.Core/Pages/CalculoCuotasIgualesPesos.cshtml.cs
using Kuotasmig.Core.Services; // Asegúrate que este sea el namespace de tu servicio
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic; // Para List<>
using System.ComponentModel.DataAnnotations;
using System.Globalization; // Para CultureInfo

namespace Kuotasmig.Core.Pages
{
    public class CalculoCuotasIgualesPesosModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CalculoCuotasIgualesPesosModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        // Para el resultado del cálculo individual
        public string? CuotaCalculadaIndividual { get; set; }
        public string? TasaEfectivaAnualEquivalente { get; set; }


        // Para la tabla de resultados
        public List<CalculoAmortizacionService.ResultadoCalculoCuota>? TablaDeCuotas { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El Capital Solicitado es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El capital debe ser mayor a cero.")]
            [Display(Name = "Capital Solicitado ($)")]
            public double? CapitalSolicitado { get; set; }

            [Required(ErrorMessage = "La Tasa de Interés Anual es obligatoria.")]
            [Display(Name = "Tasa de Interés Anual (%)")] // En el original era TXTTASA (asumo que es anual)
            public double? TasaInteresAnual { get; set; }

            [Required(ErrorMessage = "La Cantidad de Meses es obligatoria.")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad de meses debe ser al menos 1.")]
            [Display(Name = "Cantidad de Meses")]
            public int? CantidadMeses { get; set; } // Para el cálculo individual
        }

        public void OnGet() { }

        public IActionResult OnPostCalcularCuotaIndividual() // Handler para el primer botón
        {
            LimpiarResultadosIndividualesYTabla();
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
                CuotaCalculadaIndividual = resultado.MontoCuota.ToString("N2", CultureInfo.InvariantCulture);

                // Calcular Tasa Efectiva Anual Equivalente (como en el ASPX original)
                double tasaMensualEfectiva = Math.Pow(1.0 + (Input.TasaInteresAnual!.Value / 100.0), 1.0 / 12.0) - 1.0;
                double tea = (Math.Pow(1.0 + tasaMensualEfectiva, 12.0) - 1.0) * 100.0; // El original tenía una fórmula un poco diferente aquí, pero esto es TEA
                TasaEfectivaAnualEquivalente = tea.ToString("N7", CultureInfo.InvariantCulture) + "%";


                // Llamar al método que genera la tabla completa
                TablaDeCuotas = _calculoService.GenerarTablaDeCuotas(
                    Input.CapitalSolicitado!.Value,
                    Input.TasaInteresAnual!.Value
                );
            }
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel();
            LimpiarResultadosIndividualesYTabla();
            return Page();
        }

        private void LimpiarResultadosIndividualesYTabla()
        {
             CuotaCalculadaIndividual = null;
             TasaEfectivaAnualEquivalente = null;
             TablaDeCuotas = null;
             ErrorMessage = null;
        }
    }
}
