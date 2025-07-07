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
        public int CalcularDiasTranscurridosReales(DateTime fechaDesde, DateTime fechaHasta)
        {
            Console.WriteLine($"DÍAS 365 (Servicio): Desde={fechaDesde.ToShortDateString()}, Hasta={fechaHasta.ToShortDateString()}");

            if (fechaHasta < fechaDesde)
            {
                return 0; // O podrías lanzar un error si prefieres
            }

            TimeSpan diferencia = fechaHasta.Subtract(fechaDesde);
            return diferencia.Days;
        }

        public class ResultadoItemMora
        {
            public int DiasAtraso { get; set; }
            public double MontoMora { get; set; }
            public double TotalConMora { get; set; }
        }

        public ResultadoItemMora CalcularMoraParaItem(double montoItem, DateTime fechaVencimientoItem, DateTime fechaActualizacion, double tasaMoraAnualPorcentaje)
        {
            Console.WriteLine($"CALCULAR MORA ITEM (Servicio): Monto={montoItem}, Vto={fechaVencimientoItem.ToShortDateString()}, Act={fechaActualizacion.ToShortDateString()}, TasaMora%={tasaMoraAnualPorcentaje}");

            if (montoItem <= 0)
            {
                return new ResultadoItemMora { DiasAtraso = 0, MontoMora = 0, TotalConMora = montoItem };
            }

            if (fechaActualizacion <= fechaVencimientoItem)
            {
                TimeSpan diff = fechaActualizacion.Subtract(fechaVencimientoItem);
                return new ResultadoItemMora { DiasAtraso = Math.Max(0, diff.Days), MontoMora = 0, TotalConMora = montoItem };
            }

            TimeSpan diferencia = fechaActualizacion.Subtract(fechaVencimientoItem);
            int diasDeAtraso = diferencia.Days;

            double tasaMoraDiariaEfectiva = 0;
            if (tasaMoraAnualPorcentaje > 0)
            {
                tasaMoraDiariaEfectiva = Math.Pow(1.0 + (tasaMoraAnualPorcentaje / 100.0), 1.0 / 365.0) - 1.0;
            }

            double totalConMora = montoItem;
            double montoMoraCalculada = 0;

            if (diasDeAtraso > 0 && tasaMoraDiariaEfectiva > 0)
            {
                totalConMora = montoItem * Math.Pow(1.0 + tasaMoraDiariaEfectiva, diasDeAtraso);
                montoMoraCalculada = totalConMora - montoItem;
            }

            return new ResultadoItemMora
            {
                DiasAtraso = diasDeAtraso,
                MontoMora = montoMoraCalculada,
                TotalConMora = totalConMora
            };
        }
        public class ResultadoTablaMoraCuotas
        {
            public List<FilaMoraCuota> TablaMora { get; set; } = new List<FilaMoraCuota>();
            public double SubtotalCuotas { get; set; }
            public double SubtotalMora { get; set; }
            public double TotalGeneral { get; set; }
            public bool Error { get; set; } = false;
            public string? MensajeError { get; set; }
        }

        public ResultadoTablaMoraCuotas GenerarTablaCalculoMoraCuotas(
            double montoCadaCuota, 
            int cantidadTotalCuotas, 
            double tasaMoraAnualPorcentaje, 
            DateTime fechaVencimientoPrimeraCuota, 
            DateTime fechaActualizacion)
        {
            Console.WriteLine($"GENERAR TABLA MORA CUOTAS (Servicio): MontoC={montoCadaCuota}, NumCuotas={cantidadTotalCuotas}, TasaMoraAnual%={tasaMoraAnualPorcentaje}, Venc1={fechaVencimientoPrimeraCuota.ToShortDateString()}, FechAct={fechaActualizacion.ToShortDateString()}");

            var resultado = new ResultadoTablaMoraCuotas();

            if (montoCadaCuota <= 0 || cantidadTotalCuotas <= 0 || tasaMoraAnualPorcentaje < 0)
            {
                resultado.Error = true;
                resultado.MensajeError = "Monto de cuota y cantidad deben ser positivos. Tasa de mora no puede ser negativa.";
                return resultado;
            }

            try
            {
                // Tasa de mora diaria efectiva (a partir de la anual, asumiendo año 365 para mora)
                // La lógica original parece compleja, simplificaremos a interés compuesto diario para la mora.
                // Tasa Diaria = (1 + TasaMoraAnual / 100)^(1/365) - 1
                double tasaMoraDiariaEfectiva = 0;
                if (tasaMoraAnualPorcentaje > 0)
                {
                    tasaMoraDiariaEfectiva = Math.Pow(1.0 + (tasaMoraAnualPorcentaje / 100.0), 1.0 / 365.0) - 1.0;
                }


                DateTime fechaVencimientoCuotaActual = fechaVencimientoPrimeraCuota;

                for (int i = 1; i <= cantidadTotalCuotas; i++)
                {
                    var fila = new FilaMoraCuota
                    {
                        NumeroCuota = i,
                        FechaCuotaVencida = fechaVencimientoCuotaActual.ToShortDateString(),
                        FechaActualizacion = fechaActualizacion.ToShortDateString(),
                        MontoCuota = montoCadaCuota.ToString("N2", CultureInfo.InvariantCulture)
                    };

                    int diasAtraso = 0;
                    double montoMoraCalculada = 0;
                    double totalConMora = montoCadaCuota;

                    if (fechaActualizacion > fechaVencimientoCuotaActual)
                    {
                        TimeSpan diferencia = fechaActualizacion.Subtract(fechaVencimientoCuotaActual);
                        diasAtraso = diferencia.Days;
                    }

                    fila.DiasAtraso = diasAtraso;

                    if (diasAtraso > 0 && tasaMoraDiariaEfectiva > 0)
                    {
                        // Aplicar interés compuesto diario sobre el monto de la cuota por los días de atraso
                        // MontoFinal = Capital * (1 + i_diaria)^n_dias
                        totalConMora = montoCadaCuota * Math.Pow(1.0 + tasaMoraDiariaEfectiva, diasAtraso);
                        montoMoraCalculada = totalConMora - montoCadaCuota;
                    }

                    fila.MontoMora = montoMoraCalculada.ToString("N2", CultureInfo.InvariantCulture);
                    fila.TotalConMora = totalConMora.ToString("N2", CultureInfo.InvariantCulture);

                    resultado.TablaMora.Add(fila);

                    resultado.SubtotalCuotas += montoCadaCuota;
                    resultado.SubtotalMora += montoMoraCalculada;
                    resultado.TotalGeneral += totalConMora;

                    // Siguiente fecha de vencimiento
                    fechaVencimientoCuotaActual = fechaVencimientoCuotaActual.AddMonths(1);
                }
            }
            catch (Exception ex)
            {
                resultado.Error = true;
                resultado.MensajeError = "Error al generar la tabla de mora: " + ex.Message;
            }
            return resultado;
        }

        public class CalculoCuotasUIYPesosGeneral
        {
            // Resultados para el cálculo individual
            public string? TasaMensualEquivalente { get; set; }
            public string? CapitalEnPesos { get; set; }
            public string? CapitalEnUI { get; set; }
            public string? CuotaCalculadaEnUI { get; set; }
            public string? CuotaCalculadaEnPesos { get; set; }

            // Tabla de resultados
            public List<ResultadoCuotaUIYPesos> TablaResultados { get; set; } = new List<ResultadoCuotaUIYPesos>();
            public bool Error { get; set; } = false;
            public string? MensajeError { get; set; }
        }

        public CalculoCuotasUIYPesosGeneral CalcularCuotasUIYPesosCompleto(
            double capitalDolares, 
            double tasaAnualNominal, 
            int cantidadMesesIndividual, // Para el cálculo de la cuota individual
            double cotizacionDolarAPesos, 
            double cotizacionUIApesos)
        {
            Console.WriteLine($"CALCULAR CUOTAS UI Y PESOS (Servicio): CapUSD={capitalDolares}, TNA%={tasaAnualNominal}, MesesInd={cantidadMesesIndividual}, CotUSD={cotizacionDolarAPesos}, CotUI={cotizacionUIApesos}");

            var resultadoGeneral = new CalculoCuotasUIYPesosGeneral();

            if (capitalDolares <= 0 || tasaAnualNominal < 0 || cantidadMesesIndividual <= 0 || 
                cotizacionDolarAPesos <= 0 || cotizacionUIApesos <= 0)
            {
                resultadoGeneral.Error = true;
                resultadoGeneral.MensajeError = "Todos los valores de capital, tasas y cotizaciones deben ser positivos (tasa puede ser cero).";
                return resultadoGeneral;
            }

            try
            {
                // 1. Calcular Tasa Mensual Efectiva
                double tasaMensualEfectiva;
                if (tasaAnualNominal == 0)
                {
                    tasaMensualEfectiva = 0;
                    resultadoGeneral.TasaMensualEquivalente = "0.0000000%";
                }
                else
                {
                    // Asumiendo que la tasaAnualNominal es una TNA con capitalización mensual.
                    // TEM = (1 + TNA/1200)^1 -1 = TNA/1200 (si es TNA simple)
                    // O si es TEA a TEM: TEM = (1 + TEA/100)^(1/12) - 1
                    // La lógica original del ASPX:
                    // interesanual2 = Math.Pow(1.0 + (TTT / 100), ((double)1 / (double)360)); COEFDIARIO
                    // COEFEQUIV = Math.Pow(COEFDIARIO, 30);
                    // TXTTASAMENSUAL.Text = ((COEFEQUIV - 1) * 100).ToString();
                    // Esto es ( (1 + TNA/100)^(1/360) )^30 - 1 = (1 + TNA/100)^(30/360) - 1 = (1 + TNA/100)^(1/12) - 1
                    tasaMensualEfectiva = Math.Pow(1.0 + (tasaAnualNominal / 100.0), 1.0 / 12.0) - 1.0;
                    resultadoGeneral.TasaMensualEquivalente = (tasaMensualEfectiva * 100.0).ToString("N7", CultureInfo.InvariantCulture) + "%";
                }

                // 2. Conversiones de Capital
                double capitalEnPesos = capitalDolares * cotizacionDolarAPesos;
                double capitalEnUI = capitalEnPesos / cotizacionUIApesos;
                resultadoGeneral.CapitalEnPesos = capitalEnPesos.ToString("N2", CultureInfo.InvariantCulture);
                resultadoGeneral.CapitalEnUI = capitalEnUI.ToString("N2", CultureInfo.InvariantCulture);

                // 3. Calcular Cuota Individual en UI y Pesos
                double cuotaIndividualEnUI;
                if (Math.Abs(tasaMensualEfectiva) < 0.0000000001) // Tasa cero
                {
                    cuotaIndividualEnUI = capitalEnUI / cantidadMesesIndividual;
                }
                else
                {
                    double factorAnualidadInd = (tasaMensualEfectiva * Math.Pow(1.0 + tasaMensualEfectiva, cantidadMesesIndividual)) /
                                            (Math.Pow(1.0 + tasaMensualEfectiva, cantidadMesesIndividual) - 1.0);
                    cuotaIndividualEnUI = capitalEnUI * factorAnualidadInd;
                }
                resultadoGeneral.CuotaCalculadaEnUI = cuotaIndividualEnUI.ToString("N2", CultureInfo.InvariantCulture);
                resultadoGeneral.CuotaCalculadaEnPesos = (cuotaIndividualEnUI * cotizacionUIApesos).ToString("N2", CultureInfo.InvariantCulture);

                // 4. Generar Tabla de Cuotas para diferentes plazos
                for (int mesesTabla = 6; mesesTabla <= 240; mesesTabla += 6)
                {
                    double cuotaTablaEnUI;
                    if (Math.Abs(tasaMensualEfectiva) < 0.0000000001) // Tasa cero
                    {
                        cuotaTablaEnUI = capitalEnUI / mesesTabla;
                    }
                    else
                    {
                        double factorAnualidadTabla = (tasaMensualEfectiva * Math.Pow(1.0 + tasaMensualEfectiva, mesesTabla)) /
                                                    (Math.Pow(1.0 + tasaMensualEfectiva, mesesTabla) - 1.0);
                        cuotaTablaEnUI = capitalEnUI * factorAnualidadTabla;
                    }

                    double cuotaTablaEnPesos = cuotaTablaEnUI * cotizacionUIApesos;

                    resultadoGeneral.TablaResultados.Add(new ResultadoCuotaUIYPesos
                    {
                        CapitalEnDolares = capitalDolares, // El capital base es el mismo
                        Meses = mesesTabla,
                        CuotaEnUI = cuotaTablaEnUI.ToString("N2", CultureInfo.InvariantCulture),
                        CuotaEnPesos = cuotaTablaEnPesos.ToString("N2", CultureInfo.InvariantCulture)
                    });
                }
            }
            catch (Exception ex)
            {
                resultadoGeneral.Error = true;
                resultadoGeneral.MensajeError = "Error en el cálculo: " + ex.Message;
            }
            return resultadoGeneral;
        }

        public double CalcularTasaEfectivaAnual(double tasaNominalAnual, int periodosCapitalizacionPorAño = 12)
        {
            Console.WriteLine($"CALCULAR TEA (Servicio): TNA%={tasaNominalAnual}, PeríodosCap={periodosCapitalizacionPorAño}");
            if (tasaNominalAnual < -100 * periodosCapitalizacionPorAño) // Límite aproximado para evitar errores
            {
                return -100.0; // O manejar error
            }
            double tasaPeriodica = (tasaNominalAnual / 100.0) / periodosCapitalizacionPorAño;
            double tea = (Math.Pow(1.0 + tasaPeriodica, periodosCapitalizacionPorAño) - 1.0) * 100.0;
            return tea;
        }
        public List<ResultadoCalculoCuota> GenerarTablaDeCuotas(double capitalInicial, double tasaAnualNominal)
        {
            Console.WriteLine($"GENERAR TABLA DE CUOTAS (Servicio): Capital={capitalInicial}, TasaAnual%={tasaAnualNominal}");
            var listaResultados = new List<ResultadoCalculoCuota>();

            if (capitalInicial <= 0 || tasaAnualNominal < 0)
            {
                // Podríamos añadir un resultado de error o simplemente devolver una lista vacía
                // Por ahora, devolvemos una lista vacía si las entradas base son inválidas.
                return listaResultados;
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

                for (int cantidadMeses = 6; cantidadMeses <= 240; cantidadMeses += 6)
                {
                    double montoCuota;
                    if (Math.Abs(tasaMensualEfectiva) < 0.0000000001) // Tasa cero o muy cercana
                    {
                        montoCuota = Math.Round(capitalInicial / cantidadMeses, 2);
                    }
                    else
                    {
                        double factorAnualidad = (tasaMensualEfectiva * Math.Pow(1.0 + tasaMensualEfectiva, cantidadMeses)) /
                                                (Math.Pow(1.0 + tasaMensualEfectiva, cantidadMeses) - 1.0);
                        double cuotaCalculadaPrecisa = capitalInicial * factorAnualidad;
                        montoCuota = Math.Round(cuotaCalculadaPrecisa, 2);
                    }
                    listaResultados.Add(new ResultadoCalculoCuota
                    {
                        CapitalSolicitado = capitalInicial,
                        CantidadMeses = cantidadMeses,
                        MontoCuota = montoCuota
                    });
                }
            }
            catch (Exception ex)
            {
                // Manejo de error si es necesario, quizás añadiendo un item de error a la lista
                // o lanzando la excepción para que el PageModel la maneje.
                Console.WriteLine($"Error en GenerarTablaDeCuotas: {ex.Message}");
                // Podrías añadir un resultado con error:
                // listaResultados.Add(new ResultadoCalculoCuota { Error = true, MensajeError = "Error en cálculo." });
            }
            return listaResultados;
        }

        // El método CalcularCuotaFijaMensual que creamos antes también es útil
        // si solo quieres calcular una cuota individual, como lo hace el botón "Calcular Cuota"
        // antes de generar la tabla completa.
        public class ResultadoCalculoCuota
        {
            public double CapitalSolicitado { get; set; }
            public int CantidadMeses { get; set; }
            public double MontoCuota { get; set; }
            public bool Error { get; set; } = false;
            public string? MensajeError { get; set; }
        }

        public ResultadoCalculoCuota CalcularCuotaFijaMensual(double capital, double tasaAnualNominal, int cantidadMeses)
        {
            Console.WriteLine($"CALCULAR CUOTA FIJA (Servicio): Capital={capital}, TasaAnual%={tasaAnualNominal}, Meses={cantidadMeses}");
            var resultado = new ResultadoCalculoCuota
            {
                CapitalSolicitado = capital,
                CantidadMeses = cantidadMeses
            };

            if (capital <= 0 || tasaAnualNominal < 0 || cantidadMeses <= 0)
            {
                resultado.Error = true;
                resultado.MensajeError = "Capital y cantidad de meses deben ser mayores a cero. La tasa debe ser mayor o igual a cero.";
                if (tasaAnualNominal < 0 && capital > 0 && cantidadMeses > 0)
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


                if (Math.Abs(tasaMensualEfectiva) < 0.0000000001) // Tasa cero o muy cercana
                {
                    resultado.MontoCuota = Math.Round(capital / cantidadMeses, 2);
                }
                else
                {
                    double factorAnualidad = (tasaMensualEfectiva * Math.Pow(1.0 + tasaMensualEfectiva, cantidadMeses)) /
                                            (Math.Pow(1.0 + tasaMensualEfectiva, cantidadMeses) - 1.0);
                    double cuotaCalculadaPrecisa = capital * factorAnualidad;
                    resultado.MontoCuota = Math.Round(cuotaCalculadaPrecisa, 2);
                }
            }
            catch (Exception ex)
            {
                resultado.Error = true;
                resultado.MensajeError = "Error en el cálculo de la cuota: " + ex.Message;
            }
            return resultado;
        }
        public class ResultadoMoraVariasCuotasTodasVencidas
        {
            public double ImporteAPagar { get; set; }
            // Podrías añadir más propiedades si quieres desglosar, como el monto acumulado antes del excedente.
        }

        public ResultadoMoraVariasCuotasTodasVencidas CalcularMoraVariasCuotasTodasVencidas(double montoCadaCuota, double tasaInteresMoraMensual, int numeroCuotasVencidasImpagas, int diasExcedentesMora)
        {
            Console.WriteLine($"MORA VARIAS CUOTAS (TODAS VENCIDAS + EXCEDENTE) (Servicio): MontoCuota={montoCadaCuota}, TasaMoraMensual%={tasaInteresMoraMensual}, NumCuotasVencidas={numeroCuotasVencidasImpagas}, DiasExcedentes={diasExcedentesMora}");

            if (montoCadaCuota <= 0 || tasaInteresMoraMensual < 0 || numeroCuotasVencidasImpagas <= 0 || diasExcedentesMora < 0)
            {
                return new ResultadoMoraVariasCuotasTodasVencidas { ImporteAPagar = montoCadaCuota * numeroCuotasVencidasImpagas };
            }

            double tasaInteresMora = tasaInteresMoraMensual / 100.0;
            double montoAcumuladoConMora;

            // Calculamos el valor futuro de las cuotas vencidas (como en el método anterior)
            if (tasaInteresMora == 0)
            {
                montoAcumuladoConMora = montoCadaCuota * numeroCuotasVencidasImpagas;
            }
            else
            {
                double factorValorFuturoAnualidad = (Math.Pow(1.0 + tasaInteresMora, numeroCuotasVencidasImpagas) - 1.0) / tasaInteresMora;
                montoAcumuladoConMora = montoCadaCuota * factorValorFuturoAnualidad;
            }

            // Ahora, calculamos el interés adicional por los días excedentes sobre el monto acumulado.
            // Para esto, necesitamos una tasa diaria efectiva de la tasa de mora mensual.
            double importeFinalAPagar = montoAcumuladoConMora;
            if (diasExcedentesMora > 0 && tasaInteresMora > 0) // Solo aplicar mora excedente si hay días y tasa
            {
                // Tasa Diaria Efectiva = (1 + TasaMoraMensual)^(1/30) - 1
                double tasaDiariaEfectivaMora = Math.Pow(1.0 + tasaInteresMora, 1.0 / 30.0) - 1.0;

                if (tasaDiariaEfectivaMora > -1) // Evitar problemas con Math.Pow si tasa es -100%
                {
                    importeFinalAPagar = montoAcumuladoConMora * Math.Pow(1.0 + tasaDiariaEfectivaMora, diasExcedentesMora);
                }
                else
                {
                    importeFinalAPagar = 0; // Si la tasa de mora es -100% o peor
                }
            }
            else if (diasExcedentesMora > 0 && tasaInteresMora == 0) // Si hay días excedentes pero tasa de mora es 0
            {
                // No se acumula más interés
                importeFinalAPagar = montoAcumuladoConMora;
            }


            return new ResultadoMoraVariasCuotasTodasVencidas
            {
                ImporteAPagar = importeFinalAPagar
            };
        }

        public class ResultadoMoraVariasCuotasIguales
        {
            public double ImporteAPagar { get; set; } // Esto es C * [(1+i)^n - 1] / i
            // Si quieres desglosar la mora, puedes añadir más propiedades
            // public double TotalCapitalCuotas { get; set; }
            // public double TotalInteresesMora { get; set; }
        }

        public ResultadoMoraVariasCuotasIguales CalcularMoraVariasCuotasIguales(double montoCadaCuota, double tasaInteresMoraMensual, int numeroCuotasVencidasImpagas)
        {
            Console.WriteLine($"MORA VARIAS CUOTAS IGUALES (Servicio): MontoCuota={montoCadaCuota}, TasaMoraMensual%={tasaInteresMoraMensual}, NumCuotasVencidas={numeroCuotasVencidasImpagas}");

            if (montoCadaCuota <= 0 || tasaInteresMoraMensual < 0 || numeroCuotasVencidasImpagas <= 0)
            {
                // Si los datos son inválidos, devolver el capital de las cuotas sin mora o un error
                return new ResultadoMoraVariasCuotasIguales { ImporteAPagar = montoCadaCuota * numeroCuotasVencidasImpagas };
            }

            double tasaInteresMora = tasaInteresMoraMensual / 100.0;
            double importeAPagar;

            // La fórmula original era:
            // COEFDIARIO = Math.Pow(X, Y) - 1; donde X = (1 + TasaMoraMensual/100), Y = NumeroCuotasVencidas
            // COEFDIARIO2 = COEFDIARIO / (TasaMoraMensual / 100);
            // TXTIMPORTEAPAGR.Text = (COEFDIARIO2 * MontoCuota).ToString("N");
            // Esta fórmula es para el valor futuro de una anualidad: VF = C * [((1+i)^n - 1) / i]
            // Donde:
            // C = Monto de cada cuota
            // i = Tasa de interés por período (tasaInteresMora)
            // n = Número de períodos (numeroCuotasVencidasImpagas)

            if (tasaInteresMora == 0) // Si la tasa de mora es 0%
            {
                importeAPagar = montoCadaCuota * numeroCuotasVencidasImpagas;
            }
            else
            {
                double factorValorFuturoAnualidad = (Math.Pow(1.0 + tasaInteresMora, numeroCuotasVencidasImpagas) - 1.0) / tasaInteresMora;
                importeAPagar = montoCadaCuota * factorValorFuturoAnualidad;
            }

            return new ResultadoMoraVariasCuotasIguales
            {
                ImporteAPagar = importeAPagar
                // TotalCapitalCuotas = montoCadaCuota * numeroCuotasVencidasImpagas,
                // TotalInteresesMora = importeAPagar - (montoCadaCuota * numeroCuotasVencidasImpagas)
            };
        }

        public class ResultadoMoraCapital
        {
            public int DiasDeAtraso { get; set; }
            public double InteresesMoratorios { get; set; }
            public double TotalAPagar { get; set; }
        }

        public ResultadoMoraCapital CalcularMoraDeUnCapital(double capitalAdeudado, double tasaInteresMoraAnual, DateTime fechaVencimiento, DateTime fechaPago)
        {
            Console.WriteLine($"MORA CAPITAL (Servicio): Capital={capitalAdeudado}, TasaMoraAnual%={tasaInteresMoraAnual}, Venc={fechaVencimiento.ToShortDateString()}, Pago={fechaPago.ToShortDateString()}");

            if (capitalAdeudado <= 0)
            {
                return new ResultadoMoraCapital { DiasDeAtraso = 0, InteresesMoratorios = 0, TotalAPagar = capitalAdeudado };
            }

            if (fechaPago <= fechaVencimiento) // No hay mora si se paga en o antes de la fecha de vencimiento
            {
                return new ResultadoMoraCapital { DiasDeAtraso = 0, InteresesMoratorios = 0, TotalAPagar = capitalAdeudado };
            }

            TimeSpan diferencia = fechaPago.Subtract(fechaVencimiento);
            int diasDeAtraso = diferencia.Days;

            // La lógica original parece usar interés simple para la mora:
            // (Capital * TasaMoraAnual * DiasAtraso) / 36500 (si la tasa es %)
            // O (Capital * (TasaMoraAnual/100) * DiasAtraso) / 365
            double tasaMoraDiaria = (tasaInteresMoraAnual / 100.0) / 365.0; // Tasa diaria simple
            double interesesMoratorios = capitalAdeudado * tasaMoraDiaria * diasDeAtraso;
            double totalAPagar = capitalAdeudado + interesesMoratorios;

            return new ResultadoMoraCapital
            {
                DiasDeAtraso = diasDeAtraso,
                InteresesMoratorios = interesesMoratorios,
                TotalAPagar = totalAPagar
            };
        }

        public double CalcularImporteCancelacionAnticipada(double tasaInteresMensualPorcentaje, int numeroTotalCuotas, double montoDeCadaCuota, int cuotasYaPagas)
        {
            Console.WriteLine($"CANCELACIÓN ANTICIPADA (Servicio): TasaMensual%={tasaInteresMensualPorcentaje}, TotalCuotas={numeroTotalCuotas}, MontoCuota={montoDeCadaCuota}, CuotasPagas={cuotasYaPagas}");

            if (montoDeCadaCuota <= 0 || tasaInteresMensualPorcentaje < 0 || numeroTotalCuotas <= 0 || cuotasYaPagas < 0 || cuotasYaPagas >= numeroTotalCuotas)
            {
                // Si ya se pagaron todas o más cuotas, o los datos son inválidos, el importe a pagar es 0 o un error.
                // Devolver 0 si ya está pago, o podrías manejar un error/NaN para otros casos inválidos.
                if (cuotasYaPagas >= numeroTotalCuotas && montoDeCadaCuota > 0) return 0.0;
                // Para otros datos inválidos, podría ser mejor indicar un error o devolver NaN.
                // Por simplicidad para el mock, si los datos no tienen sentido, devolvemos 0.
                return 0.0;
            }

            double tasaInteresMensual = tasaInteresMensualPorcentaje / 100.0;
            int cuotasRestantes = numeroTotalCuotas - cuotasYaPagas;

            if (tasaInteresMensual == 0) // Si la tasa es 0%
            {
                return montoDeCadaCuota * cuotasRestantes;
            }

            // Fórmula del valor presente de una anualidad:
            // VA = C * [ (1 - (1 + i)^-n) / i ]
            // Donde C = Monto de cada cuota, i = tasa de interés por período, n = número de períodos restantes.
            // La lógica original usaba:
            // double X = (+T + 1); // T era tasaInteresMensualPorcentaje / 100
            // double Y = -(Convert.ToDouble(TXTCUOTAS.Text) - Convert.ToDouble(TXTCUOTASYAPAGAS.Text)); // -cuotasRestantes
            // T1 = Math.Pow(X, Y);
            // T1 = 1 - T1;
            // T2 = T1 / +(T);
            // TXTIMPORTEAPAGAR.Text = (T2 * Convert.ToDouble(TXTMONTODECADACUOTA.Text)).ToString("N");

            // Replicando la fórmula:
            double factor1 = Math.Pow(1.0 + tasaInteresMensual, -cuotasRestantes);
            double valorPresenteFactor = (1.0 - factor1) / tasaInteresMensual;
            double importeAPagar = valorPresenteFactor * montoDeCadaCuota;

            return importeAPagar;
        }
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