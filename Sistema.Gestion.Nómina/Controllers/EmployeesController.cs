using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using NuGet.Protocol;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.DTOs.Usuarios;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    [Authorize]
    public class EmployeesController(SistemaGestionNominaContext context, ILogServices logServices, IMapper _mapper) : Controller
    {
        public async Task<ActionResult<IEnumerable<GETEmpleadosDTO>>> Index(int page = 1, int pageSize = 5)
        {
            var session = logServices.GetSessionData();
            var totalItems = await context.Empleados.CountAsync(e => e.Activo == 1 && e.IdEmpresa == session.company);
            var empleados = await context.Empleados
                .Where(u => u.Activo == 1 && u.IdEmpresa == session.company)
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new GETEmpleadosDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Puesto = u.IdPuestoNavigation.Descripcion,
                    Departamento = u.IdDepartamentoNavigation.Descripcion,
                    DPI = u.Dpi
                })
                .ToListAsync();

            var paginatedResult = new PaginatedResult<GETEmpleadosDTO>
            {
                Items = empleados,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(paginatedResult);
        }


        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var empleado = await context.Empleados
                .Where(u => u.Id == id)
                .AsNoTracking()
                .Select(u => new GetEmpleadoDTO
			{
				Id = u.Id,
				Nombre = u.Nombre,
				Puesto = u.IdPuestoNavigation.Descripcion,
				Departamento = u.IdDepartamentoNavigation.Descripcion,
				DPI = u.Dpi,
				Sueldo = u.Sueldo,
				FechaContratado = DateOnly.FromDateTime(u.FechaContratado),
				Usuario = u.IdUsuarioNavigation.Usuario1
			}).FirstOrDefaultAsync();

            if (empleado == null)
            {
                return NotFound();
            }

            return Json(empleado);
		}

		[HttpGet]
		public async Task<ActionResult> Update (int id)
        {
            var empleado = await context.Empleados
                .Where(u => u.Id == id)
                .AsNoTracking()
                .Select(u => new UpdateEmpleadoViewModel
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Sueldo = u.Sueldo,
                    IdDepto = u.IdDepartamento,
                    Usuario = u.IdUsuarioNavigation.Usuario1
                }).FirstOrDefaultAsync();
            if (empleado == null)
            {
                return NotFound();
            }
            empleado.Puestos = await ObtenerPuestos(empleado.IdDepto);
            empleado.Departamento = await ObtenerDepartamentos();
            empleado.Usuarios = await ObtenerUsuariosSinAsignar();
            return Json(empleado);
        }

        [HttpGet]
        public async Task<ActionResult> Create(int id)
        {
            CreateEmpleadoViewModel Datos = new CreateEmpleadoViewModel
            {
                Departamentos = await ObtenerDepartamentos(),
                Puestos = await ObtenerPuestos(id),
                Usuarios = await ObtenerUsuariosSinAsignar(),
            };
            if (Datos == null){

            }
            return Json(Datos);
        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> GetPuestos (int id)
        {
            var puestos = new
            {
                puestos = await ObtenerPuestos(id)
            };

            return Json(puestos);
        }
        [HttpPost]
        public async Task<ActionResult> Update(UpdateEmpleadoDTO request, int id)
        {
            var empleado = await context.Empleados.Where(u => u.Id == id).AsNoTracking().FirstOrDefaultAsync();
            if (empleado == null)
            {
                return NotFound();
            }
            empleado.Nombre = request.Nombre;
            empleado.Sueldo = request.Sueldo;
            empleado.IdPuesto = request.IdPuesto;
            empleado.IdDepartamento = request.IdDepartamento;
            empleado.IdUsuario =  request.IdUsuario != 0 ? request.IdUsuario : empleado.IdUsuario;


            context.Update(empleado);
            await context.SaveChangesAsync();

            TempData["Message"] = "Empleado Actualizado con Exito";

            return RedirectToAction("Index", "Employees");
        }

        [HttpPost]
        public async Task<ActionResult> Delete (int id)
        {
            var empleado = await context.Empleados.Where(p=> p.Id == id).AsNoTracking().FirstOrDefaultAsync();
            if (empleado == null)
            {
                TempData["Error"] = "El Empleado no existe";
                return RedirectToAction("Index", "Employees");
            }
            //verificar que no sea jefe de departamento
            var depto = await context.Departamentos.Where(d => d.IdJefe == empleado.Id).AsNoTracking().FirstOrDefaultAsync();
            if(depto != null)
            {
                TempData["Error"] = "No se pueden eliminar Empleados jefes de Departamento";
                return RedirectToAction("Index", "Employees");
            }
            //desactivar usuario
            empleado.Activo = 0;
            TempData["Message"] = "Empleado Eliminado con Exito";
            return RedirectToAction("Index", "Employees");
        }


        [HttpPost]
        public async Task<ActionResult> Create(CreateEmployeeDTO request)
        {
            //verificar que no existan usuarios con el mismo DPI
            var existe = await context.Empleados.Where(e => e.Dpi == request.Dpi).AsNoTracking().FirstOrDefaultAsync();
            if(existe != null)
            {
                TempData["Error"] = "El DPI ya esta registrado en otro empleado";
                return RedirectToAction("Index", "Employees");
            }
            TimeOnly timeOnly = new TimeOnly(00, 00, 00);
            var session = logServices.GetSessionData();
            var empleado = new Empleado
            {
                Activo = request.Activo,
                Nombre = request.Nombre,
                IdEmpresa = session.company,
                IdPuesto = request.IdPuesto,
                IdDepartamento = request.IdDepartamento,
                Sueldo = request.Sueldo,
                FechaContratado = request.FechaContratado.ToDateTime(timeOnly),
                IdUsuario = request.IdUsuario,
                Dpi = request.Dpi,
            };
            if (empleado == null)
            {
                TempData["Error"] = "Error al crear usuario";
                return RedirectToAction("Index", "Employees");
            }
            context.Empleados.Add(empleado);
            await context.SaveChangesAsync();
            return RedirectToAction("Index", "Employees");

        }
        private async Task<List<GetPuestoDTO>> ObtenerPuestos(int? idDepartamento)
        {
            var puestos = await context.Puestos.Where(p => p.IdDepartamento == idDepartamento).AsNoTracking().ToListAsync();
            var listado = _mapper.Map<List<GetPuestoDTO>>(puestos);

            return listado;
        }
        private async Task<List<GetDepartamentoDTO>> ObtenerDepartamentos()
        {
            var departamentos = await context.Departamentos.AsNoTracking().ToListAsync();
            var listado = _mapper.Map<List<GetDepartamentoDTO>>(departamentos);

            return listado;
        }
        private async Task<List<GetUsuariosDTO>> ObtenerUsuariosSinAsignar()
        {
            var usuariosNoAsignados = await context.Usuarios
                                        .Where(u => !context.Empleados.Any(e => e.IdUsuario == u.Id))
                                        .AsNoTracking()
                                        .ToListAsync();
            var listado = _mapper.Map<List<GetUsuariosDTO>>(usuariosNoAsignados);

            return listado;
        }
    }
}
