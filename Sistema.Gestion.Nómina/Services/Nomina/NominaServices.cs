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
                var adelanto = sueldo * 0.45m;
                return adelanto;
            }
            catch (Exception ex)
            {
                throw new Exception($"ERROR al calcular Adelanto: {ex.Message}", ex);
            }
        }

        public decimal? CalcularComisionVenta(decimal? sueldo, int idEmpleado, decimal? Total)
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

                return comision;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular la comisión para el empleado {idEmpleado}: {ex.Message}", ex);
            }
        }


    }
}
