namespace Sistema.Gestion.Nómina.DTOs.EmployeeDepto
{
    public class GetEmployeeDeptoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Puesto { get; set; }
        public string Departamento { get; set; }
        public string DPI { get; set; }
        public decimal? Sueldo { get; set; }
        public decimal? HorasExtra { get; set; }
        public decimal? ComisionHorasExtra { get; set; }
        public decimal? HorasDiaFestivo { get; set; }
        public decimal? ComisionDiaFestivo { get; set; }
        public decimal? ComisionVentas { get; set; }
        public decimal? ComisionProd { get; set; }
        public string? Anticipo { get; set; }
        public DateOnly FechaContratado { get; set; }
    }
}
