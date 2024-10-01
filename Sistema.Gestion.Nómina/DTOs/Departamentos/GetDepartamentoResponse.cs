using Sistema.Gestion.Nómina.DTOs.Puestos;

namespace Sistema.Gestion.Nómina.DTOs.Departamentos
{
    public class GetDepartamentoResponse
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Jefe { get; set; }
        public List<GetPuestoDTO> Puestos { get; set; } = new List<GetPuestoDTO>();
    }
}
