using Kuotasmig.Core.Services; // Asumiendo que CalculoFinancieroService está aquí
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class CalculoDeInteresSimpleModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService; // CAMBIADO AQUÍ

        public CalculoDeInteresSimpleModel(CalculoAmortizacionService calculoService) // CAMBIADO AQUÍ
        {
            _calculoService = calculoService;
        }

        // --- Propiedades para el primer cálculo (Tiempo en Días) ---
        [BindProperty]
        public InputDiasModel? InputDias { get; set; } = new InputDiasModel();
        public string? ResultadoInteresDias { get; set; }

        // --- Propiedades para el segundo cálculo (Tiempo en Meses) ---
        [BindProperty]
        public InputMesesModel? InputMeses { get; set; } = new InputMesesModel();
        public string? ResultadoInteresMeses { get; set; }

        // --- Propiedades para el tercer cálculo (Tiempo en Años) ---
        [BindProperty]
        public InputAñosModel? InputAños { get; set; } = new InputAñosModel();
        public string? ResultadoInteresAños { get; set; }

        public string? ErrorMessage { get; set; }


        public class InputDiasModel
        {
            [Display(Name = "Capital ($)")]
            public double? Capital { get; set; }
            [Display(Name = "Tasa de Interés Anual (%)")]
            public double? TasaAnual { get; set; }
            [Display(Name = "Tiempo (Días)")]
            public int? TiempoDias { get; set; }
        }

        public class InputMesesModel
        {
            [Display(Name = "Capital ($)")]
            public double? Capital { get; set; }
            [Display(Name = "Tasa de Interés Anual (%)")]
            public double? TasaAnual { get; set; }
            [Display(Name = "Tiempo (Meses)")]
            public int? TiempoMeses { get; set; }
        }

        public class InputAñosModel
        {
            [Display(Name = "Capital ($)")]
            public double? Capital { get; set; }
            [Display(Name = "Tasa de Interés Anual (%)")]
            public double? TasaAnual { get; set; }
            [Display(Name = "Tiempo (Años)")]
            public int? TiempoAños { get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPostCalcularInteresDias()
        {
            if (InputDias?.Capital != null && InputDias?.TasaAnual != null && InputDias?.TiempoDias != null)
            {
                double resultado = _calculoService.CalcularInteresSimplePorDias(InputDias.Capital.Value, InputDias.TasaAnual.Value, InputDias.TiempoDias.Value);
                ResultadoInteresDias = resultado.ToString("N2", CultureInfo.InvariantCulture);
            }
            else ErrorMessage = "Faltan datos para el cálculo por días.";
            return Page();
        }

        public IActionResult OnPostCalcularInteresMeses()
        {
            if (InputMeses?.Capital != null && InputMeses?.TasaAnual != null && InputMeses?.TiempoMeses != null)
            {
                double resultado = _calculoService.CalcularInteresSimplePorMeses(InputMeses.Capital.Value, InputMeses.TasaAnual.Value, InputMeses.TiempoMeses.Value);
                ResultadoInteresMeses = resultado.ToString("N2", CultureInfo.InvariantCulture);
            }
            else ErrorMessage = "Faltan datos para el cálculo por meses.";
            return Page();
        }

        public IActionResult OnPostCalcularInteresAños()
        {
             if (InputAños?.Capital != null && InputAños?.TasaAnual != null && InputAños?.TiempoAños != null)
            {
                double resultado = _calculoService.CalcularInteresSimplePorAños(InputAños.Capital.Value, InputAños.TasaAnual.Value, InputAños.TiempoAños.Value);
                ResultadoInteresAños = resultado.ToString("N2", CultureInfo.InvariantCulture);
            }
            else ErrorMessage = "Faltan datos para el cálculo por años.";
            return Page();
        }

        public IActionResult OnPostLimpiar() // Un solo limpiar para todos o individuales?
        {
            ModelState.Clear();
            InputDias = new InputDiasModel();
            InputMeses = new InputMesesModel();
            InputAños = new InputAñosModel();
            ResultadoInteresDias = null;
            ResultadoInteresMeses = null;
            ResultadoInteresAños = null;
            ErrorMessage = null;
            return Page();
        }
    }
}
