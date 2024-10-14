using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Ausencias;
using Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class SolicitudesAusenciasController (SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Index(GetSolicitudesDTO request)
        {
            var session = logger.GetSessionData();
            try
            {

                var depto = await context.Departamentos.SingleAsync(d => d.IdJefe == session.idEmpleado);
                var query = context.Ausencias.Where(a => a.IdEmpleadoNavigation.IdDepartamento == depto.Id && a.Autorizado == 2);
                //aplicar filtros
                if (!string.IsNullOrEmpty(request.Empleado))
                {
                    query = query.Where(a => a.IdEmpleadoNavigation.Nombre.Contains(request.Empleado));
                }
                if (request.fechaSoli.HasValue)
                {
                    query = query.Where(a => a.FechaSolicitud == request.fechaSoli.Value);
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
                        Estado = u.Autorizado,
                        FechaSolicitud = DateOnly.FromDateTime(u.FechaSolicitud),
                        FechaInicio = DateOnly.FromDateTime(u.FechaInicio),
                        FechaFin = DateOnly.FromDateTime(u.FechaFin),
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
                ViewBag.Depto = depto.Descripcion;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "SolicitudesAusencias.Index", $"Se consultaron todas las solicitudes del departamendo {depto.Id} {depto.Descripcion}", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "SolicitudesAusencias.Index", "Error al consultar las solicitudes pendientes.", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar solicitudes";
                return View();
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(AuthorizeDTO request)
        {
            var session = logger.GetSessionData();
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
                await logger.LogTransaction(session.idEmpleado, session.company, "SolicitudesAusencias.Update", $"Fue {accion} la ausencia del empleado id: {ausencia.IdEmpleado}, por parte de el empleado: {request.Id}", session.nombre);

                TempData["Message"] = $"Ausencia {accion} con éxito";
                return RedirectToAction("Index", "Ausencias");
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "SolicitudesAusencias.Update", "Error al Autorizar o denegar Solicitud de ausencia", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo crear el empleado";
                return RedirectToAction("Index", "Ausencias");
            }
        }
    }
}

