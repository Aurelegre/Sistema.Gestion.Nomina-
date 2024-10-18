namespace Sistema.Gestion.Nómina.DTOs.Creditos
{
    public class CreateCreditoDTO
    {
        public int IdEmpleado { get; set; }
        public decimal Total { get; set; }
        public int Cuotas { get; set; }
    }
}
