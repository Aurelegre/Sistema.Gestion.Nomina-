namespace Sistema.Gestion.Nómina.DTOs.Departamentos
{
    public class GetDepartamentosDTO
    {
        public string Descripcion { get; set; }
        public string jefe { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
