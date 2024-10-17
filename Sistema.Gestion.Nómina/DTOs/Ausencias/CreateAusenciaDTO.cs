namespace Sistema.Gestion.Nómina.DTOs.Ausencias
{
    public class CreateAusenciaDTO
    {
        public string Detalle { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
    }
}
