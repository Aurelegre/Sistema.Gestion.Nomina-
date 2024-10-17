namespace Sistema.Gestion.Nómina.DTOs.Ausencias
{
    public class EditAusenciaDTO
    {
        public int  Id { get; set; }
        public DateTime FechaIni { get; set; }
        public DateTime FechaFin { get; set; }
        public string? Detalle { get; set; }
    }
}
