using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Prestamos;
using Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class PrestamoController(SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        // GET: PrestamoController
        public async Task<ActionResult> Index(GetPrestamosDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
                var query = context.Prestamos.Where(p => p.IdEmpleado == session.idEmpleado && p.IdTipo == 1);

                //aplicar filtros
                if (request.Estado != 0)
                {
                    //0 = Todos
                    //1 = SinPAgar
                    //2 = Pagados
                    query = query.Where(q => q.Pagado == request.Estado);
                }
                if (request.Fecha.HasValue)
                {
                    query = query.Where(a => DateOnly.FromDateTime(a.FechaPrestamo) == request.Fecha.Value);
                }
                var totalItems = await query.CountAsync();
                var prestmos = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GetPrestamosResponse
                    {
                        Id = u.Id,
                        Estado = u.Pagado,
                        Fecha = DateOnly.FromDateTime(u.FechaPrestamo),
                        Cuotas = u.Cuotas,
                        CPendientes = u.CuotasPendientes,
                        Total = u.Total,
                        TotalPendiente = u.TotalPendiente
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<GetPrestamosResponse>
                {
                    Items = prestmos,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };
                ViewBag.Fecha = request.Fecha;
                ViewBag.Estado = request.Estado;

                await logger.LogTransaction(session.idEmpleado, session.company, "Prestamos.Index", $"Se consultaron todas los prestamos del Empleado {session.idEmpleado}", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Prestamos.Index",$"Error al consultar los prestamos del empleado {session.idEmpleado}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Prestamos";
                return View();
            }
        }

        // GET: PrestamoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PrestamoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PrestamoController/Create
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

        // GET: PrestamoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PrestamoController/Edit/5
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

        // GET: PrestamoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PrestamoController/Delete/5
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
