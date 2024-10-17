namespace Sistema.Gestion.Nómina.DTOs.Prestamos
{
    public class CreatePrestamoDTO
    { 
        public int IdEmpleado { get; set; }
        public decimal Total { get; set; }
        public int Cuotas { get; set; }
    }
}
