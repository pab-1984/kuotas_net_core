using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;

namespace Kuotasmig.Core.Pages
{
    public class DiasTranscurridos365Model : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public DiasTranscurridos365Model(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public int? DiasCalculados { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "La fecha 'Desde' es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha Desde")]
            public DateTime? FechaDesde { get; set; }

            [Required(ErrorMessage = "La fecha 'Hasta' es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha Hasta")]
            public DateTime? FechaHasta { get; set; }
        }

        public void OnGet()
        {
            // Inicializar con fechas de hoy, como en el original
            Input.FechaDesde = DateTime.Today;
            Input.FechaHasta = DateTime.Today;
        }

        public IActionResult OnPostCalcular()
        {
            if (!ModelState.IsValid)
            {
                DiasCalculados = null;
                return Page();
            }

            if (Input.FechaHasta!.Value < Input.FechaDesde!.Value)
            {
                ErrorMessage = "La 'Fecha Hasta' no puede ser anterior a la 'Fecha Desde'.";
                DiasCalculados = null;
                return Page();
            }

            DiasCalculados = _calculoService.CalcularDiasTranscurridosReales(Input.FechaDesde!.Value, Input.FechaHasta!.Value);
            ErrorMessage = null;
            
            return Page();
        }

        public IActionResult OnPostLimpiar()
        {
            ModelState.Clear();
            Input = new InputModel { FechaDesde = DateTime.Today, FechaHasta = DateTime.Today };
            DiasCalculados = null;
            ErrorMessage = null;
            return Page();
        }
    }
}
