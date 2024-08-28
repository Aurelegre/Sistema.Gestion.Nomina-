namespace Sistema.Gestion.Nómina.Entitys
{
    public partial class HistorialPago
    {
        public int Id { get; set; }

        public int? IdEmpleado { get; set; }

        public int? IdPrestamo { get; set; }

        public decimal? TotalPagado { get; set; }

        public DateTime? FechaPago { get; set; }

        public decimal? TotalPendiente { get; set; }

        public virtual Empleado? IdEmpleadoNavigation { get; set; }

        public virtual Prestamo? IdPrestamoNavigation { get; set; }
    }
}
