namespace Sistema.Gestion.Nómina.DTOs.Ausencias
{
    public class GetSolicitudesResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? Estado { get; set; }
        public DateOnly FechaSolicitud { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
    }
}
