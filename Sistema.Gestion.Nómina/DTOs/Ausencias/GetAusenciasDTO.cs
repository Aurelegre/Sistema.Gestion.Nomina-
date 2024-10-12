namespace Sistema.Gestion.Nómina.DTOs.Ausencias
{
    public class GetAusenciasDTO
    {
        public int Estado { get; set; }
        public int Tipo { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
