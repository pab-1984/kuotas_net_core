// Kuotasmig.Core/Models/FilaMoraCuota.cs
namespace Kuotasmig.Core.Models
{
    public class FilaMoraCuota
    {
        public int NumeroCuota { get; set; }
        public string FechaCuotaVencida { get; set; } = string.Empty;
        public string FechaActualizacion { get; set; } = string.Empty;
        public int DiasAtraso { get; set; }
        public string MontoCuota { get; set; } = string.Empty;
        public string MontoMora { get; set; } = string.Empty;
        public string TotalConMora { get; set; } = string.Empty;
    }
}