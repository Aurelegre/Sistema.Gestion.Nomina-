namespace Sistema.Gestion.Nómina.Views.Credito
{
    public class GetCreditoHistorialResponse
    {
        public int Id { get; set; }
        public int? CPendientes { get; set; }
        public decimal? TotalPediente { get; set; }
        public List<GetCreditoHistorialDTO> Pagos { get; set; }
    }
}
