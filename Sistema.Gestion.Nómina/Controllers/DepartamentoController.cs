using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class DepartamentoController(SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
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
                                                           Jefe = d.IdJefeNavigation.Nombre
                                                       }).FirstOrDefaultAsync();
                if(depto == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Departamento";
                    return RedirectToAction("Index", "Departamento");
                }

                depto.Puestos = await ObtenerPuestos(id);

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
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DepartamentoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DepartamentoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DepartamentoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DepartamentoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
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
    }
}
