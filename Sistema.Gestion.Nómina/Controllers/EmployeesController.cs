using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    [Authorize]
    public class EmployeesController(SistemaGestionNominaContext context, ILogServices logServices) : Controller
    {
        public async Task<ActionResult <IEnumerable< EmpleadosDTO>>> Index()
        {
            var empleados =  context.Empleados.Where(u => u.Activo == 1).Select(u => new EmpleadosDTO
            {
                Id= u.Id,
                Nombre = u.Nombre,
                Puesto = u.IdPuestoNavigation.Descripcion,
                Departamento = u.IdPuestoNavigation.IdDepartamentoNavigation.Descripcion,
                DPI = u.Dpi
			}).ToList();

            return View(empleados);
        }

        [HttpGet]
		public async Task<ActionResult<EmpleadosDTO>> Details(int id)
		{
			var empleados = context.Empleados.Where(u => u.Id == id).Select(u => new EmpleadosDTO
			{
				Id = u.Id,
				Nombre = u.Nombre,
				Puesto = u.IdPuestoNavigation.Descripcion,
				Departamento = u.IdPuestoNavigation.IdDepartamentoNavigation.Descripcion,
				DPI = u.Dpi,
				Sueldo = u.Sueldo,
				FechaContratado = u.FechaContratado,
				Usuario = u.IdUsuarioNavigation.Usuario1
			}).ToList();

			return View(empleados);
		}

		//[HttpPost]
		//public async Task<IActionResult>
	}
}
