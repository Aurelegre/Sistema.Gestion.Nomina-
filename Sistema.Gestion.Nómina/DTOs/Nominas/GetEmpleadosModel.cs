namespace Sistema.Gestion.Nómina.DTOs.Nominas
{
    public class GetEmpleadosModel
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string DPI { get; set; }
        public string Departamento { get; set; }
        public string Puesto { get; set; }
        public decimal? Sueldo { get; set; }
        public DateTime FechaContratado { get; set; }
    }
}
