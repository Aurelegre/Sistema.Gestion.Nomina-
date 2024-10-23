using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.Services.Nomina
{
    public class NominaServices : INominaServices
    {
        public decimal? DescuentoAusencia(decimal? amount, int? days)
        {
            try
            {
                // Validar parámetros de entrada
                if (amount <= 0)
                    throw new ArgumentException("El monto del salario debe ser mayor que 0.");
               

                // Cálculo del descuento
                decimal? diario = amount / 30;
                decimal? descuento = Math.Round(diario.Value * days.Value, 2);

                return descuento;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular el descuento por ausencias: {ex.Message}", ex);
            }
        }
        public decimal? CalcularAdelanto(decimal? sueldo)
        {
            try
            {
                var adelanto = Math.Round(sueldo.Value * 0.45m, 2);
                return adelanto;
            }
            catch (Exception ex)
            {
                throw new Exception($"ERROR al calcular Adelanto: {ex.Message}", ex);
            }
        }

        public decimal? CalcularComisionVenta(int idEmpleado, decimal? Total)
        {
            try
            {
                decimal? comision = 0;

                // Calcular la comisión en función del rango de ventas
                if (Total >= 0 && Total <= 100000)
                {
                    comision = 0.0m; // 0.0%
                }
                else if (Total >= 100001 && Total <= 200000)
                {
                    comision = Total * 0.025m; // 2.5%
                }
                else if (Total >= 200001 && Total <= 400000)
                {
                    comision = Total * 0.035m; // 3.5%
                }
                else if (Total > 400000)
                {
                    comision = Total * 0.045m; // 4.5%
                }
                
                comision = Math.Round(comision.Value, 2);
                return comision;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular la comisión para el empleado {idEmpleado}: {ex.Message}", ex);
            }
        }

        public decimal? CalcularComisionProd(int idEmpleado, decimal? TotalPiezas)
        {
            try
            {
                // Validar que el total de piezas no sea nulo ni menor a 0
                if (TotalPiezas == null || TotalPiezas < 0)
                    throw new ArgumentException("El total de piezas debe ser un valor válido y mayor o igual a 0.");

                // Cada pieza otorga 0.01 (un centavo)
                decimal? comision = Math.Round(TotalPiezas.Value * 0.01m, 2);

                return comision;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular la comisión de producción para el empleado {idEmpleado}: {ex.Message}", ex);
            }
        }

        public decimal? CalcularHorasExtras(decimal? salario, decimal? TotalHoras)
        {
            try
            {
                // Validar parámetros de entrada
                if (salario == null || salario <= 0)
                    throw new ArgumentException("El salario debe ser mayor que 0.");
                

                // Obtener el salario por hora (dividiendo el salario mensual entre 30 días y luego entre 8 horas por día)
                var salarioHora = (salario / 30) / 8;

                // Calcular el pago de horas extras (1.5 veces el salario por hora)
                if (salarioHora.HasValue && TotalHoras.HasValue)
                {
                    var totalAumento = Math.Round(salarioHora.Value * 1.5m * TotalHoras.Value, 2);
                    return totalAumento;
                }
                else
                {
                    return 0.00m;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular las horas extras: {ex.Message}", ex);
            }
        }

        public decimal? CalcularComisionDiafestivo(decimal? salario, decimal? totalHoras)
        {
            try
            {
                // Obtener el salario por hora (dividiendo el salario mensual entre 30 días y luego entre 8 horas por día)
                var salarioHora = (salario / 30) / 8;

                // Calcular el pago de horas extras (1.5 veces el salario por hora)
                var totalAumento = Math.Round(salarioHora.Value * 2 * totalHoras.Value, 2);

                return totalAumento;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular comision por dias festivos: {ex.Message}", ex);
            }
        }

        public decimal? PagarCuotaPrestamo (int? cuotas, decimal? pendiente)
        {
            try
            {
                var pagar = Math.Round((pendiente.Value / cuotas.Value), 2); ;
                return pagar;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular total a pagar por cuota: {ex.Message}", ex);
            }
        }
        public decimal? CuotaLaboralIGSS(decimal? totalDevengado)
        {
            if (totalDevengado == null || totalDevengado <= 0)
            {
                throw new ArgumentException("El sueldo debe ser mayor a 0.");
            }

            // Calcular el 4.87% del sueldo para la cuota laboral
            decimal? cuotaLaboral = totalDevengado * 0.0487m;
            return Math.Round(cuotaLaboral.Value, 2); // Limitar a 2 decimales
        }

        public decimal? CuotaPatronalIGSS(decimal? totalDevengado)
        {
            if (totalDevengado == null || totalDevengado <= 0)
            {
                throw new ArgumentException("El sueldo debe ser mayor a 0.");
            }

            // Calcular el 12.67% del sueldo para la cuota patronal
            decimal? cuotaPatronal = totalDevengado * 0.1267m;
            return Math.Round(cuotaPatronal.Value, 2); // Limitar a 2 decimales
        }

        public decimal? CalcularISR(decimal? salarioBruto, decimal? iSRAcumulado, decimal? bonificacion)
        {
            if((salarioBruto + bonificacion) <= 4200.00m)
            {
                return 0;
            }
            decimal? salarioAnual = salarioBruto.Value * 12,
                    bonificacionAnual = bonificacion.Value * 12,
                    isr = 0.00m;
            decimal igssAnual = salarioAnual.Value * 0.0483m;
            decimal ingresoAnual = salarioAnual.Value + bonificacionAnual.Value;
            decimal gastosPesonales = (48000m - iSRAcumulado.Value); //descontar el IRS que se ha pagado

            //descuentos anuales
            decimal? totalDescuentos = igssAnual + gastosPesonales;

            //resta de descuentos anuales y ingresos anuales
            decimal? baseImponible = ingresoAnual - totalDescuentos;

            //aplicar tipo impositivo del ISR
            if(baseImponible > 30000m)
            {
                //si la base imponible es mas de 30,0000, obtener el exedente
                baseImponible = baseImponible - 30000m;
                isr = (1500.00m + (baseImponible * 0.07m));//base imponible de 1,500 + el 7% sobre el exdente de 30,000
            }
            else
            {
                //si la base imponible es menor a 30,000 aplicar 5% 
                isr = (baseImponible * 0.05m);
            }
            isr = isr / 12; //obtener el isr mensual
            return Math.Round(isr.Value, 2);
        }

        public decimal? CalcularAguinaldo(decimal? salarioMensual, DateTime fechaContratacion, DateTime fechaCorte)
        {
            // Calcular el número de meses trabajados
            int mesesTrabajados = ((fechaCorte.Year - fechaContratacion.Year) * 12) + fechaCorte.Month - fechaContratacion.Month;

            // Si la fecha de corte es antes de la fecha de contratación, no calcular aguinaldo
            if (mesesTrabajados < 0)
                return 0;

            // Aguinaldo prorrateado
            decimal aguinaldo = (salarioMensual.Value / 12) * mesesTrabajados;

            return Math.Round(aguinaldo, 2);
        }

        public decimal? CalcularBono14(decimal? salarioMensual, DateTime fechaContratacion, DateTime fechaCorte)
        {
            // Calcular el número de meses trabajados
            int mesesTrabajados = ((fechaCorte.Year - fechaContratacion.Year) * 12) + fechaCorte.Month - fechaContratacion.Month;

            // Si la fecha de corte es antes de la fecha de contratación, no calcular Bono 14
            if (mesesTrabajados < 0)
                return 0;

            // Bono 14 prorrateado
            decimal bono14 = (salarioMensual.Value / 12) * mesesTrabajados;

            return Math.Round(bono14, 2);

        }

    }
}
