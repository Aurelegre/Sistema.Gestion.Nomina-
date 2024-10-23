using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.Services.Nomina
{
    public interface INominaServices
    {
        public decimal? DescuentoAusencia (decimal? amount, int? days);
        public decimal? CalcularAdelanto(decimal? Sueldo);
        public decimal? CalcularComisionVenta( int idEmpleado, decimal? Total);
        public decimal? CalcularComisionProd(int idEmpleado, decimal? TotalPiezas);
        public decimal? CalcularHorasExtras(decimal? salario, decimal? TotalHoras);
        public decimal? CalcularComisionDiafestivo(decimal? salario, decimal? totalHoras);
        public decimal? PagarCuotaPrestamo(int? cuotas, decimal? pendiente);
        public decimal? CuotaLaboralIGSS(decimal? totalDevengado);
        public decimal? CuotaPatronalIGSS(decimal? totalDevengado);
        public decimal? CalcularISR(decimal? salarioBruto, decimal? iSRAcumulado, decimal? bonificacion);
        public decimal? CalcularAguinaldo(decimal? salarioMensual, DateTime fechaContratacion, DateTime fechaCorte);
        public decimal? CalcularBono14(decimal? salarioMensual, DateTime fechaContratacion, DateTime fechaCorte);

    }
}
