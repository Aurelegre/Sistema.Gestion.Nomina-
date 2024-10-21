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
                throw new Exception("ERROR al calcular Adelanto", ex);
            }
        }

    }
}
