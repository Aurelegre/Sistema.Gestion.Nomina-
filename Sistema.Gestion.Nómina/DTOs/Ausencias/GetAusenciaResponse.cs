namespace Sistema.Gestion.Nómina.DTOs.Ausencias
{
    public class GetAusenciaResponse
    {
        public int? IdEmpleado { get; set; }
        public int? Id { get; set; }
        public string Detalle { get; set; }
        public DateOnly FechaSoli { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public DateOnly? FechaAut { get; set; }
        public int? Estado { get; set; }
        public int? Tipo { get; set; }
        public string? Jefe {  get; set; }
        public decimal? Deducible { get; set; }
        public int? Dias { get; set; }

    }
}
    