using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
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
                await logger.LogError(session.idEmpleado, session.company, "Employees.Index", "Error al realizar el Get de todos los empleados activos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Empleados";
                return View();
            }
        }

        // GET: DepartamentoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DepartamentoController/Create
        public ActionResult Create()
        {
            return View();
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
    }
}
