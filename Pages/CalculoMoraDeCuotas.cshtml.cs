// Kuotasmig.Core/Pages/CalculoMoraDeCuotas.cshtml.cs
using Kuotasmig.Core.Models;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kuotasmig.Core.Pages
{
    public class CalculoMoraDeCuotasModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public CalculoMoraDeCuotasModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public CalculoAmortizacionService.ResultadoTablaMoraCuotas? Resultado { get; set; }
        // ErrorMessage ahora está dentro de Resultado.MensajeError

        public class InputModel
        {
            [Required(ErrorMessage = "El Monto de Cuota es obligatorio.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
            [Display(Name = "Monto de Cada Cuota ($)")]
            public double? MontoCadaCuota { get; set; }

            [Required(ErrorMessage = "La Cantidad de Cuotas es obligatoria.")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad de cuotas debe ser al menos 1.")]
            [Display(Name = "Cantidad de Cuotas")]
            public int? CantidadTotalCuotas { get; set; }

            [Required(ErrorMessage = "La Tasa de Mora Anual es obligatoria.")]
            [Display(Name = "Tasa de Interés por Mora Anual (%)")]
            public double? TasaMoraAnual { get; set; } // Puede ser 0

            [Required(ErrorMessage = "La Fecha de Vencimiento de la 1ra Cuota es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha Vencimiento 1ra Cuota")]
            public DateTime? FechaVencimientoPrimeraCuota { get; set; }

            [Required(ErrorMessage = "La Fecha de Actualización/Pago es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha de Actualización/Pago")]
            public DateTime? FechaActualizacion { get; set; }
        }

        public void OnGet()
        {
            Input.FechaVencimientoPrimeraCuota = DateTime.Today.AddMonths(-1); // Ejemplo
            Input.FechaActualizacion = DateTime.Today;
        }

        public IActionResult OnPostCalcular()
        {
            Resultado = null; 
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Resultado = _calculoService.GenerarTablaCalculoMoraCuotas(
                Input.MontoCadaCuota!.Value,
                Input.CantidadTotalCuotas!.Value,
                Input.TasaMoraAnual!.Value,
                Input.FechaVencimientoPrimeraCuota!.Value,
                Input.FechaActualizacion!.Value
            );
            
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel { FechaVencimientoPrimeraCuota = DateTime.Today.AddMonths(-1), FechaActualizacion = DateTime.Today };
            Resultado = null;
            return Page();
        }
    }
}
