using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Authorize]
    public class PuestoController(SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        // GET: PuestoController
        [Authorize(Policy = "Puesto.Listar")]
        public async Task<ActionResult> Index(GetPuestosDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Puestos.Where(p => p.IdDepartamentoNavigation.IdEmpresa == session.company);

                //aplicar empleados
                if (!string.IsNullOrEmpty(request.Descripcion))
                {
                    query = query.Where(e => e.Descripcion.Contains(request.Descripcion));
                }
                if (!string.IsNullOrEmpty(request.Departamento))
                {
                    query = query.Where(e => e.IdDepartamentoNavigation.Descripcion.Contains(request.Departamento));
                }
                //paginación
                var totalItems = await query.CountAsync();
                var puestos = await query
                    .AsNoTracking()
                    .OrderBy(p => p.IdDepartamentoNavigation.Descripcion)
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(p => new GetPuestosResponse
                    {
                        Id = p.Id,
                        Descripcion = p.Descripcion,
                        Departamento = p.IdDepartamentoNavigation.Descripcion
                    })
                    .ToListAsync();
                var paginatedResult = new PaginatedResult<GetPuestosResponse>
                {
                    Items = puestos,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                //pasar filtros a la vista
                ViewBag.Descripcion = request.Descripcion;
                ViewBag.Departamento = request.Departamento;

                //Registrar bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Puesto.Index", $"Se consultaron todos los empleados de la empresa {session.company} y se envió a la vista", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Puesto.Index", "Error al realizar el Get de todos los Puestos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Puestos";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Policy = "Puesto.Ver")]
        public async Task<ActionResult> GetDeptos()
        {
            try
            {
                var session = logger.GetSessionData();
                var deptos = await context.Departamentos.Where(p => p.IdEmpresa == session.company).AsNoTracking().Select(p => new
                {
                    id = p.Id,
                    descripcion = p.Descripcion,
                }).ToListAsync();
                var departamentos = new
                {
                    departamentos = deptos
                };
                await logger.LogTransaction(session.idEmpleado, session.company, "Puestos.GetDeptos", $"Se obtubieron todos los departamentos de la empresa con id: {session.company}", session.nombre);
                return Json(departamentos);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Puestos.GetDeptos", $"Error al obtener los departamendos de la empresa: {session.company}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al obtener puestos por departamento";
                return RedirectToAction("Index", "Puesto");
            }
        }
        // GET: PuestoController/Details/5
        [Authorize(Policy = "Puesto.Ver")]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var puesto = await context.Puestos.Where(p => p.Id == id)
                                            .AsNoTracking()
                                            .Select(p => new GetPuestoResponse
                                            {
                                                Id = p.Id,
                                                Descripcion = p.Descripcion,
                                                Departamento = p.IdDepartamentoNavigation.Descripcion
                                            })
                                            .FirstOrDefaultAsync();
                if (puesto == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Puesto";
                    return RedirectToAction("Index", "Puesto");
                }
                puesto.Empleados = await context.Empleados.Where(p => p.IdPuesto == id).AsNoTracking().Select(e => e.Nombre).ToListAsync();
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Puesto.Details", $"Se consultaron detalles del puesto con id: {id}", session.nombre);

                return Json(puesto);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Puesto.Details", $"Error al consultar detalles del Puesto con id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Puesto";
                return RedirectToAction("Index", "Rol");
            }
        }


        // POST: PuestoController/Create
        [HttpPost]
        [Authorize(Policy = "Puesto.Crear")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreatePuestoDTO request)
        {
            try
            {
                var exist = await context.Puestos.AnyAsync(p => p.Descripcion == request.Descripcion && p.IdDepartamento == request.IdDepartamento);
                if (exist)
                {
                    TempData["Error"] = "El puesto ya existe dentro del Departamento seleccionado";
                    return RedirectToAction("Index", request.Vista);
                }
                //crear puesto
                Puesto puesto = new Puesto
                {
                    Descripcion = request.Descripcion,
                    IdDepartamento = request.IdDepartamento
                };
                context.Puestos.Add(puesto);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Puesto.Create", $"Se creó puesto con id: {puesto.Id}, Nombre: {puesto.Descripcion}", session.nombre);

                TempData["Message"] = "Puesto creado con éxito";
                return RedirectToAction("Index", request.Vista);

            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Puesto.Create", "Error al crear puesto", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo crear el Puesto";
                return RedirectToAction("Index", request.Vista);
            }
        }


        // POST: PuestoController/Edit/5
        [HttpPost]
        [Authorize(Policy = "Puesto.Actualizar")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdatePuestoDTO request)
        {
            try
            {
                var puesto = await context.Puestos.AsNoTracking().SingleOrDefaultAsync(p => p.Id == request.id);
                if (puesto == null)
                {
                    TempData["Error"] = "Puesto no encontrado";
                    return RedirectToAction("Index", "Puesto");
                }
                puesto.Descripcion = request.descripcion;
                context.Puestos.Update(puesto);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Puesto.Update", $"Se actualizó puesto con id: {puesto.Id}, Nombre: {puesto.Descripcion}", session.nombre);

                TempData["Message"] = "Puesto Actualizado con Exito";
                return RedirectToAction("Index", "Puesto");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Puesto.Update", $"Error al acualizar Puesto con id {request.id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo actualizar Puesto";
                return RedirectToAction("Index", "Puesto");
            }
        }

        // POST: PuestoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Puesto.Eliminar")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var puesto = await context.Puestos.AsNoTracking().SingleOrDefaultAsync(d => d.Id == id);
                if (puesto == null)
                {
                    TempData["Error"] = "El Puesto no existe";
                    return RedirectToAction("Index", "Puesto");
                }

                //verificar que no tenga usuarios asignados
                var employee = await context.Empleados.CountAsync(e => e.IdPuesto == id);
                if(employee != 0)
                {
                    TempData["Error"] = "No se puede eliminar el puesto con empleados asignados.";
                    return RedirectToAction("Index", "Puesto");
                }

                //eliminar puesto
                context.Puestos.Remove(puesto);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Puesto.Delete", $"Se eliminó Puesto con id: {puesto.Id}, Nombre: {puesto.Descripcion}", session.nombre);

                TempData["Message"] = "Puesto eliminado exitosamente.";
                return RedirectToAction("Index", "Puesto");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Puesto.Delete", $"Error al eliminar Puesto con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Ocurrió un error al intentar eliminar el puesto";
                return RedirectToAction("Index", "Puesto");
            }
        }
    }
}
