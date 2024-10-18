namespace Sistema.Gestion.Nómina.DTOs.Creditos
{
    public class GetCreditosDTO
    {
        public int Estado { get; set; }
        public DateOnly? Fecha { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
