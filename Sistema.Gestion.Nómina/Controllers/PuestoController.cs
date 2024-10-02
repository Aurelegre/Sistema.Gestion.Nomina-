using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class PuestoController(SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        // GET: PuestoController
        public async Task<ActionResult> Index(GetPuestosDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Puestos.Where(p=> p.IdDepartamentoNavigation.IdEmpresa == session.company);

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
                    .Skip((request.page - 1)* request.pageSize)
                    .Take(request.pageSize)
                    .Select(p=> new GetPuestosResponse
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
                return View();
            }
        }

        // GET: PuestoController/Details/5
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
                puesto.Empleados = await context.Empleados.Where(p => p.IdPuesto == id).AsNoTracking().Select(e=> e.Nombre).ToListAsync();
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

        // GET: PuestoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PuestoController/Create
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

        // GET: PuestoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PuestoController/Edit/5
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

        // GET: PuestoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PuestoController/Delete/5
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
    }
}
