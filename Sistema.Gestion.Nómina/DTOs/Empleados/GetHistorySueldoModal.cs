namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class GetHistorySueldoModal
    {
        public decimal? Nuevo { get; set; }
        public decimal? Anterior { get; set; }
        public DateOnly Fecha { get; set; }
    }
}
