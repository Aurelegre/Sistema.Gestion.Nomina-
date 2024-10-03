using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.DTOs.Roles;
using Sistema.Gestion.Nómina.DTOs.Usuarios;
using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class UpdateEmpleadoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<GetPuestoDTO> Puestos { get; set; }
        public List<GetDepartamentoDTO> Departamento { get; set; }
        public List<GetRolResponse> Roles { get; set; }
        public string Usuario { get; set; }
        public decimal? Sueldo { get; set; }
        public int? idDepto { get; set; }
        public int? idRol { get; set; }
        public int? idPuesto { get; set; }
    }
}
