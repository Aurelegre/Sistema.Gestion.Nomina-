namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
	public class EmpleadosDTO
	{
		public int Id { get; set; }
        public string Nombre { get; set; }
		public string Puesto { get; set; }
		public string Departamento { get; set;}
        public string DPI { get; set; }
		public decimal? Sueldo { get; set; }
        public DateTime? FechaContratado { get; set; }
        public string Usuario { get; set; }

    }
}
