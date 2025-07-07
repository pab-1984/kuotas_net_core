using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class EquivalenciaDeTasasModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public EquivalenciaDeTasasModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputTasaAnualModel InputAnual { get; set; } = new();
        [BindProperty]
        public InputTasaMensualModel InputMensual { get; set; } = new();
        [BindProperty]
        public InputTasaDiariaModel InputDiaria { get; set; } = new();
        [BindProperty]
        public InputTasaLinealModel InputLineal { get; set; } = new();
        [BindProperty]
        public InputTasaEfectivaModel InputEfectiva { get; set; } = new();

        public string? ResultadoEqAnual { get; set; }
        public string? ResultadoEqMensual { get; set; }
        public string? ResultadoEqDiaria { get; set; }
        public string? ResultadoEfectivaDesdeLineal { get; set; }
        public string? ResultadoLinealDesdeEfectiva { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputTasaAnualModel
        {
            [Display(Name = "Tasa Anual (%)")]
            public double? TasaAnual { get; set; }
            [Display(Name = "Días para Equivalencia")]
            public int? DiasEquivalencia { get; set; }
        }
        public class InputTasaMensualModel
        {
            [Display(Name = "Tasa Mensual (%)")]
            public double? TasaMensual { get; set; }
            [Display(Name = "Días Capitalización T. Mensual")]
            public int? DiasCapitalizacionMensual { get; set; }
        }
        public class InputTasaDiariaModel
        {
            [Display(Name = "Tasa Diaria (%)")]
            public double? TasaDiaria { get; set; }
        }
        public class InputTasaLinealModel
        {
            [Display(Name = "Tasa Lineal Anual (%)")]
            public double? TasaLinealAnual { get; set; }
        }
        public class InputTasaEfectivaModel
        {
            [Display(Name = "Tasa Efectiva Anual (%)")]
            public double? TasaEfectivaAnual { get; set; }
        }

        public void OnGet() { }

        public void OnPostCalcularEqAnual()
        {
            if (InputAnual?.TasaAnual != null && InputAnual?.DiasEquivalencia != null)
            {
                double resultado = _calculoService.CalcularEquivalenciaDesdeTasaAnual(InputAnual.TasaAnual.Value, InputAnual.DiasEquivalencia.Value);
                ResultadoEqAnual = resultado.ToString("N7", CultureInfo.InvariantCulture) + "%";
            }
            else ErrorMessage = "Faltan datos para cálculo de equivalencia anual.";
        }

        public void OnPostCalcularEqMensual()
        {
            if (InputMensual?.TasaMensual != null && InputMensual?.DiasCapitalizacionMensual != null)
            {
                double resultado = _calculoService.CalcularEquivalenciaDesdeTasaMensual(InputMensual.TasaMensual.Value, InputMensual.DiasCapitalizacionMensual.Value);
                ResultadoEqMensual = resultado.ToString("N7", CultureInfo.InvariantCulture) + "%";
            }
            else ErrorMessage = "Faltan datos para cálculo de equivalencia mensual.";
        }

        public void OnPostCalcularEqDiaria()
        {
            if (InputDiaria?.TasaDiaria != null)
            {
                double resultado = _calculoService.CalcularEquivalenciaDesdeTasaDiaria(InputDiaria.TasaDiaria.Value);
                ResultadoEqDiaria = resultado.ToString("N7", CultureInfo.InvariantCulture) + "%";
            }
            else ErrorMessage = "Falta Tasa Diaria para cálculo.";
        }
        
        public void OnPostCalcularEfectivaDesdeLineal()
        {
            if (InputLineal?.TasaLinealAnual != null)
            {
                double resultado = _calculoService.ConvertirLinealAnualAEfectivaAnual(InputLineal.TasaLinealAnual.Value);
                ResultadoEfectivaDesdeLineal = resultado.ToString("N7", CultureInfo.InvariantCulture) + "%";
            }
            else ErrorMessage = "Falta Tasa Lineal Anual para cálculo.";
        }

        public void OnPostCalcularLinealDesdeEfectiva()
        {
            if (InputEfectiva?.TasaEfectivaAnual != null)
            {
                double resultado = _calculoService.ConvertirEfectivaAnualALinealAnual(InputEfectiva.TasaEfectivaAnual.Value);
                ResultadoLinealDesdeEfectiva = resultado.ToString("N7", CultureInfo.InvariantCulture) + "%";
            }
             else ErrorMessage = "Falta Tasa Efectiva Anual para cálculo.";
        }

        public IActionResult OnPostLimpiarTodo()
        {
            ModelState.Clear();
            return RedirectToPage(); // Redirige a la misma página para limpiar todo
        }
    }
}