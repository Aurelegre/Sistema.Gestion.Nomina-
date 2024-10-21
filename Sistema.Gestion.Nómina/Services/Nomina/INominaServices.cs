using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.Services.Nomina
{
    public interface INominaServices
    {
        public decimal? DescuentoAusencia (decimal? amount, int? days);
        public decimal? CalcularAdelanto(decimal? Sueldo); 
    }
}
