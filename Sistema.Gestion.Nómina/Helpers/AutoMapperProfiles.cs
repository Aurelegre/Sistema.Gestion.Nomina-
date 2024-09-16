using AutoMapper;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.DTOs.Usuarios;
using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.Helpers
{
	public class AutoMapperProfiles : Profile
	{
		public AutoMapperProfiles() 
		{ 
			CreateMap<Puesto,GetPuestoDTO>();
            CreateMap<Departamento, GetDepartamentoDTO>();
            CreateMap<Usuario, GetUsuariosDTO>();
			CreateMap<CreateEmployeeDTO, Empleado>();

        }
    }
}
