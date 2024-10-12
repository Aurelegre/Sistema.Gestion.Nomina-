namespace Sistema.Gestion.Nómina.DTOs.Ausencias
{
    public class GetSolicitudesResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? Estado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Depto { get; set; }
    }
}
