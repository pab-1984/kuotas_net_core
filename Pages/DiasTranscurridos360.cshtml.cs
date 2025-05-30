// Kuotasmig.Core/Pages/DiasTranscurridos360.cshtml.cs
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;

namespace Kuotasmig.Core.Pages
{
    public class DiasTranscurridos360Model : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public DiasTranscurridos360Model(CalculoAmortizacionService calculoService)
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

            if (Input.FechaDesde!.Value > Input.FechaHasta!.Value) // Usamos ! porque ModelState es válido
            {
                ErrorMessage = "La 'Fecha Hasta' no puede ser anterior a la 'Fecha Desde'.";
                DiasCalculados = null;
                return Page();
            }

            DiasCalculados = _calculoService.CalcularDiasTranscurridosBase360(Input.FechaDesde!.Value, Input.FechaHasta!.Value);
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