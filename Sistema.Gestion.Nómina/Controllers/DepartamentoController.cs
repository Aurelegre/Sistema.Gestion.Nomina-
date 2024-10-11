using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using System.Linq;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Authorize]
    public class DepartamentoController(SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        [HttpGet]
        [Authorize(Policy = "Departamento.Listar")]
        
        // GET: DepartamentoController
        public async Task<ActionResult> Index(GetDepartamentosDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Departamentos.Where(d => d.IdEmpresa == session.company);

                //aplicar filtros
                if (!string.IsNullOrEmpty(request.Descripcion)){
                    query = query.Where(e => e.Descripcion.Contains(request.Descripcion));
                }
                if (!string.IsNullOrEmpty(request.jefe))
                {
                    query = query.Where(e => e.IdJefeNavigation.Nombre.Contains(request.jefe));
                }

                //paginación
                var totalItems = await query.CountAsync();
                var deptos = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(d => new GetDepartamentosResponse
                    {
                        Id = d.Id,
                        Descripcion = d.Descripcion,
                        Jefe = d.IdJefeNavigation.Nombre
                    })
                    .ToListAsync();
                var paginatedResult = new PaginatedResult<GetDepartamentosResponse>
                {
                    Items = deptos,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                //pasar filtros a la vista
                ViewBag.Descripcion = request.Descripcion;
                ViewBag.Jefe = request.jefe;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Departamento.Index", $"Se consultaron todos los Departamentos de la empresa {session.company} y se envió a la vista", session.nombre);

                return View(paginatedResult);

            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Departamento.Index", "Error al realizar el Get de todos los Departamentos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Departamento";
                return View();
            }
        }

        [HttpGet]
        [Authorize(Policy = "Departamento.Ver")]
        // GET: DepartamentoController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var depto = await context.Departamentos.Where(d => d.Id == id)
                                                       .AsNoTracking()
                                                       .Select( d => new GetDepartamentoResponse
                                                       {
                                                           Id = d.Id,
                                                           Descripcion = d.Descripcion,
                                                           Jefe = d.IdJefeNavigation.Nombre,
                                                           IdJefe = d.IdJefe
                                                       }).FirstOrDefaultAsync();
                if(depto == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Departamento";
                    return RedirectToAction("Index", "Departamento");
                }

                depto.Puestos = await ObtenerPuestos(id);
                depto.empleadoPuesto = await Obtenerempleados(id, depto.IdJefe);

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Departamento.Details", $"Se consultaron detalles del departamendo con id: {id}", session.nombre);

                return Json(depto);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Departamento.Details", $"Error al consultar detalles del Departamento con idPermiso: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Departamento";
                return RedirectToAction("Index", "Rol");
            }
        }

        // POST: DepartamentoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Departamento.Crear")]
        public async Task<ActionResult> Create(CreateDepartamentoDTO request)
        {
            try
            {
                var exist = await context.Departamentos.AnyAsync(d=> d.Descripcion == request.Descripcion);
                if (exist)
                {
                    TempData["Error"] = "Ya existe un Departamento con este nombre";
                    return RedirectToAction("Index", "Departamento");
                }
                var session = logger.GetSessionData();
                Departamento depto =new Departamento
                {
                    Descripcion = request.Descripcion,
                    IdEmpresa = session.company,
                    IdJefe = null
                };
                context.Departamentos.Add(depto);
                await context.SaveChangesAsync();
                //crear el puesto JEFE
                Puesto puesto = new Puesto
                {
                    Descripcion = "Jefe",
                    IdDepartamento = depto.Id,
                };
                context.Puestos.Add(puesto);
                await context.SaveChangesAsync();

                //guardar bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Departamento.Create", $"Se creó departamento con id: {depto.Id}, Nombre: {depto.Descripcion}", session.nombre); ;
                TempData["Message"] = "Departamento creado con Exito";
                return RedirectToAction("Index", "Departamento");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Departamento.Create", $"Error al crear Departamento", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo crear Departamento";
                return RedirectToAction("Index", "Departamento");
            }
        }

        // POST: DepartamentoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Departamento.Actualizar")]
        public async Task<ActionResult> Edit(EditDepartamentoDTO request)
        {
            try
            {
                var depto = await context.Departamentos.SingleAsync(d => d.Id == request.Id);
                if (depto == null)
                {
                    TempData["Error"] = "El Departamento a editar no existe";
                    return RedirectToAction("Index", "Departamento");
                }
                depto.Descripcion = request.Descripcion;

                if(!(request.idJefe == depto.IdJefe))
                {
                    var newJefe = await context.Empleados
                                                .AsNoTracking()
                                                .SingleAsync(e => e.Id == request.idJefe);
                   
                    if(depto.IdJefe != null)
                    {
                        var oldjefe = await context.Empleados
                                              .AsNoTracking()
                                              .SingleAsync(e => e.Id == depto.IdJefe);
                        newJefe.IdPuesto = oldjefe.IdPuesto;// acutalizar el puesto del nuevo jefe
                        oldjefe.IdPuesto = null; // dejar al anterior jefe sin puestos
                        context.Empleados.Update(oldjefe);

                    }
                    else
                    {
                        var idPuesto = await context.Puestos
                                                    .AsNoTracking()
                                                    .SingleAsync(p => p.IdDepartamento == depto.Id && p.Descripcion.Equals("Jefe"));
                        if (idPuesto == null)
                        {
                            TempData["Error"] = "No existe el puesto Jefe dentro del Departamento";
                            return RedirectToAction("Index", "Departamento");
                        }
                        newJefe.IdPuesto = idPuesto.Id;
                    }
                    context.Empleados.Update(newJefe);
                    depto.IdJefe = request.idJefe;                 
                }
                context.Departamentos.Update(depto);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Departamento.Edit", $"Se actualizó Departamento con id: {depto.Id}, Nombre: {depto.Descripcion}", session.nombre);

                TempData["Message"] = "Departamento actualizado con Exito";
                return RedirectToAction("Index", "Departamento");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Departamento.Edit", $"Error al acualizar Departamento con id {request.Id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo actualizar Departamento";
                return RedirectToAction("Index", "Departamento");
            } 
        }

        // POST: DepartamentoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Departamento.Eliminar")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // Obtener el departamento por su Id
                var depto = await context.Departamentos.SingleOrDefaultAsync(d => d.Id == id);
                if (depto == null)
                {
                    TempData["Error"] = "El Departamento no existe";
                    return RedirectToAction("Index", "Departamento");
                }

                // Verificar si existen puestos asociados a este departamento
                var puestos = await context.Puestos.Where(p => p.IdDepartamento == id).ToListAsync();
                if (puestos.Any())
                {
                    var puestosConEmpleados = await context.Puestos
                                                            .Where(p => p.IdDepartamento == id && context.Empleados.Any(e => e.IdPuesto == p.Id))
                                                            .ToListAsync();

                    if (puestosConEmpleados.Any())
                    {
                        TempData["Error"] = "No se puede eliminar el departamento, ya que hay empleados asignados a puestos de este departamento.";
                        return RedirectToAction("Index", "Departamento");
                    }
                }

                // Si no hay empleados en puestos del departamento, se puede proceder con la eliminación de los puestos
                context.Puestos.RemoveRange(puestos);

                // Eliminar el departamento
                context.Departamentos.Remove(depto);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Departamento.Delete", $"Se eliminó Departamento con id: {depto.Id}, Nombre: {depto.Descripcion}", session.nombre);

                TempData["Message"] = "Departamento eliminado exitosamente.";
                return RedirectToAction("Index", "Departamento");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Departamento.Delete", $"Error al eliminar Departamento con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Ocurrió un error al intentar eliminar el departamento";
                return RedirectToAction("Index", "Departamento");
            }
        }

        private async Task<List<GetPuestoDTO>> ObtenerPuestos(int? idDepartamento)
        {
            try
            {
                var puestos = await context.Puestos.Where(p => p.IdDepartamento == idDepartamento).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetPuestoDTO>>(puestos);

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Departamento.ObtenerPuestos", $"Se consultaron puestos del departamento {idDepartamento}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Departamento.ObtenerPuestos", $"Error al consultar puestos del departamento {idDepartamento}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<List<EmpleadoPuestoDTO>> Obtenerempleados(int? idDepartamento, int? idJefe)
        {
            try
            {
                var empleado = await context.Empleados
                                            .Where(p => p.IdDepartamento == idDepartamento && p.Id != idJefe)
                                            .AsNoTracking()
                                            .Select(e=> new EmpleadoPuestoDTO
                                            {
                                                idEmpleado = e.Id,
                                                Empleado = e.Nombre,
                                                idPuesto = e.IdPuesto,
                                                Puesto = e.IdPuestoNavigation.Descripcion
                                            })
                                            .ToListAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Departamento.Empleados", $"Se consultaron Empleados del departamento {idDepartamento}", session.nombre);

                return empleado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Departamento.Empleados", $"Error al consultar Empleados del departamento {idDepartamento}", ex.Message, ex.StackTrace);
                return null;
            }

        }
    }
}
