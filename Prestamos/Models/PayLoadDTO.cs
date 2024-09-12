namespace Prestamos.Models
{
    public class PayLoadDTO
    {
        public int IdEmpresa { get; set; }
        public int IdPrestamo { get; set; }
        public int IdEmpleado { get; set; }
        public decimal TotalPagado { get; set; }

    }
}
