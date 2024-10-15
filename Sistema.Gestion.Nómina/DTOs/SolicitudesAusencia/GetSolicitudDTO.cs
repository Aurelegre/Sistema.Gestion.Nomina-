namespace Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia
{
    public class GetSolicitudDTO
    {
        public int Id { get; set; }
        public int? idEmpeado { get; set; }
        public string Empleado { get; set; }
        public string Detalle { get; set; }
        public DateOnly FechaSoli { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public DateOnly? FechaAut { get; set; }
        public int? Dias { get; set; }
        public int? Estado { get; set; }
        public int? Tipo { get; set; }
        public decimal? Deducible { get; set; }
        public string? Jefe { set; get; }
    }
}
