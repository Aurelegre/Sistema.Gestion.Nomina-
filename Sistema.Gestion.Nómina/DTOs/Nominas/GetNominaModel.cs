namespace Sistema.Gestion.Nómina.DTOs.Nominas
{
    public class GetNominaModel
    {
        public int Id { get; set; }

        public string NombreEmpleado { get; set; }
        public string Departamento { get; set; }
        public string Puesto { get; set; }
        public decimal? Sueldo { get; set; }
        public decimal? SueldoExtra { get; set; }

        public decimal? Comisiones { get; set; }

        public decimal? Bonificaciones { get; set; }
        public decimal? AguinaldoBono { get; set; }

        public decimal? OtrosIngresos { get; set; }

        public decimal? TotalDevengado { get; set; }
        public decimal? TotalDescuentos { get; set; }
        public decimal? TotalLiquido { get; set; }

        public decimal? Igss { get; set; }

        public decimal? Isr { get; set; }

        public decimal? Prestamos { get; set; }
        public decimal? Creditos { get; set; }

        public decimal? Anticipos { get; set; }

        public decimal? OtrosDesc { get; set; }
    }
}
