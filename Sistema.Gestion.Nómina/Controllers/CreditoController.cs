using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Creditos;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class CreditoController(SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        [HttpGet]
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

        [HttpGet]
        public async Task<ActionResult> Details (int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var credito = await context.Prestamos.Where(e => e.Id == id)
                                                     .AsNoTracking()
                                                     .Select(e => new
                                                     {
                                                         Id = e.Id,
                                                         cuotasP = e.CuotasPendientes,
                                                         TotalP = e.TotalPendiente
                                                     })
                                                     .FirstOrDefaultAsync();
                if(credito == null)
                {
                    TempData["Error"] = "Error al consultar Credito";
                    return RedirectToAction("Index", "Credito");
                }
                var historial = await context.HistorialPagos.Where(h => h.IdPrestamo == id)
                                                                           .AsNoTracking()
                                                                           .Select(e => new GetCreditoHistorialDTO
                                                                           {
                                                                               fecha = DateOnly.FromDateTime(e.FechaPago),
                                                                               totalPagado = e.TotalPagado,
                                                                               totalPediente = e.TotalPendiente
                                                                           }).ToListAsync();
                if (historial == null)
                {
                    TempData["Error"] = "No se han registrado pagos";
                    return RedirectToAction("Index", "Credito");
                }

                GetCreditoHistorialResponse historialResponse = new GetCreditoHistorialResponse
                {
                    Id = credito.Id,
                    CPendientes = credito.cuotasP,
                    TotalPediente = credito.TotalP,
                    Pagos = historial
                };

                await logger.LogTransaction(session.idEmpleado, session.company, "Credito.Details", $"Se consultó detalle de pagos del Credito: {id}", session.nombre);

                return Json(historialResponse);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Credito.Details", $"Error al consultar detalle de pagos del Credito: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Credito";
                return RedirectToAction("Index", "Credito");
            }
        }

        [HttpGet]
        public async Task<ActionResult> HistoryActive()
        {
            var session = logger.GetSessionData();
            try
            {
                //traer los créditos activos del usuario
                var creditos = await context.Prestamos.Where(e => e.IdEmpleado == session.idEmpleado && e.Pagado == 1 && e.IdTipo == 2)
                                                      .AsNoTracking()
                                                      .Select(e => new GetActiveCreditHistorialDTO
                                                      {
                                                          Id = e.Id,
                                                          CPendientes = e.CuotasPendientes,
                                                          TotalPediente = e.TotalPendiente,
                                                          fecha = DateOnly.FromDateTime(e.FechaPrestamo),
                                                          Pagos = context.HistorialPagos.Where(h => h.IdPrestamo == e.Id)
                                                                                        .AsNoTracking()
                                                                                        .Select(e => new GetCreditoHistorialDTO
                                                                                        {
                                                                                            fecha = DateOnly.FromDateTime(e.FechaPago),
                                                                                            totalPagado = e.TotalPagado,
                                                                                            totalPediente = e.TotalPendiente
                                                                                        }).ToList()
                                                      })
                                                      .OrderByDescending(e => e.fecha)
                                                      .ToListAsync();
                if(creditos == null)
                {
                    TempData["Error"] = "Error al Historial de Créditos activos";
                    return RedirectToAction("Index", "Credito");
                }

                GetActiveCreditHistorialResponse getActiveCreditHistorialDTO = new GetActiveCreditHistorialResponse
                {
                    Historials = creditos
                };
                await logger.LogTransaction(session.idEmpleado, session.company, "Credito.HistoryActive", $"Se consultó Historial de créditos activos", session.nombre);

                return View(getActiveCreditHistorialDTO);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Credito.HistoryActive", $"Error al consultar Historial de créditos activos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Crédito";
                return RedirectToAction("Index", "Credito");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCreditoDTO request)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                var session = logger.GetSessionData();
                try
                {
                    var total = await context.Prestamos
                                                .Where(e => e.IdEmpleado == session.idEmpleado && e.IdTipo == 2)
                                                .AsNoTracking()
                                                .SumAsync(e => e.TotalPendiente);
                    total += request.Total;
                    var count = await context.Prestamos.CountAsync(e => e.IdEmpleado == session.idEmpleado && e.IdTipo == 1);
                    //si ya posee un prestamo, el credito a deber no puede ser mayor a 200
                    if (total > 200 && count >= 1)
                    {
                        TempData["Error"] = "No se puede registrar, supera el máximo crédito disponible Q.200.00";
                        return RedirectToAction("Index", "Credito");
                    }
                    Prestamo prestamo = new Prestamo
                    {
                        IdEmpleado = session.idEmpleado,
                        Total = request.Total,
                        Cuotas = request.Cuotas,
                        FechaPrestamo = DateTime.Now.Date,
                        Pagado = 1,
                        IdTipo = 2,
                        TotalPendiente = request.Total,
                        CuotasPendientes = request.Cuotas
                    };
                    context.Prestamos.Add(prestamo);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Guardar bitácora
                    await logger.LogTransaction(session.idEmpleado, session.company, "Credito.Create", $"Se registró Credito con id {prestamo.Id}, empleado: {session.idEmpleado}", session.nombre);

                    TempData["Message"] = "Credito registrado con éxito";
                    return RedirectToAction("Index", "Credito");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await logger.LogError(session.idEmpleado, session.company, "Credito.Create", "Error al registrar Crédito", ex.Message, ex.StackTrace);
                    TempData["Error"] = "No se pudo registrar Credito";
                    return RedirectToAction("Index", "Credito");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var credito = await context.Prestamos.Where(e=> e.Id == id).AsNoTracking().FirstOrDefaultAsync();
                if (credito == null)
                {
                    TempData["Error"] = "El crédito a eliminar no existe";
                    return RedirectToAction("Index", "Credito");
                }
                //validar si tiene pagos registrados
                var pagos = await context.HistorialPagos.AsNoTracking().AnyAsync(e => e.IdPrestamo == credito.Id);
                if (pagos)
                {
                    TempData["Error"] = "El crédito posee pagos registrados";
                    return RedirectToAction("Index", "Credito");
                }
                context.Remove(credito);
                await context.SaveChangesAsync();
                // Guardar bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Credito.Delete", $"Se eliminó prestamo con id {credito.Id}, empleado: {session.idEmpleado}", session.nombre);

                TempData["Message"] = "Crédito eliminado con éxito";
                return RedirectToAction("Index", "Credito");
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Credito.Delete", "Error al eliminó Credito", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo eliminar Crédito";
                return RedirectToAction("Index", "Credito");
            }
        }
    }
}
