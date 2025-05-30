// Kuotasmig.Core/Models/ResultadoCuotaUIYPesos.cs
namespace Kuotasmig.Core.Models
{
    public class ResultadoCuotaUIYPesos
    {
        public double CapitalEnDolares { get; set; }
        public int Meses { get; set; }
        public string CuotaEnUI { get; set; } = string.Empty;
        public string CuotaEnPesos { get; set; } = string.Empty;
        // Podrías añadir más propiedades si fueran necesarias para la visualización
    }
}