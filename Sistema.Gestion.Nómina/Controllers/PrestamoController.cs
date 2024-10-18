using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using Sistema.Gestion.Nómina.DTOs.Prestamos;
using Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Authorize]
    [Controller]
    public class PrestamoController(SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        [HttpGet]
        [Authorize(Policy = "Prestamos.Listar")]
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
                    .OrderByDescending(e=> e.Fecha)
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
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        [Authorize(Policy = "Prestamos.Ver")]
        // GET: PrestamoController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var prestamo = await context.Prestamos.Where(e => e.Id == id)
                                                      .AsNoTracking()
                                                      .Select(e => new
                                                      {
                                                          Id = e.Id,
                                                          cuotasP = e.CuotasPendientes,
                                                          TotalP = e.TotalPendiente
                                                      })
                                                      .FirstOrDefaultAsync();
                if (prestamo == null)
                {
                    TempData["Error"] = "Error al consultar Prestamo";
                    return RedirectToAction("Index", "Prestamo");
                }
                var historial = await context.HistorialPagos.Where(h => h.IdPrestamo == id)
                                                            .AsNoTracking()
                                                            .Select(e => new GetHistorialDTO
                                                            {
                                                                fecha = DateOnly.FromDateTime(e.FechaPago),
                                                                totalPagado = e.TotalPagado,
                                                                totalPediente = e.TotalPendiente
                                                            }).ToListAsync();
                if(historial == null)
                {
                    TempData["Error"] = "No se han registrado pagos";
                    return RedirectToAction("Index", "Prestamo");
                }
                
                GetHistorialResponse historialResponse = new GetHistorialResponse
                {
                    Id = prestamo.Id,
                    CPendientes = prestamo.cuotasP,
                    TotalPediente = prestamo.TotalP,
                    Pagos = historial
                };

                await logger.LogTransaction(session.idEmpleado, session.company, "Prestamo.Details", $"Se consultó detalle de pagos del prestamo: {id}", session.nombre);

                return Json(historialResponse);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Prestamo.Details", $"Error al consultar detalle de pagos del prestamo: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Prestamo";
                return RedirectToAction("Index", "Prestamo");
            }
        }
        [HttpGet]
        [Authorize(Policy = "Prestamos.Ver")]
        public async Task<ActionResult> HistoryActive()
        {
            var session = logger.GetSessionData();
            try
            {
                //trear los prestamos activos del empleado 
                var prestamo = await context.Prestamos.Where(e => e.IdEmpleado == session.idEmpleado && e.Pagado == 1)
                                                      .AsNoTracking()
                                                      .Select(e => new GetActiveHistorialDTO
                                                      {
                                                          Id = e.Id,
                                                          CPendientes = e.CuotasPendientes,
                                                          TotalPediente = e.TotalPendiente,
                                                          fecha = DateOnly.FromDateTime(e.FechaPrestamo),
                                                          Pagos =  context.HistorialPagos.Where(h => h.IdPrestamo == e.Id)
                                                                                        .AsNoTracking()
                                                                                        .Select(e => new GetHistorialDTO
                                                                                        {
                                                                                            fecha = DateOnly.FromDateTime(e.FechaPago),
                                                                                            totalPagado = e.TotalPagado,
                                                                                            totalPediente = e.TotalPendiente
                                                                                        }).ToList()
                                                       })
                                                      .OrderByDescending(e=> e.fecha)
                                                      .ToListAsync();
                if (prestamo == null)
                {
                    TempData["Error"] = "Error al Historial de Prestamos activos";
                    return RedirectToAction("Index", "Prestamo");
                }

                GetActiveHistorialResponse historialResponse = new GetActiveHistorialResponse
                {
                    Historials = prestamo
                };

                await logger.LogTransaction(session.idEmpleado, session.company, "Prestamo.HistoryActive", $"Se consultó Historial de prestamos activos", session.nombre);

                return View(historialResponse);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Prestamo.HistoryActive", $"Error al consultar Historial de prestamos activos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Prestamo";
                return RedirectToAction("Index", "Prestamo");
            }
        }


        // POST: PrestamoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Prestamos.Crear")]
        public async Task<ActionResult> Create(CreatePrestamoDTO request)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                var session = logger.GetSessionData();
                try
                {
                    var count = await context.Prestamos.CountAsync(e => e.IdEmpleado == session.idEmpleado);
                    if (count == 2)
                    {
                        TempData["Error"] = "Máximo número de prestamos activos alcanzado";
                        return RedirectToAction("Index", "Prestamo");
                    }
                    Prestamo prestamo = new Prestamo
                    {
                        IdEmpleado = session.idEmpleado,
                        Total = request.Total,
                        Cuotas = request.Cuotas,
                        FechaPrestamo = DateTime.Now.Date,
                        Pagado = 1,
                        IdTipo = 1,
                        TotalPendiente = request.Total,
                        CuotasPendientes = request.Cuotas
                    };
                    context.Prestamos.Add(prestamo);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Guardar bitácora
                    await logger.LogTransaction(session.idEmpleado, session.company, "Prestamo.Create", $"Se registró prestamo con id {prestamo.Id}, empleado: {session.idEmpleado}", session.nombre);

                    TempData["Message"] = "Prestamo registrado con éxito";
                    return RedirectToAction("Index", "Prestamo");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await logger.LogError(session.idEmpleado, session.company, "Prestamo.Create", "Error al registrar prestamo", ex.Message, ex.StackTrace);
                    TempData["Error"] = "No se pudo registrar Prestamo";
                    return RedirectToAction("Index", "Prestamo");
                }
            }
        }

       

        // POST: PrestamoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Prestamos.Eliminar")]
        public async Task<ActionResult> Delete(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var prestamo = await context.Prestamos.Where(e => e.Id == id).AsNoTracking().FirstOrDefaultAsync();
                if(prestamo == null)
                {
                    TempData["Error"] = "El prestamo a eliminar no existe";
                    return RedirectToAction("Index", "Prestamo");
                }
                //validar si tiene pagos registrados
                var pagos = await context.HistorialPagos.AsNoTracking().AnyAsync(e => e.IdPrestamo == prestamo.Id);
                if (pagos)
                {
                    TempData["Error"] = "El prestamo posee pagos registrados";
                    return RedirectToAction("Index", "Prestamo");
                }
                context.Remove(prestamo);
                await context.SaveChangesAsync();
                // Guardar bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Prestamo.Delete", $"Se eliminó prestamo con id {prestamo.Id}, empleado: {session.idEmpleado}", session.nombre);

                TempData["Message"] = "Prestamo eliminado con éxito";
                return RedirectToAction("Index", "Prestamo");
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Prestamo.Delete", "Error al eliminó prestamo", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo eliminar Prestamo";
                return RedirectToAction("Index", "Prestamo");
            }
        }
    }
}
