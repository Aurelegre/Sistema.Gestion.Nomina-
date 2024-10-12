using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Ausencias;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class AusenciasController (SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        // GET: AusenciasController
        public async Task<ActionResult> Index(GetAusenciasDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Ausencias.Where(a => a.IdEmpleado == session.idEmpleado);

                //aplicar filtros
                if (request.Estado != 0)
                {
                    //0 = Todos
                    //1 = Autorizado
                    //2 = Pendiente
                    //3 = Denegados
                    query = query.Where(q => q.Autorizado == request.Estado);
                }
                if(request.Tipo != 0)
                {
                    //1 = traer deducible
                    //2 = traer no deducibles
                    query = query.Where(q => q.Deducible == request.Tipo);
                }
                bool isjefe = await context.Departamentos.AnyAsync(d => d.IdJefe == session.idEmpleado);
                var totalItems = await query.CountAsync();
                var ausencias = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GetAusenciasResponse
                    {
                        Id = u.Id,
                        Detalle = u.Detalle,
                        Estado = u.Autorizado,
                        Deducible = u.Deducible,
                        FechaSolicitud = DateOnly.FromDateTime( u.FechaSolicitud),
                        FechaInicio = DateOnly.FromDateTime( u.FechaInicio),
                        FechaFin = DateOnly.FromDateTime(u.FechaFin),
                        IsJefe = isjefe
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<GetAusenciasResponse>
                {
                    Items = ausencias,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };
                // Pasar filtros a la vista
                ViewBag.Tipo = request.Tipo;
                ViewBag.Estado = request.Estado;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Ausencias.Index", $"Se consultaron todas las ausencias del empleado{session.idEmpleado} y se envió a la vista", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Ausencias.Index", $"Error al realizar el Get de todas las ausencias del empleado: {session.idEmpleado}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Ausencias";
                return View();
            }
        }

        // GET: AusenciasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetSolicitudes(GetSolicitudesDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
               
                var depto = await context.Departamentos.SingleAsync(d=> d.IdJefe == session.idEmpleado);
                var query = context.Ausencias.Where(a => a.IdEmpleadoNavigation.IdDepartamento == depto.Id && a.Autorizado == 2);
                //aplicar filtros
                if (!string.IsNullOrEmpty(request.Empleado))
                {
                    query = query.Where(a => a.IdEmpleadoNavigation.Nombre.Contains(request.Empleado));
                }
                if (request.fechaSoli.HasValue)
                {
                    query = query.Where(a=> a.FechaSolicitud == request.fechaSoli.Value); 
                }
                var totalItems = await query.CountAsync();
                var solicitudes = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GetSolicitudesResponse
                    {
                        Id = u.Id,
                        Nombre = u.IdEmpleadoNavigation.Nombre.Substring(0, u.IdEmpleadoNavigation.Nombre.IndexOf(" ") != -1 ? u.IdEmpleadoNavigation.Nombre.IndexOf(" ") : u.IdEmpleadoNavigation.Nombre.Length) + " " + u.IdEmpleadoNavigation.Apellidos.Substring(0, u.IdEmpleadoNavigation.Apellidos.IndexOf(" ") != -1 ? u.IdEmpleadoNavigation.Apellidos.IndexOf(" ") : u.IdEmpleadoNavigation.Apellidos.Length),
                        FechaSolicitud = u.FechaSolicitud,
                        Estado= u.Autorizado,
                        FechaFin = u.FechaFin,
                        FechaInicio = u.FechaInicio,
                        Depto = depto.Descripcion
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<GetSolicitudesResponse>
                {
                    Items = solicitudes,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };
                // Pasar filtros a la vista
                ViewBag.Empleado = request.Empleado;
                ViewBag.Fecha = request.fechaSoli;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Ausencias.GetSolicitudes", $"Se consultaron todas las solicitudes del departamendo {depto.Id} {depto.Descripcion}", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Ausencias.GetSolicitudes", "Error al consultar las solicitudes pendientes.", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar solicitudes";
                return View();
            }
        }
        // POST: AusenciasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAusenciaDTO request)
        {
            var session = logger.GetSessionData();
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {   //estandarización
                    TimeOnly timeOnly = new TimeOnly(00, 00, 00);
                    var fechafin = request.FechaFin.ToDateTime(timeOnly);
                    var fechaInicio = request.FechaInicio.ToDateTime(timeOnly);
                    //verificar que no existe ausencia registrada en el rango establecido con anterioridad
                    var exist = await context.Ausencias.AnyAsync(a => a.FechaInicio < fechaInicio && a.FechaFin > fechaInicio && a.IdEmpleado == session.idEmpleado);
                    //verificar que la ausencia sea mayor a la fecha actual
                    var fecha = DateTime.Now > fechaInicio ? true : false;
                    //verificar que la fecha inicio sea menor a la del final
                    var mayor = fechaInicio > fechafin ? true : false;
                    if (exist)
                    {
                        TempData["Error"] = "Ya existe una ausencia registrada en este rango de fechas";
                        return RedirectToAction("Index", "Ausencias");
                    }
                    if (fecha || mayor)
                    {
                        TempData["Error"] = "Las Fechas ingresadas no son válidas";
                        return RedirectToAction("Index", "Ausencias");
                    }
                    Ausencia ausencia = new Ausencia
                    {
                        IdEmpleado = session.idEmpleado,
                        FechaSolicitud = DateTime.Now.Date,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaInicio,
                        TotalDias = (fechafin - fechaInicio).Days,
                        Detalle = request.Detalle,
                        Autorizado = 2,
                        Deducible = 0,
                        FechaAutorizado = null
                    };
                    context.Ausencias.Add(ausencia);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Guardar bitácora
                    await logger.LogTransaction(session.idEmpleado, session.company, "Ausencias.Create", $"Se agregó solicitud de ausencia con id: {ausencia.Id}, empleado: {session.idEmpleado}", session.nombre);

                    TempData["Message"] = "Solicitud de ausencia registrada con éxito";
                    return RedirectToAction("Index", "Ausencias");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await logger.LogError(session.idEmpleado, session.company, "Ausencias.Create", "Error al agregar solicitud de ausencia", ex.Message, ex.StackTrace);
                    TempData["Error"] = "No se pudo registrar la solicitud de ausencia";
                    return RedirectToAction("Index", "Ausencias");
                }
            }
        }
        [HttpPost]
        public async Task<ActionResult> Authorize(AuthorizeDTO request)
        {   var session = logger.GetSessionData();
            try
            {
                
                var ausencia = await context.Ausencias.SingleAsync(a => a.Id == request.Id);
                if (ausencia == null)
                {
                    TempData["Error"] = "La ausencia seleccionada no existe";
                    return RedirectToAction("Index", "Ausencias");
                }
                ausencia.Autorizado = request.Estado;
                ausencia.Deducible = request.Tipo;
                ausencia.FechaAutorizado = DateTime.Now;
                ausencia.idJefe = session.idEmpleado;
                context.Ausencias.Update(ausencia);
                await context.SaveChangesAsync();

                string accion = request.Estado == 1 ? "Autorizada" : request.Estado == 3 ? "Denegada" : "Error";

                //registrar bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Ausencias.Authorize", $"Fue {accion} la ausencia del empleado id: {ausencia.IdEmpleado}, por parte de el empleado: {request.Id}", session.nombre);

                TempData["Message"] = $"Ausencia {accion} con éxito";
                return RedirectToAction("Index", "Ausencias");
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Ausencias.Authorize", "Error al Autorizar o denegar Solicitud de ausencia", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo crear el empleado";
                return RedirectToAction("Index", "Ausencias");
            }
        }
        // GET: AusenciasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AusenciasController/Edit/5
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

        // GET: AusenciasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AusenciasController/Delete/5
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
