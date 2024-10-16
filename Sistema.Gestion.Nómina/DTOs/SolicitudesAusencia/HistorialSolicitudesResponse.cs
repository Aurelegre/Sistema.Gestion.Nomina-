namespace Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia
{
    public class HistorialSolicitudesResponse
    {
        public string NombreJefe { get; set; }
        public DateOnly Fecha { get; set; }
        public string Descripcion { get; set; }
        public string? NombreEmpleado { get; set; }
        public int IdAusencia { get; set; }
    }
}
