namespace Sistema.Gestion.Nómina.Views.Credito
{
    public class GetCreditoHistorialDTO
    {
        public DateOnly fecha { get; set; }
        public decimal? totalPagado { get; set; }
        public decimal? totalPediente { get; set; }
    }
}
