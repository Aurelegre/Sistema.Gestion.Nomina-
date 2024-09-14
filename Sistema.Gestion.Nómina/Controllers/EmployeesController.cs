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
    public class EmployeesController : Controller
    {
		private readonly SistemaGestionNominaContext context;
		private readonly ILogServices logServices;

		public EmployeesController(SistemaGestionNominaContext context,ILogServices logServices)
        {
			this.context = context;
			this.logServices = logServices;
		}
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
    }
}
