using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using NuGet.Protocol;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Familia;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.DTOs.Usuarios;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    [Authorize]
    public class EmployeesController(SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        public async Task<ActionResult<IEnumerable<GETEmpleadosResponse>>> Index(GetEmployeesDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Empleados
                    .Where(e => e.Activo == 1 && e.IdEmpresa == session.company);

                // Aplicar filtros
                if (!string.IsNullOrEmpty(request.DPI))
                {
                    query = query.Where(e => e.Dpi == request.DPI);
                }
                if (!string.IsNullOrEmpty(request.Nombre))
                {
                    query = query.Where(e => e.Nombre.Contains(request.Nombre));
                }
                if (!string.IsNullOrEmpty(request.Departamento))
                {
                    query = query.Where(e => e.IdDepartamentoNavigation.Descripcion == request.Departamento);
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
                    .Select(u => new GETEmpleadosResponse
                    {
                        Id = u.Id,
                        Nombre = u.Nombre,
                        Puesto = u.IdPuestoNavigation.Descripcion,
                        Departamento = u.IdDepartamentoNavigation.Descripcion,
                        DPI = u.Dpi
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<GETEmpleadosResponse>
                {
                    Items = empleados,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                // Pasar filtros a la vista
                ViewBag.DPI = request.DPI;
                ViewBag.Nombre = request.Nombre;
                ViewBag.Departamento = request.Departamento;
                ViewBag.Puesto = request.Puesto;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Index", $"Se consultaron todos los empleados activos de la empresa {session.company} y se envió a la vista", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Index", "Error al realizar el Get de todos los empleados activos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Empleados";
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
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

                empleado.Family = await ObtenerFamily(id);

                if (empleado == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Empleado";
                    return RedirectToAction("Index", "Employees");
                }

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Details", $"Se consultaron detalles del empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                return Json(empleado);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Details", $"Error al consultar detalles del empleado con Id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }

		[HttpGet]
		public async Task<ActionResult> Update (int id)
        {
            try
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

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Update", $"Se obtubieron datos para actualizar empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                return Json(empleado);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Update", $"Error al obtener datos para actualizar empleado con Id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al obtener datos para actualizar Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }

        [HttpGet]
        public async Task<ActionResult> Create(int id)
        {
            try
            {
                CreateEmpleadoViewModel Datos = new CreateEmpleadoViewModel
                {
                    Departamentos = await ObtenerDepartamentos(),
                    Puestos = await ObtenerPuestos(id),
                    Usuarios = await ObtenerUsuariosSinAsignar(),
                };
                if (Datos == null)
                {
                    TempData["Error"] = "Error al obtener datos para crear Empleado";
                    return RedirectToAction("Index", "Employees");
                }

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Create", $"Se obtubieron datos para crear empleado", session.nombre);

                return Json(Datos);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Create", $"Error al obtener datos para crear empleado", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al obtener datos para crear Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> GetPuestos (int id)
        {
            try
            {
                var puestos = new
                {
                    puestos = await ObtenerPuestos(id)
                };

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.GetPuestos", $"Se obtubieron todos los puestos del departamento con id: {id}", session.nombre);

                return Json(puestos);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.GetPuestos", $"Error al obtener los puestos del departamento: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al obtener puestos por departamento";
                return RedirectToAction("Index", "Employees");
            }

        }
        [HttpPost]
        public async Task<ActionResult> Update(UpdateEmpleadoDTO request, int id)
        {
            try
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
                empleado.IdUsuario = request.IdUsuario != 0 ? request.IdUsuario : empleado.IdUsuario;


                context.Update(empleado);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Update", $"Se actualizó empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                TempData["Message"] = "Empleado Actualizado con Exito";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Update", $"Error al acualizar empleado con id {request.Id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo actualizar Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }

        [HttpPost]
        public async Task<ActionResult> Delete (int id)
        {
            try
            {
                var empleado = await context.Empleados.Where(p => p.Id == id).AsNoTracking().FirstOrDefaultAsync();
                if (empleado == null)
                {
                    TempData["Error"] = "El Empleado no existe";
                    return RedirectToAction("Index", "Employees");
                }
                //verificar que no sea jefe de departamento
                var depto = await context.Departamentos.Where(d => d.IdJefe == empleado.Id).AsNoTracking().FirstOrDefaultAsync();
                if (depto != null)
                {
                    TempData["Error"] = "No se pueden eliminar Empleados jefes de Departamento";
                    return RedirectToAction("Index", "Employees");
                }
                //desactivar usuario
                empleado.Activo = 0;
                //actualizar
                context.Update(empleado);
                await context.SaveChangesAsync();
                //guardar bitácora
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Delete", $"Se eliminó empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                TempData["Message"] = "Empleado Eliminado con Exito";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Delete", $"Error al eliminar empleado con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo eliminar Empleado";
                return RedirectToAction("Index", "Employees");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateEmployeeDTO request)
        {
            try
            {
                //verificar que no existan usuarios con el mismo DPI
                var existe = await context.Empleados.Where(e => e.Dpi == request.Dpi).AsNoTracking().FirstOrDefaultAsync();
                if (existe != null)
                {
                    TempData["Error"] = "El DPI ya esta registrado en otro empleado";
                    return RedirectToAction("Index", "Employees");
                }
                TimeOnly timeOnly = new TimeOnly(00, 00, 00);
                var session = logger.GetSessionData();
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
                //agregar a la BD
                context.Empleados.Add(empleado);
                await context.SaveChangesAsync();

                int? jefefam = await context.Empleados
                                    .Where(e => e.Dpi == request.Dpi)
                                    .AsNoTracking()
                                    .Select(u => u.Id) // Devuelve directamente el Id
                                    .FirstOrDefaultAsync();

                foreach (var familia in request.FamilyEmployeeDTOs)
                {
                    if (familia != null)
                    {
                        Familia familiar = new Familia
                        {
                            Nombre = familia.Nombre,
                            Edad = familia.Edad,
                            Parentesco = familia.Parentesco,
                            IdEmpleado = jefefam
                        };
                        context.Add(familiar);
                    }
                }
                await context.SaveChangesAsync();

                //guardar bitacora
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Create", $"Se creó empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);
                TempData["Message"] = "Empleado creado con Exito";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Create", $"Error al creaer empleado", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo crear Empleado";
                return RedirectToAction("Index", "Employees");
            }


        }
        private async Task<List<GetPuestoDTO>> ObtenerPuestos(int? idDepartamento)
        {
            try
            {
                var puestos = await context.Puestos.Where(p => p.IdDepartamento == idDepartamento).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetPuestoDTO>>(puestos);

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerPuestos", $"Se consultaron puestos del departamento {idDepartamento}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerPuestos", $"Error al consultar puestos del departamento {idDepartamento}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<List<GetDepartamentoDTO>> ObtenerDepartamentos()
        {
            try 
            {
                var session = logger.GetSessionData();
                var departamentos = await context.Departamentos.Where(d => d.IdEmpresa == session.company).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetDepartamentoDTO>>(departamentos);


                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerDepartamentos", $"Se consultaron departamentos de la empresa {session.company}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerDepartamentos", $"Error al consultar departamentos", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<List<GetUsuariosDTO>> ObtenerUsuariosSinAsignar()
        {
            try 
            {
                var session = logger.GetSessionData();
                var usuariosNoAsignados = await context.Usuarios
                                                       .Where(u => u.activo == 1 && u.IdEmpresa == session.company) // Filtrar usuarios activos de la empresa actual
                                                       .GroupJoin(
                                                           context.Empleados.Where(e => e.IdEmpresa == session.company),
                                                           u => u.Id,
                                                           e => e.IdUsuario,
                                                           (u, e) => new { Usuario = u, Empleados = e }
                                                       )
                                                       .SelectMany(
                                                           ue => ue.Empleados.DefaultIfEmpty(),
                                                           (ue, empleado) => new { ue.Usuario, empleado }
                                                       )
                                                       .Where(joinResult => joinResult.empleado == null) // Usuarios sin empleado asignado
                                                       .Select(joinResult => joinResult.Usuario)
                                                       .AsNoTracking()
                                                       .ToListAsync();


                //var usuariosNoAsignados = await context.Usuarios
                //                                        .Where(u => !context.Empleados.Any(e => e.IdUsuario == u.Id && e.IdEmpresa == session.company)
                //                                                    && u.IdEmpresa == session.company)
                //                                        .AsNoTracking()
                //                                        .ToListAsync();

                var listado = _mapper.Map<List<GetUsuariosDTO>>(usuariosNoAsignados);

                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerUsuariosSinAsignar", $"Se consultaron Usuarios sin asignar de la empresa {session.company}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerUsuariosSinAsignar", $"Error al consultar Usuarios sin asignar de la emoesa {session.company}", ex.Message, ex.StackTrace);
                return null;
            }


        }
        private async Task<List<GetFamilyEmployeeDTO>> ObtenerFamily(int? idEmpleado)
        {
            try
            {
                var Familia = await context.Familias.Where(p => p.IdEmpleado == idEmpleado).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetFamilyEmployeeDTO>>(Familia);

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerFamily", $"Se consultaron familiares del empleado {idEmpleado}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerPuestos", $"Error al consultar familiares del empleado: {idEmpleado}", ex.Message, ex.StackTrace);
                return null;
            }

        }
    }
}
