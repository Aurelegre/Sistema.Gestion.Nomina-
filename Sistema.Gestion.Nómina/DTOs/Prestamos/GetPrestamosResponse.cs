namespace Sistema.Gestion.Nómina.DTOs.Prestamos
{
    public class GetPrestamosResponse
    {
        public int Id { get; set; }
        public int? Estado { get; set; }
        public int? Cuotas { get; set; }
        public int? CPendientes { get; set; }
        public decimal? Total { get; set; }
        public decimal? TotalPendiente { get; set; }
        public DateOnly Fecha { get; set; }
    }
}
