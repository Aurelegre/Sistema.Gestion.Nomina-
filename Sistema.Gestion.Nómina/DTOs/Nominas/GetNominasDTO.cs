namespace Sistema.Gestion.Nómina.DTOs.Nominas
{
    public class GetNominasDTO
    {
        public DateOnly fecha { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public string Depto { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 15;
    }
}
