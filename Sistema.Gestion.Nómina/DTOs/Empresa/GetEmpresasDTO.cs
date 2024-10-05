namespace Sistema.Gestion.Nómina.DTOs.Empresa
{
    public class GetEmpresasDTO
    {
        public  string Nombre { get; set; }
        public  string Telefono { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
