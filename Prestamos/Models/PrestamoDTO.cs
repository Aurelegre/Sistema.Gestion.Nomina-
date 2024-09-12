using System.Security.Principal;

namespace Prestamos.Models
{
    public class PrestamoDTO
    {
        public int IdEmpresa { get; set; }
        public int? IdEmpleado { get; set; }

        public decimal? Total { get; set; }

        public int? Cuotas { get; set; }

        public DateTime? FechaPrestamo { get; set; }

        public int? IdTipo { get; set; }
    }
}
