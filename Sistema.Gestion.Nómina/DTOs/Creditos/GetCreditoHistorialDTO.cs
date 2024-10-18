namespace Sistema.Gestion.Nómina.DTOs.Creditos
{
    public class GetCreditoHistorialDTO
    {
        public DateOnly fecha { get; set; }
        public decimal? totalPagado { get; set; }
        public decimal? totalPediente { get; set; }
    }
}
