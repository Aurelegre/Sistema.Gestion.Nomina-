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
                if (days < 0 || days > 30)
                    throw new ArgumentException("La cantidad de días debe estar entre 0 y 30.");

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
                if (TotalHoras < 0)
                    throw new ArgumentException("Las horas extra deben ser mayores que 0.");

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
                    return null;
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

    }
}
