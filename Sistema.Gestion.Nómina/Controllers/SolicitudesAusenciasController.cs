using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Ausencias;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Empresa;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Authorize]
    [Controller]
    public class SolicitudesAusenciasController (SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        [HttpGet]
        [Authorize(Policy = "SolicitudesAusencias.Listar")]
        public async Task<ActionResult> Index(GetSolicitudesDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
                string depto = string.Empty;
                var query = context.Ausencias.Where(a =>  a.IdEmpleado != session.idEmpleado);
                if (session.rol.Equals("CEO"))
                {
                    depto = "Jefes";
                    //traer las solicitudes de los usuarios jefe de deptos
                    query = query.Where(a => context.Departamentos.Any(d => d.IdJefe == a.IdEmpleado));
                }
                else
                {
                    //traer el departamento del que es JEFE
                    var deptos = await context.Departamentos.SingleAsync(d => d.IdJefe == session.idEmpleado);
                    depto = deptos.Descripcion;
                    //traer todas las solicitudes de los departamentos menos la de el jefe
                    query = query.Where(a => a.IdEmpleadoNavigation.IdDepartamento == deptos.Id);
                }
                //aplicar filtros
                if (!string.IsNullOrEmpty(request.Empleado))
                {
                    query = query.Where(a => a.IdEmpleadoNavigation.Nombre.Contains(request.Empleado));
                }
                if (request.fechaSoli.HasValue)
                {
                    query = query.Where(a => a.FechaSolicitud == request.fechaSoli.Value);
                }
                if (request.Estado != 0)
                {
                    //0 = Todos
                    //1 = Autorizado
                    //2 = Pendiente
                    //3 = Denegados
                    query = query.Where(q => q.Autorizado == request.Estado);
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
                ViewBag.Depto = depto;
                ViewBag.Estado = request.Estado;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "SolicitudesAusencias.Index", $"Se consultaron todas las solicitudes del departamendo {depto}", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "SolicitudesAusencias.Index", "Error al consultar las solicitudes pendientes.", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar solicitudes";
                return View();
            }
        }

        [HttpGet]
        [Authorize(Policy = "SolicitudesAusencias.Ver")]
        public async Task<ActionResult> Details (int id)
        {
            try
            {
                var solicitud = await context.Ausencias.Where(a => a.Id == id)
                                                         .AsNoTracking()
                                                         .Select(a => new GetSolicitudDTO
                                                         {
                                                             Id = a.Id,
                                                             idEmpeado = a.IdEmpleado,
                                                             Tipo = a.Deducible,
                                                             Detalle = a.Detalle,
                                                             FechaSoli = DateOnly.FromDateTime(a.FechaSolicitud),
                                                             FechaInicio = DateOnly.FromDateTime(a.FechaInicio),
                                                             FechaFin = DateOnly.FromDateTime(a.FechaFin),
                                                             FechaAut = DateOnly.FromDateTime(a.FechaAutorizado.GetValueOrDefault()),
                                                             Jefe = a.idJefeNavigation.Nombre.Substring(0, a.idJefeNavigation.Nombre.IndexOf(" ") != -1 ? a.idJefeNavigation.Nombre.IndexOf(" ") : a.idJefeNavigation.Nombre.Length) + " " + a.idJefeNavigation.Apellidos.Substring(0, a.idJefeNavigation.Apellidos.IndexOf(" ") != -1 ? a.idJefeNavigation.Apellidos.IndexOf(" ") : a.idJefeNavigation.Apellidos.Length),
                                                             Dias = a.TotalDias,
                                                             Estado = a.Autorizado,
                                                             Empleado = a.IdEmpleadoNavigation.Nombre.Substring(0, a.IdEmpleadoNavigation.Nombre.IndexOf(" ") != -1 ? a.IdEmpleadoNavigation.Nombre.IndexOf(" ") : a.IdEmpleadoNavigation.Nombre.Length) + " " + a.IdEmpleadoNavigation.Apellidos.Substring(0, a.IdEmpleadoNavigation.Apellidos.IndexOf(" ") != -1 ? a.IdEmpleadoNavigation.Apellidos.IndexOf(" ") : a.IdEmpleadoNavigation.Apellidos.Length)
                                                         })
                                                         .FirstOrDefaultAsync();
                if(solicitud == null)
                {
                    TempData["Error"] = "Error al obtener detalle de la solicitud";
                    return RedirectToAction("Index", "SolicitudesAusencias");
                }
                //si el tipo es deducible, hacer el calculo
                if(solicitud.Tipo == 1)
                {
                    decimal? sueldoEmpleado = await context.Empleados
                                                    .Where(e => e.Id == solicitud.idEmpeado)
                                                    .AsNoTracking()
                                                    .Select(e => e.Sueldo)
                                                    .FirstOrDefaultAsync();
                    if (sueldoEmpleado.HasValue)
                    {
                        // Llamar al servicio de nómina para calcular el descuento
                        solicitud.Deducible = nominaServices.DescuentoAusencia(sueldoEmpleado.Value, solicitud.Dias);
                    }
                }
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "SolicitudesAusencias.Details", $"Se consultaron detalles de la ausencia: {id} ", session.nombre);

                return Json(solicitud);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "SolicitudesAusencias.Details", $"Error al consultar detalles de la ausencia: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de la Ausencia";
                return RedirectToAction("Index", "SolicitudesAusencias");
            }
        }

        [HttpGet]
        public async Task<ActionResult> HistoryAusencia(HistorialSolicitudesDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
                var Depto = await context.Empleados.Where(e => e.Id == session.idEmpleado).AsNoTracking().Select(e=> new
                {
                    Id = e.IdDepartamento,
                    Nombre = e.IdDepartamentoNavigation.Descripcion
                }).FirstOrDefaultAsync();
                var querys = await context.Set<HistorialSolicitudesModel>()
                                            .FromSqlRaw("EXEC sp_GetHistorialAusencias @IdDepartamento",
                                                        new SqlParameter("@IdDepartamento", Depto.Id))
                                            .ToListAsync();

                if (!string.IsNullOrEmpty(request.Empleado))
                {
                    querys = querys.Where(e=> e.NombreEmpleado.Contains(request.Empleado)).ToList();
                }
                if (request.Fecha.HasValue)
                {
                    querys = querys.Where(a => DateOnly.FromDateTime(a.Fecha) == request.Fecha.Value).ToList();
                }
                if (!string.IsNullOrEmpty(request.Estado))
                {
                    querys = querys.Where(q => q.Descripcion.Contains(request.Estado)).ToList();
                }
                var totalItems = querys.Count();
                var historial = querys
                                .Skip((request.page - 1) * request.pageSize)
                                .Take(request.pageSize)
                                .Select(d => new HistorialSolicitudesResponse
                                {
                                    NombreJefe = d.NombreJefe,
                                    Fecha = DateOnly.FromDateTime(d.Fecha),
                                    Descripcion = d.Descripcion,
                                    NombreEmpleado = d.NombreEmpleado,
                                    IdAusencia = d.IdAusencia,
                                });
                var paginatedResult = new PaginatedResult<HistorialSolicitudesResponse>
                {
                    Items = historial,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                ViewBag.Depto = Depto.Nombre;
                ViewBag.Empleado = request.Empleado;
                ViewBag.Fecha = request.Fecha;
                ViewBag.Estado = request.Estado;

                await logger.LogTransaction(session.idEmpleado, session.company, "SolicitudesAusencias.HistoryAusencia", $"Se consultó Historial de solicitudes de ausencia del departamento: {Depto.Id} ", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "SolicitudesAusencias.Details", $"Error al consulta querys de solicitudes ausencias", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo obtener el Hitorial del Departamento, vuelva a intentar";
                return RedirectToAction("Index", "SolicitudesAusencias");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SolicitudesAusencias.Autorizar")]
        public async Task<ActionResult> Update(AuthorizeDTO request)
        {
            var session = logger.GetSessionData();
            try
            {

                var ausencia = await context.Ausencias.SingleAsync(a => a.Id == request.Id);
                if (ausencia == null)
                {
                    TempData["Error"] = "La ausencia seleccionada no existe";
                    return RedirectToAction("Index", "SolicitudesAusencias");
                }
                ausencia.Autorizado = request.Estado;
                ausencia.Deducible = request.Tipo;
                ausencia.FechaAutorizado = DateTime.Now;
                ausencia.idJefe = session.idEmpleado;
                context.Ausencias.Update(ausencia);
                await context.SaveChangesAsync();

                string accion = request.Estado == 1 ? "Autorizada" : request.Estado == 3 ? "Denegada" : "Error";

                //registrar bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "SolicitudesAusencias.Update", $"Fue {accion} la ausencia del empleado id: {ausencia.IdEmpleado}, Ausencia con Id: {request.Id}", session.nombre);

                TempData["Message"] = $"Ausencia {accion} con éxito";
                return RedirectToAction("Index", "SolicitudesAusencias");
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "SolicitudesAusencias.Update", "Error al Autorizar o denegar Solicitud de ausencia", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al modificar Solicitud";
                return RedirectToAction("Index", "SolicitudesAusencias");
            }
        }
    }
}

