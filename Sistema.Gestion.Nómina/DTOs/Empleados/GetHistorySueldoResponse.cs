namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class GetHistorySueldoResponse
    {
        public string Nombre { get; set; }
        public List<GetHistorySueldoModal> History { get; set; }
    }
}
