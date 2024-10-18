using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.EmployeeDepto;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class EmployeeDeptoController(SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Index(GetEmployeesDeptoDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
                //traer el departamento del deptoEmpleado
                var deptoEmpleado = await context.Empleados.Where(e => e.Id == session.idEmpleado).AsNoTracking().Select(e=> e.IdDepartamento).FirstOrDefaultAsync();
                if (deptoEmpleado == null)
                {
                    TempData["Error"] = "Empleado sin departamento Asignado";
                    return RedirectToAction("Index", "Home");
                }
                var depto = await context.Departamentos.AsNoTracking().SingleAsync(e => e.Id == deptoEmpleado);
                //si es Jefe no traer su registro
                //Iniciar a traer los empleados del departamento seleccionado
                var query = context.Empleados.Where(e => e.Activo == 1 && e.IdDepartamento == depto.Id && e.Id != depto.IdJefe);

                //aplicar filtros
                if (!string.IsNullOrEmpty(request.DPI))
                {
                    query = query.Where(e => e.Dpi == request.DPI);
                }
                if (!string.IsNullOrEmpty(request.Nombre))
                {
                    query = query.Where(e => e.Nombre.Contains(request.Nombre));
                }
                if (!string.IsNullOrEmpty(request.Puesto))
                {
                    query = query.Where(e => e.IdPuestoNavigation.Descripcion == request.Puesto);
                }

                var totalItems = await query.CountAsync();
                var empleados = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GetEmployeesDeptoResponse
                    {
                        Id = u.Id,
                        Nombre = u.Nombre.Substring(0, u.Nombre.IndexOf(" ") != -1 ? u.Nombre.IndexOf(" ") : u.Nombre.Length) + " " + u.Apellidos.Substring(0, u.Apellidos.IndexOf(" ") != -1 ? u.Apellidos.IndexOf(" ") : u.Apellidos.Length),
                        Puesto = u.IdPuestoNavigation.Descripcion,
                        DPI = u.Dpi,
                    })
                    .ToListAsync();
                var paginatedResult = new PaginatedResult<GetEmployeesDeptoResponse>
                {
                    Items = empleados,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                // Pasar filtros a la vista
                ViewBag.DPI = request.DPI;
                ViewBag.Nombre = request.Nombre;
                ViewBag.Puesto = request.Puesto;
                ViewBag.Departamento = depto.Descripcion;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "EmployeeDepto.Index", $"Se consultaron todos los empleados del departamento {depto.Id}", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            { 
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.Index", "Error al realizar el Get de todos los empleados de un departamento", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Empleados del Departamento";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var empleado = await context.Empleados
                                           .Where(u => u.Id == id)
                                           .AsNoTracking()
                                           .Select(u => new GetEmployeeDeptoDTO
                                           {
                                               Id = u.Id,
                                               Nombre = u.Nombre,
                                               Apellidos = u.Apellidos,
                                               Puesto = u.IdPuestoNavigation.Descripcion,
                                               Departamento = u.IdDepartamentoNavigation.Descripcion,
                                               DPI = u.Dpi,
                                               Sueldo = u.Sueldo,
                                               FechaContratado = DateOnly.FromDateTime(u.FechaContratado),
                                           }).FirstOrDefaultAsync();

                if (empleado == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Empleado";
                    return RedirectToAction("Index", "EmployeeDepto");
                }

                await logger.LogTransaction(session.idEmpleado, session.company, "EmployeeDepto.Details", $"Se consultaron detalles del empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                return Json(empleado);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.Details", $"Error al consultar detalles del empleado con id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Empleado";
                return RedirectToAction("Index", "EmployeeDepto");
            }
        }
    }
}
