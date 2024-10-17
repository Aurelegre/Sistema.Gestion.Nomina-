namespace Sistema.Gestion.Nómina.DTOs.Prestamos
{
    public class GetHistorialDTO
    {
        public DateOnly fecha { get; set; }
        public decimal? totalPagado { get; set; }
        public decimal? totalPediente { get; set; }
    }
}
