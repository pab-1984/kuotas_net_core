// Kuotasmig.Core/Pages/CalculoDeInteresCompuesto.cshtml.cs
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class CalculoDeInteresCompuestoModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CalculoDeInteresCompuestoModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        // --- Modelo para el primer cálculo (Tasa Mensual) ---
        [BindProperty(SupportsGet = true, Name = "InputMensual")] // SupportsGet para mantener valores si se refresca
        public InputTasaMensualModel InputMensual { get; set; } = new InputTasaMensualModel();
        public string? TotalCalculoMensual { get; set; }
        public string? InteresesCalculoMensual { get; set; }
        public int? DiasCalculadosMensual { get; set; }


        // --- Modelo para el segundo cálculo (Tasa Anual) ---
        [BindProperty(SupportsGet = true, Name = "InputAnual")]
        public InputTasaAnualModel InputAnual { get; set; } = new InputTasaAnualModel();
        public string? TotalCalculoAnual { get; set; }
        public string? InteresesCalculoAnual { get; set; }
        public string? TasaMensualEquivalenteAnual { get; set; }
        public int? DiasCalculadosAnual { get; set; }
        
        public string? ErrorMessage { get; set; }


        public class InputTasaMensualModel
        {
            [Display(Name = "Capital ($)")]
            public double? Capital { get; set; }

            [Display(Name = "Tasa de Interés Mensual (%)")]
            public double? TasaInteresMensual { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Fecha Desde")]
            public DateTime? FechaDesde { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Fecha Hasta")]
            public DateTime? FechaHasta { get; set; }
        }

        public class InputTasaAnualModel
        {
            [Display(Name = "Capital ($)")]
            public double? Capital { get; set; }

            [Display(Name = "Tasa de Interés Anual (%)")]
            public double? TasaInteresAnual { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Fecha Desde")]
            public DateTime? FechaDesde { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Fecha Hasta")]
            public DateTime? FechaHasta { get; set; }
        }


        public void OnGet()
        {
            InputMensual.FechaDesde = DateTime.Today;
            InputMensual.FechaHasta = DateTime.Today;
            InputAnual.FechaDesde = DateTime.Today;
            InputAnual.FechaHasta = DateTime.Today;
        }

        public IActionResult OnPostCalcularMensual()
        {
            DiasCalculadosMensual = null; // Reset
            TotalCalculoMensual = null;
            InteresesCalculoMensual = null;

            if (InputMensual.Capital == null || InputMensual.TasaInteresMensual == null || 
                InputMensual.FechaDesde == null || InputMensual.FechaHasta == null)
            {
                ErrorMessage = "Todos los campos son obligatorios para el cálculo con tasa mensual.";
                return Page();
            }
             if (InputMensual.FechaDesde.Value > InputMensual.FechaHasta.Value)
            {
                ErrorMessage = "La 'Fecha Hasta' no puede ser anterior a la 'Fecha Desde' para el cálculo con tasa mensual.";
                return Page();
            }


            TimeSpan diferencia = InputMensual.FechaHasta.Value.Subtract(InputMensual.FechaDesde.Value);
            DiasCalculadosMensual = diferencia.Days;

            var resultado = _calculoService.CalcularInteresCompuesto(
                InputMensual.Capital.Value,
                InputMensual.TasaInteresMensual.Value, // Tasa ya es mensual
                DiasCalculadosMensual.Value
            );

            TotalCalculoMensual = resultado.MontoFinal.ToString("N2", CultureInfo.InvariantCulture);
            InteresesCalculoMensual = resultado.InteresesDevengados.ToString("N2", CultureInfo.InvariantCulture);
            ErrorMessage = null;
            return Page();
        }

        public IActionResult OnPostCalcularAnual()
        {
            DiasCalculadosAnual = null; // Reset
            TotalCalculoAnual = null;
            InteresesCalculoAnual = null;
            TasaMensualEquivalenteAnual = null;

            if (InputAnual.Capital == null || InputAnual.TasaInteresAnual == null || 
                InputAnual.FechaDesde == null || InputAnual.FechaHasta == null)
            {
                ErrorMessage = "Todos los campos son obligatorios para el cálculo con tasa anual.";
                return Page();
            }
            if (InputAnual.FechaDesde.Value > InputAnual.FechaHasta.Value)
            {
                ErrorMessage = "La 'Fecha Hasta' no puede ser anterior a la 'Fecha Desde' para el cálculo con tasa anual.";
                return Page();
            }

            TimeSpan diferencia = InputAnual.FechaHasta.Value.Subtract(InputAnual.FechaDesde.Value);
            DiasCalculadosAnual = diferencia.Days;

            // Convertir Tasa Anual a Tasa Mensual Efectiva
            double tasaAnual = InputAnual.TasaInteresAnual.Value / 100.0;
            double tasaMensualEfectiva = (Math.Pow(1.0 + tasaAnual, 1.0 / 12.0) - 1.0) * 100.0;
            TasaMensualEquivalenteAnual = tasaMensualEfectiva.ToString("N7", CultureInfo.InvariantCulture) + "%";


            var resultado = _calculoService.CalcularInteresCompuesto(
                InputAnual.Capital.Value,
                tasaMensualEfectiva,
                DiasCalculadosAnual.Value
            );

            TotalCalculoAnual = resultado.MontoFinal.ToString("N2", CultureInfo.InvariantCulture);
            InteresesCalculoAnual = resultado.InteresesDevengados.ToString("N2", CultureInfo.InvariantCulture);
            ErrorMessage = null;
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            InputMensual = new InputTasaMensualModel { FechaDesde = DateTime.Today, FechaHasta = DateTime.Today };
            InputAnual = new InputTasaAnualModel { FechaDesde = DateTime.Today, FechaHasta = DateTime.Today };
            TotalCalculoMensual = null;
            InteresesCalculoMensual = null;
            DiasCalculadosMensual = null;
            TotalCalculoAnual = null;
            InteresesCalculoAnual = null;
            TasaMensualEquivalenteAnual = null;
            DiasCalculadosAnual = null;
            ErrorMessage = null;
            return Page();
        }
    }
}