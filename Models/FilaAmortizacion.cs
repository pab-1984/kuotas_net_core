// Kuotasmig.Core/Models/FilaAmortizacion.cs
namespace Kuotasmig.Core.Models
{
    public class FilaAmortizacion
    {
        public string NumeroCuota { get; set; } = string.Empty;
        public string SaldoInicial { get; set; } = string.Empty;
        public string Cuota { get; set; } = string.Empty;
        public string Intereses { get; set; } = string.Empty;
        public string Amortizacion { get; set; } = string.Empty;
        public string SaldoCapital { get; set; } = string.Empty;
    }
}