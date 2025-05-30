// Kuotasmig.Core/Services/CalculoAmortizacionService.cs
using Kuotasmig.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq; // Necesario para .Sum() si lo usas

namespace Kuotasmig.Core.Services
{
    public class CalculoAmortizacionService // Puedes renombrar esta clase a CalculoFinancieroService si prefieres más adelante
    {
        // Dentro de la clase CalculoAmortizacionService (o tu servicio financiero)

        public class ResultadoDescuentoDocumento
        {
            public double Descuento { get; set; }
            public double ValorActual { get; set; }
            public int DiasAlVencimiento { get; set; } // Para mostrarlo en la UI
        }

        public ResultadoDescuentoDocumento CalcularDescuentoDocumentoCompuesto(double valorNominal, double tasaDescuentoAnual, DateTime fechaEmision, DateTime fechaVencimiento)
        {
            Console.WriteLine($"DESCUENTO DOCUMENTO (Servicio): VN={valorNominal}, TasaDescAnual={tasaDescuentoAnual}, Emision={fechaEmision.ToShortDateString()}, Venc={fechaVencimiento.ToShortDateString()}");

            if (valorNominal <= 0 || tasaDescuentoAnual < 0) // La tasa podría ser 0
            {
                return new ResultadoDescuentoDocumento { Descuento = 0, ValorActual = valorNominal, DiasAlVencimiento = 0 };
            }
            if (fechaVencimiento < fechaEmision)
            {
                // Considerar devolver un error o un resultado indicativo
                return new ResultadoDescuentoDocumento { Descuento = 0, ValorActual = valorNominal, DiasAlVencimiento = 0 };
            }

            TimeSpan diferencia = fechaVencimiento.Subtract(fechaEmision);
            int diasAlVencimiento = diferencia.Days;

            if (diasAlVencimiento == 0) // Si la fecha de emisión y vencimiento son la misma
            {
                return new ResultadoDescuentoDocumento { Descuento = 0, ValorActual = valorNominal, DiasAlVencimiento = 0 };
            }

            // Convertir tasa de descuento anual a tasa de descuento diaria
            // d_diaria = 1 - (1 - TasaDescAnual/100)^(1/360) (asumiendo año 360 días para descuento)
            // O si la tasa es efectiva anual: (1 + TasaAnual/100)^(1/360) - 1 para el factor de descuento
            // La fórmula original usaba: Math.Pow((1 + TXTTASADEDESCUENTOANUAL.Text.ToDouble() / 100), (1 / 360))
            // Esto es un factor de capitalización diario. El descuento se calcula como:
            // D = VN * (1 - (1+i_diaria)^-n)  o VA = VN * (1+i_diaria)^-n
            // El original parece usar: CoeficienteDiario = Math.Pow((1 + TasaAnual / 100), (1 / (double)360));
            // Descuento = VN * (1 - Math.Pow(CoeficienteDiario, -ndias));

            double tasaAnual = tasaDescuentoAnual / 100.0;
            double coeficienteDiario = Math.Pow(1.0 + tasaAnual, 1.0 / 360.0); // Asumiendo año comercial

            double descuentoCalculado;
            double valorActualCalculado;

            if (coeficienteDiario == 1) // Si la tasa es 0%
            {
                descuentoCalculado = 0;
                valorActualCalculado = valorNominal;
            }
            else
            {
                // VA = VN * (coeficienteDiario)^-diasAlVencimiento
                valorActualCalculado = valorNominal * Math.Pow(coeficienteDiario, -diasAlVencimiento);
                descuentoCalculado = valorNominal - valorActualCalculado;
            }

            return new ResultadoDescuentoDocumento
            {
                Descuento = descuentoCalculado,
                ValorActual = valorActualCalculado,
                DiasAlVencimiento = diasAlVencimiento
            };
        }
        public class ResultadoInteresCompuesto
        {
            public double MontoFinal { get; set; }
            public double InteresesDevengados { get; set; }
        }

        public ResultadoInteresCompuesto CalcularInteresCompuesto(double capital, double tasaInteresMensualEfectiva, int numeroDeDias)
        {
            Console.WriteLine($"INTERÉS COMPUESTO (Servicio): C={capital}, TasaMensualEfec={tasaInteresMensualEfectiva}, Días={numeroDeDias}");

            if (capital <= 0 || numeroDeDias < 0) // La tasa puede ser 0
            {
                // Devolver el capital inicial si no hay días o el capital es cero/negativo
                return new ResultadoInteresCompuesto { MontoFinal = capital, InteresesDevengados = 0 };
            }

            // Convertir la tasa de interés mensual efectiva a una tasa diaria efectiva
            // Tasa Diaria = (1 + TasaMensualEfectiva)^(1/30) - 1
            // (Asumiendo 30 días por mes para consistencia con cómo se calculó la tasa mensual originalmente)
            double tasaDiariaEfectiva = Math.Pow(1.0 + (tasaInteresMensualEfectiva / 100.0), 1.0 / 30.0) - 1.0;

            // Si la tasa diaria efectiva es negativa (lo que podría pasar si la tasa mensual es -100%),
            // el monto final podría ser menor que el capital o incluso cero.
            // Si tasaDiariaEfectiva es -1 (tasa mensual -100%), Math.Pow daría 0.

            double montoFinal;
            if (tasaDiariaEfectiva <= -1) // Evitar problemas con Math.Pow si la base es <= 0
            {
                montoFinal = 0; // Si la tasa es -100% o peor, el capital se pierde.
            }
            else
            {
                montoFinal = capital * Math.Pow(1.0 + tasaDiariaEfectiva, numeroDeDias);
            }

            double interesesDevengados = montoFinal - capital;

            return new ResultadoInteresCompuesto
            {
                MontoFinal = montoFinal,
                InteresesDevengados = interesesDevengados
            };
        }
        public double CalcularRendimientoAnualReal(double tasaAnualFacial, double numeroDePagos)
        {
            Console.WriteLine($"RENDIMIENTO ANUAL REAL (Servicio): TasaFacial={tasaAnualFacial}, Pagos={numeroDePagos}");
            if (numeroDePagos == 0)
            {

                return 0;
            }
            return (100 * tasaAnualFacial) / numeroDePagos;
        }
        public int CalcularDiasTranscurridosBase360(DateTime fechaDesde, DateTime fechaHasta)
        {
            Console.WriteLine($"DÍAS 360 (Servicio): Desde={fechaDesde.ToShortDateString()}, Hasta={fechaHasta.ToShortDateString()}");

            // Replicando la lógica original
            double años = fechaHasta.Year - fechaDesde.Year;
            double meses = fechaHasta.Month - fechaDesde.Month;
            double dias = fechaHasta.Day - fechaDesde.Day;

            meses = meses + (años * 12);
            // La lógica original para días parecía intentar ajustar a múltiplos de 30.
            // Una forma común de 30/360 es: (Año2 - Año1) * 360 + (Mes2 - Mes1) * 30 + (Día2 - Día1)
            // Con ajustes si Día1 o Día2 son 31 (se tratan como 30) o si Día2 es el último de febrero.
            // Para simplificar y seguir tu lógica original aproximada:
            dias = dias + (meses * 30); 
            // La línea original "dias = ((((CDIAS * 100)/30)*30)/100)" parece que no tiene un efecto significativo
            // si CDIAS ya es un entero. Simplemente `dias = dias + (meses * 30);` es lo que hace.

            // Implementación más estándar del método 30E/360 (Europeo)
            int d1 = fechaDesde.Day;
            int m1 = fechaDesde.Month;
            int y1 = fechaDesde.Year;
            int d2 = fechaHasta.Day;
            int m2 = fechaHasta.Month;
            int y2 = fechaHasta.Year;

            if (d1 == 31) d1 = 30;
            if (d2 == 31) d2 = 30;

            return ((y2 - y1) * 360) + ((m2 - m1) * 30) + (d2 - d1);
        }
        // Clase interna para el resultado de la planilla de amortización
        public class ResultadoCalculoAmortizacion
        {
            public double CuotaCalculada { get; set; }
            public List<FilaAmortizacion> Planilla { get; set; } = new List<FilaAmortizacion>();
            public bool Error { get; set; } = false;
            public string? MensajeError { get; set; } // Permite null
        }

        public ResultadoCalculoAmortizacion GenerarPlanilla(double capital, double tasaAnualNominal, int cantidadCuotas)
        {
            var resultado = new ResultadoCalculoAmortizacion();

            if (capital <= 0 || tasaAnualNominal < 0 || cantidadCuotas <= 0) // Permitir tasa 0
            {
                resultado.Error = true;
                resultado.MensajeError = "Capital y cantidad de cuotas deben ser mayores a cero. La tasa debe ser mayor o igual a cero.";
                if (tasaAnualNominal < 0 && capital > 0 && cantidadCuotas > 0) // Caso especial para evitar error si solo la tasa es el problema
                {
                     resultado.MensajeError = "La tasa anual nominal no puede ser negativa.";
                }
                return resultado;
            }

            try
            {
                double tasaMensualEfectiva;
                if (tasaAnualNominal == 0)
                {
                    tasaMensualEfectiva = 0;
                }
                else
                {
                    tasaMensualEfectiva = Math.Pow(1.0 + (tasaAnualNominal / 100.0), 1.0 / 12.0) - 1.0;
                }


                if (Math.Abs(tasaMensualEfectiva) < 0.0000000001) // Comparación más segura para tasa cero o muy cercana
                {
                    resultado.CuotaCalculada = Math.Round(capital / cantidadCuotas, 2);
                    double saldoPendiente = capital;
                    for (int i = 1; i <= cantidadCuotas; i++)
                    {
                        string saldoInicialActualParaFila = saldoPendiente.ToString("N2", CultureInfo.InvariantCulture);
                        double intereses = 0;
                        double amortizacionEstaCuota = resultado.CuotaCalculada;
                        
                        if (i == cantidadCuotas) {
                             amortizacionEstaCuota = saldoPendiente; // Asegurar que la última amortización cubra el saldo exacto
                             resultado.CuotaCalculada = amortizacionEstaCuota; // La última cuota puede diferir ligeramente
                        }
                        
                        saldoPendiente -= amortizacionEstaCuota;

                        resultado.Planilla.Add(new FilaAmortizacion
                        {
                            NumeroCuota = i.ToString(),
                            SaldoInicial = saldoInicialActualParaFila,
                            Cuota = (i == cantidadCuotas) ? Math.Round(amortizacionEstaCuota, 2).ToString("N2", CultureInfo.InvariantCulture) : resultado.CuotaCalculada.ToString("N2", CultureInfo.InvariantCulture),
                            Intereses = intereses.ToString("N2", CultureInfo.InvariantCulture),
                            Amortizacion = Math.Round(amortizacionEstaCuota, 2).ToString("N2", CultureInfo.InvariantCulture),
                            SaldoCapital = Math.Round(Math.Abs(saldoPendiente) < 0.005 ? 0 : saldoPendiente, 2).ToString("N2", CultureInfo.InvariantCulture)
                        });
                    }
                    // Asegurar que la cuota general refleje la cuota regular si todas son iguales
                    if (cantidadCuotas > 0) resultado.CuotaCalculada = Math.Round(capital / cantidadCuotas, 2);
                    return resultado;
                }

                double factorAnualidad = (tasaMensualEfectiva * Math.Pow(1.0 + tasaMensualEfectiva, cantidadCuotas)) /
                                         (Math.Pow(1.0 + tasaMensualEfectiva, cantidadCuotas) - 1.0);

                double cuotaCalculadaPrecisa = capital * factorAnualidad;
                resultado.CuotaCalculada = Math.Round(cuotaCalculadaPrecisa, 2);

                double saldoCapital = capital;
                for (int i = 1; i <= cantidadCuotas; i++)
                {
                    string saldoInicialActualParaFila = saldoCapital.ToString("N2", CultureInfo.InvariantCulture);
                    double intereses = Math.Round(saldoCapital * tasaMensualEfectiva, 7);
                    double amortizacion = cuotaCalculadaPrecisa - intereses;
                    double cuotaParaEstaFila = cuotaCalculadaPrecisa;

                    if (i == cantidadCuotas)
                    {
                        amortizacion = saldoCapital;
                        cuotaParaEstaFila = amortizacion + intereses;
                    }
                    
                    saldoCapital -= amortizacion;

                    resultado.Planilla.Add(new FilaAmortizacion
                    {
                        NumeroCuota = i.ToString(),
                        SaldoInicial = saldoInicialActualParaFila,
                        Cuota = Math.Round(cuotaParaEstaFila, 2).ToString("N2", CultureInfo.InvariantCulture),
                        Intereses = Math.Round(intereses, 2).ToString("N2", CultureInfo.InvariantCulture),
                        Amortizacion = Math.Round(amortizacion, 2).ToString("N2", CultureInfo.InvariantCulture),
                        SaldoCapital = Math.Round(Math.Abs(saldoCapital) < 0.005 ? 0 : saldoCapital, 2).ToString("N2", CultureInfo.InvariantCulture)
                    });
                }
            }
            catch (Exception ex)
            {
                resultado.Error = true;
                resultado.MensajeError = "Error en el cálculo de la planilla: " + ex.Message;
            }
            return resultado;
        }

        // --- MÉTODOS PARA INTERÉS SIMPLE (MOCKEADOS/SIMULADOS) ---
        public double CalcularInteresSimplePorDias(double capital, double tasaAnualPorcentaje, int tiempoEnDias)
        {
            Console.WriteLine($"INTERÉS SIMPLE DÍAS (Servicio): C={capital}, T%={tasaAnualPorcentaje}, D={tiempoEnDias}");
            if (capital <= 0 || tasaAnualPorcentaje < 0 || tiempoEnDias <= 0) return 0;
            return (capital * tasaAnualPorcentaje * tiempoEnDias) / 36000.0;
        }

        public double CalcularInteresSimplePorMeses(double capital, double tasaAnualPorcentaje, int tiempoEnMeses)
        {
            Console.WriteLine($"INTERÉS SIMPLE MESES (Servicio): C={capital}, T%={tasaAnualPorcentaje}, M={tiempoEnMeses}");
            if (capital <= 0 || tasaAnualPorcentaje < 0 || tiempoEnMeses <= 0) return 0;
            return (capital * tasaAnualPorcentaje * tiempoEnMeses) / 1200.0;
        }

        public double CalcularInteresSimplePorAños(double capital, double tasaAnualPorcentaje, int tiempoEnAños)
        {
            Console.WriteLine($"INTERÉS SIMPLE AÑOS (Servicio): C={capital}, T%={tasaAnualPorcentaje}, A={tiempoEnAños}");
            if (capital <= 0 || tasaAnualPorcentaje < 0 || tiempoEnAños <= 0) return 0;
            return (capital * tasaAnualPorcentaje * tiempoEnAños) / 100.0;
        }

        // --- MÉTODOS PARA EQUIVALENCIA DE TASAS (MOCKEADOS/SIMULADOS) ---
        public double CalcularEquivalenciaDesdeTasaAnual(double tasaAnualNominal, int diasParaEquivalencia)
        {
            Console.WriteLine($"EQUIV ANUAL (Servicio): T%Anual={tasaAnualNominal}, DíasEq={diasParaEquivalencia}");
            if (diasParaEquivalencia <= 0 || tasaAnualNominal < -100) return 0; // Tasa -100% es el límite inferior
            double tasaDiariaEfectiva = Math.Pow(1.0 + (tasaAnualNominal / 100.0), 1.0 / 360.0) - 1.0;
            double tasaEquivalente = (Math.Pow(1.0 + tasaDiariaEfectiva, diasParaEquivalencia) - 1.0) * 100.0;
            return tasaEquivalente;
        }

        public double CalcularEquivalenciaDesdeTasaMensual(double tasaMensual, int diasCapitalizacionMensual)
        {
            Console.WriteLine($"EQUIV MENSUAL (Servicio): T%Mensual={tasaMensual}, DíasCap={diasCapitalizacionMensual}");
            if (diasCapitalizacionMensual <= 0 || tasaMensual < -100) return 0;
            double tasaAnualEfectiva = (Math.Pow(1.0 + (tasaMensual / 100.0), 360.0 / diasCapitalizacionMensual) - 1.0) * 100.0;
            return tasaAnualEfectiva;
        }

        public double CalcularEquivalenciaDesdeTasaDiaria(double tasaDiaria)
        {
            Console.WriteLine($"EQUIV DIARIA (Servicio): T%Diaria={tasaDiaria}");
            if (tasaDiaria < -100) return 0;
            double tasaMensualEfectiva = (Math.Pow(1.0 + (tasaDiaria / 100.0), 30.0) - 1.0) * 100.0;
            return tasaMensualEfectiva;
        }

        public double ConvertirLinealAnualAEfectivaAnual(double tasaLinealAnual)
        {
            Console.WriteLine($"LINEAL A EFECTIVA (Servicio): T%Lineal={tasaLinealAnual}");
            if (tasaLinealAnual < -100 * 12) return 0; // Aproximación
            double tasaMensual = tasaLinealAnual / 100.0 / 12.0;
            double tasaEfectivaAnual = (Math.Pow(1.0 + tasaMensual, 12.0) - 1.0) * 100.0;
            return tasaEfectivaAnual;
        }

        public double ConvertirEfectivaAnualALinealAnual(double tasaEfectivaAnual)
        {
            Console.WriteLine($"EFECTIVA A LINEAL (Servicio): T%Efectiva={tasaEfectivaAnual}");
             if (tasaEfectivaAnual < -100) return 0;
            double tasaMensualEquivalente = Math.Pow(1.0 + (tasaEfectivaAnual / 100.0), 1.0 / 12.0) - 1.0;
            double tasaLinealAnual = tasaMensualEquivalente * 12.0 * 100.0;
            return tasaLinealAnual;
        }
    }
}