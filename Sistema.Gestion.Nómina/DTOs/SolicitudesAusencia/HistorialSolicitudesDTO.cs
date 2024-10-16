namespace Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia
{
    public class HistorialSolicitudesDTO
    {
        public string Estado { get; set; }
        public string? Empleado { get; set; }
        public DateOnly? Fecha { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
