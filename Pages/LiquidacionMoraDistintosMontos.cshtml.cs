// Kuotasmig.Core/Pages/LiquidacionMoraDistintosMontos.cshtml.cs
using Kuotasmig.Core.Models;
using Kuotasmig.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Kuotasmig.Core.Pages
{
    public class LiquidacionMoraDistintosMontosModel : PageModel
    {
        private readonly CalculoAmortizacionService _calculoService;

        public LiquidacionMoraDistintosMontosModel(CalculoAmortizacionService calculoService)
        {
            _calculoService = calculoService;
        }

        [BindProperty]
        public GlobalInputModel GlobalInputs { get; set; } = new GlobalInputModel();

        [BindProperty]
        public List<ItemLiquidacionMora> Items { get; set; } = new List<ItemLiquidacionMora>();

        public CalculoAmortizacionService.ResultadoTablaMoraCuotas? Resultado { get; set; }

        public double SubtotalMontos { get; set; }
        public double SubtotalMora { get; set; }
        public double TotalGeneral { get; set; }
        
        public string? TasaMensualCalculada { get; set; }
        public string? TasaDiariaCalculada { get; set; }


        public class GlobalInputModel
        {
            [Required(ErrorMessage = "La Fecha de Actualización es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha de Actualización/Pago")]
            public DateTime? FechaActualizacion { get; set; } = DateTime.Today;

            [Required(ErrorMessage = "La Tasa de Mora Anual es obligatoria.")]
            [Display(Name = "Tasa de Interés por Mora Anual (%)")]
            public double? TasaMoraAnual { get; set; }

            [Display(Name = "Tasa de Interés por Mora Mensual (%)")]
            public double? TasaMoraMensual { get; set; } // Se calculará

            [Display(Name = "Tasa de Interés por Mora Diaria (factor)")]
            public double? TasaMoraDiariaFactor { get; set; } // Se calculará
            
            [Range(1, 20, ErrorMessage = "Puede agregar entre 1 y 20 filas a la vez.")]
            [Display(Name = "Nº Filas a Agregar")]
            public int FilasAAgregar { get; set; } = 1;
        }

        public void OnGet()
        {
            if (!Items.Any())
            {
                // Añadir algunas filas por defecto como en el original
                for (int i = 0; i < 6; i++)
                {
                    Items.Add(new ItemLiquidacionMora());
                }
            }
        }

        public IActionResult OnPostCalcularTodo()
        {
            SubtotalMontos = 0;
            SubtotalMora = 0;
            TotalGeneral = 0;

            if (!ModelState.IsValid) // Valida GlobalInputs y la lista de Items
            {
                // Si hay errores en los items, se mostrarán en la tabla
                return Page();
            }
            
            ConvertirTasas(); // Calcular tasas mensual y diaria

            foreach (var item in Items)
            {
                if (item.Monto.HasValue && item.FechaDesde.HasValue && GlobalInputs.FechaActualizacion.HasValue && GlobalInputs.TasaMoraAnual.HasValue)
                {
                    var resultadoItem = _calculoService.CalcularMoraParaItem(
                        item.Monto.Value,
                        item.FechaDesde.Value,
                        GlobalInputs.FechaActualizacion.Value,
                        GlobalInputs.TasaMoraAnual.Value
                    );
                    item.DiasAtraso = resultadoItem.DiasAtraso;
                    item.MontoMora = resultadoItem.MontoMora;
                    item.TotalConMora = resultadoItem.TotalConMora;

                    SubtotalMontos += item.Monto.Value;
                    SubtotalMora += item.MontoMora;
                    TotalGeneral += item.TotalConMora;
                }
            }
            return Page();
        }

        public IActionResult OnPostAgregarFilas()
        {
            ConvertirTasas(); // Mantener las tasas calculadas
            int filasNuevas = GlobalInputs.FilasAAgregar > 0 ? GlobalInputs.FilasAAgregar : 1;
            for (int i = 0; i < filasNuevas; i++)
            {
                Items.Add(new ItemLiquidacionMora());
            }
            return Page();
        }
         public IActionResult OnPostQuitarFila(Guid itemId)
        {
            ConvertirTasas();
            var itemToRemove = Items.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
            }
            // Recalcular si es necesario o simplemente actualizar la UI
            // OnPostCalcularTodo(); // Opcional: recalcular todo
            return Page();
        }


        public IActionResult OnPostLimpiarTodo()
        {
            ModelState.Clear();
            GlobalInputs = new GlobalInputModel();
            Items = new List<ItemLiquidacionMora>();
            for (int i = 0; i < 6; i++) // Volver a añadir filas por defecto
            {
                Items.Add(new ItemLiquidacionMora());
            }
            SubtotalMontos = 0;
            SubtotalMora = 0;
            TotalGeneral = 0;
            TasaMensualCalculada = null;
            TasaDiariaCalculada = null;
            return Page();
        }

        // Este handler se llama cuando cambia TXTTASAANUALMORA
        public void OnPostActualizarTasasAnual()
        {
            if (GlobalInputs.TasaMoraAnual.HasValue)
            {
                double tna = GlobalInputs.TasaMoraAnual.Value / 100.0;
                // TEM = (1+TNA/12) - No, es (1+TEA)^(1/12)-1
                // Si TNA es cap mensual: Tasa mensual = TNA/12
                // Si TNA es efectiva anual para el cálculo de mora diaria:
                double tasaDiariaFactor = Math.Pow(1.0 + tna, 1.0 / 365.0); // Factor (1+i_d)
                GlobalInputs.TasaMoraDiariaFactor = Math.Round(tasaDiariaFactor, 9);
                
                double tasaMensualEquivalente = (Math.Pow(tasaDiariaFactor, 30) - 1.0) * 100.0; // TEM %
                GlobalInputs.TasaMoraMensual = Math.Round(tasaMensualEquivalente, 7);

                TasaMensualCalculada = GlobalInputs.TasaMoraMensual?.ToString("N7", CultureInfo.InvariantCulture) + "%";
                TasaDiariaCalculada = GlobalInputs.TasaMoraDiariaFactor?.ToString("N9", CultureInfo.InvariantCulture);

            }
            // Mantener los items
             OnPostCalcularTodo(); // Recalcular todo con la nueva tasa
        }

        // Este handler se llama cuando cambia TXTTASAMENSUALMORA
        public void OnPostActualizarTasasMensual()
        {
             if (GlobalInputs.TasaMoraMensual.HasValue)
            {
                double tem = GlobalInputs.TasaMoraMensual.Value / 100.0;
                // TEA = (1+TEM)^12 - 1
                double tasaAnualEquivalente = (Math.Pow(1.0 + tem, 12.0) - 1.0) * 100.0;
                GlobalInputs.TasaMoraAnual = Math.Round(tasaAnualEquivalente, 7);

                double tasaDiariaFactor = Math.Pow(1.0 + tem, 1.0 / 30.0); // Factor (1+i_d)
                GlobalInputs.TasaMoraDiariaFactor = Math.Round(tasaDiariaFactor, 9);

                TasaMensualCalculada = GlobalInputs.TasaMoraMensual?.ToString("N7", CultureInfo.InvariantCulture) + "%";
                TasaDiariaCalculada = GlobalInputs.TasaMoraDiariaFactor?.ToString("N9", CultureInfo.InvariantCulture);
            }
            // Mantener los items
             OnPostCalcularTodo(); // Recalcular todo con la nueva tasa
        }
        private void ConvertirTasas()
        {
             // Similar a OnPostActualizarTasasAnual, pero sin llamar a Page()
            if (GlobalInputs.TasaMoraAnual.HasValue && GlobalInputs.TasaMoraMensual == null) // Calcular mensual y diaria desde anual
            {
                double tna = GlobalInputs.TasaMoraAnual.Value / 100.0;
                double tasaDiariaFactor = Math.Pow(1.0 + tna, 1.0 / 365.0);
                GlobalInputs.TasaMoraDiariaFactor = Math.Round(tasaDiariaFactor, 9);
                
                double tasaMensualEquivalente = (Math.Pow(tasaDiariaFactor, 30) - 1.0) * 100.0;
                GlobalInputs.TasaMoraMensual = Math.Round(tasaMensualEquivalente, 7);
            }
            else if (GlobalInputs.TasaMoraMensual.HasValue && GlobalInputs.TasaMoraAnual == null) // Calcular anual y diaria desde mensual
            {
                 double tem = GlobalInputs.TasaMoraMensual.Value / 100.0;
                double tasaAnualEquivalente = (Math.Pow(1.0 + tem, 12.0) - 1.0) * 100.0;
                GlobalInputs.TasaMoraAnual = Math.Round(tasaAnualEquivalente, 7);

                double tasaDiariaFactor = Math.Pow(1.0 + tem, 1.0 / 30.0);
                GlobalInputs.TasaMoraDiariaFactor = Math.Round(tasaDiariaFactor, 9);
            }
             TasaMensualCalculada = GlobalInputs.TasaMoraMensual?.ToString("N7", CultureInfo.InvariantCulture) + "%";
             TasaDiariaCalculada = GlobalInputs.TasaMoraDiariaFactor?.ToString("N9", CultureInfo.InvariantCulture);
        }

    }
}