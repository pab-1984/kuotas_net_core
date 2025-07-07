// Kuotasmig.Core/Pages/CalculoCuotasUIYPesos.cshtml.cs
using Kuotasmig.Core.Models;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class CalculoCuotasUIYPesosModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CalculoCuotasUIYPesosModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public CalculoAmortizacionService.CalculoCuotasUIYPesosGeneral? Resultado { get; set; }
        // ErrorMessage ahora está dentro de Resultado, pero puedes tener uno a nivel de página si prefieres
        // public string? ErrorMessage { get; set; }


        public class InputModel
        {
            [Required(ErrorMessage = "El Capital en Dólares es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El capital debe ser mayor a cero.")]
            [Display(Name = "Capital en Dólares (U$S)")]
            public double? CapitalDolares { get; set; }

            [Required(ErrorMessage = "La Tasa de Interés Anual es obligatoria.")]
            [Display(Name = "Tasa de Interés Anual Nominal (%)")]
            public double? TasaInteresAnual { get; set; }

            [Required(ErrorMessage = "La Cantidad de Meses es obligatoria.")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad de meses debe ser al menos 1.")]
            [Display(Name = "Cantidad de Meses (para cuota individual)")]
            public int? CantidadMesesIndividual { get; set; }
            
            [Required(ErrorMessage = "La Cotización del Dólar es obligatoria.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "La cotización debe ser mayor a cero.")]
            [Display(Name = "Cotización Dólar a Pesos ($)")]
            public double? CotizacionDolarAPesos { get; set; }

            [Required(ErrorMessage = "La Cotización de la UI es obligatoria.")]
            [Range(0.000001, double.MaxValue, ErrorMessage = "La cotización debe ser mayor a cero.")]
            [Display(Name = "Cotización UI a Pesos ($)")]
            public double? CotizacionUIApesos { get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPostCalcular()
        {
            Resultado = null; // Limpiar resultado previo
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Resultado = _calculoService.CalcularCuotasUIYPesosCompleto(
                Input.CapitalDolares!.Value,
                Input.TasaInteresAnual!.Value,
                Input.CantidadMesesIndividual!.Value,
                Input.CotizacionDolarAPesos!.Value,
                Input.CotizacionUIApesos!.Value
            );
            
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel();
            Resultado = null;
            return Page();
        }
    }
}