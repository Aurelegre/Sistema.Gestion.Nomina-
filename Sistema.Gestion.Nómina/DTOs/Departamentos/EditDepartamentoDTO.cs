namespace Sistema.Gestion.Nómina.DTOs.Departamentos
{
    public class EditDepartamentoDTO
    {
        public int Id { get; set; }
        public int? idJefe { get; set; }
        public string Descripcion { get; set; }
    }
}
