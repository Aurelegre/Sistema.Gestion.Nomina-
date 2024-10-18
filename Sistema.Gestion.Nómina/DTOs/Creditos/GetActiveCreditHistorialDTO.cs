using Sistema.Gestion.Nómina.DTOs.Prestamos;

namespace Sistema.Gestion.Nómina.DTOs.Creditos
{
    public class GetActiveCreditHistorialDTO
    {
        public int Id { get; set; }
        public int? CPendientes { get; set; }
        public decimal? TotalPediente { get; set; }
        public DateOnly fecha { get; set; }
        public List<GetCreditoHistorialDTO> Pagos { get; set; }
    }
}
