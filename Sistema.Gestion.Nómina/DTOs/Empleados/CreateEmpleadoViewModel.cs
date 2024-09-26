using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.DTOs.Roles;
using Sistema.Gestion.Nómina.DTOs.Usuarios;

namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class CreateEmpleadoViewModel
    {
        public List<GetPuestoDTO> Puestos { get; set; }
        public List<GetDepartamentoDTO> Departamentos { get; set;}
        public List<GetRolResponse> Roles { get; set; }
    }
}
