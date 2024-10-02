using Sistema.Gestion.Nómina.DTOs.Empleados;

namespace Sistema.Gestion.Nómina.DTOs.Puestos
{
    public class GetPuestoResponse
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Departamento { get; set; }
        public List<string> Empleados { get; set;}
    }
}
