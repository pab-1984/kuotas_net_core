// Kuotasmig.Core/Models/ItemLiquidacionMora.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Kuotasmig.Core.Models
{
    public class ItemLiquidacionMora
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Para identificar filas unívocamente

        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty; // Inicializar para evitar warnings

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser positivo.")]
        [Display(Name = "Monto ($)")]
        public double? Monto { get; set; } // Anulable para que [Required] funcione bien

        [Required(ErrorMessage = "La fecha 'Desde' es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Vto./Desde")]
        public DateTime? FechaDesde { get; set; } // Anulable para que [Required] funcione bien

        // Propiedades para los resultados calculados por fila
        public int DiasAtraso { get; set; }
        public double MontoMora { get; set; }
        public double TotalConMora { get; set; }

        // Constructor por defecto para inicializar valores si es necesario
        public ItemLiquidacionMora()
        {
            FechaDesde = DateTime.Today; // Valor por defecto para nuevas filas
        }
    }
}