namespace Sistema.Gestion.Nómina.DTOs.Puestos
{
    public class GetPuestosDTO
    {
        public string Descripcion { get; set; }
        public string Departamento { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
