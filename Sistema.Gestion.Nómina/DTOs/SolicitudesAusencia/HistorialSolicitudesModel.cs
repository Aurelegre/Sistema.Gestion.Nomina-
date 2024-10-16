namespace Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia
{
    public class HistorialSolicitudesModel
    {
        public string NombreJefe { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public string? NombreEmpleado { get; set; }
        public int IdAusencia { get; set; }
    }
}
