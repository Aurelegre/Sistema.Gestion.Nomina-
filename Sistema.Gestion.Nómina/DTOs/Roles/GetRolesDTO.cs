namespace Sistema.Gestion.Nómina.DTOs.Roles
{
    public class GetRolesDTO
    {
        public string Descripcion { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
