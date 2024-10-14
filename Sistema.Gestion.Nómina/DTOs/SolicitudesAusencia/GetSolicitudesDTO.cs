namespace Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia
{
    public class GetSolicitudesDTO
    {

        public string Empleado { get; set; }
        public DateTime? fechaSoli { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
