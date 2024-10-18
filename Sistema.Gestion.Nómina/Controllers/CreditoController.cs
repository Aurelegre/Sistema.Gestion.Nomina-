using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Creditos;
using Sistema.Gestion.Nómina.DTOs.Prestamos;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class CreditoController(SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        public async Task<ActionResult> Index(GetCreditosDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
                var query = context.Prestamos.Where(p => p.IdEmpleado == session.idEmpleado && p.IdTipo == 2);

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
                    .Select(u => new GetCreditosResponse
                    {
                        Id = u.Id,
                        Estado = u.Pagado,
                        Fecha = DateOnly.FromDateTime(u.FechaPrestamo),
                        Cuotas = u.Cuotas,
                        CPendientes = u.CuotasPendientes,
                        Total = u.Total,
                        TotalPendiente = u.TotalPendiente
                    })
                    .OrderByDescending(e => e.Fecha)
                    .ToListAsync();
                var paginatedResult = new PaginatedResult<GetCreditosResponse>
                {
                    Items = prestmos,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };
                ViewBag.Fecha = request.Fecha;
                ViewBag.Estado = request.Estado;

                await logger.LogTransaction(session.idEmpleado, session.company, "Creditos.Index", $"Se consultaron todas los creditos del Empleado {session.idEmpleado}", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Creditos.Index", $"Error al consultar los creditos del empleado {session.idEmpleado}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Créditos";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
