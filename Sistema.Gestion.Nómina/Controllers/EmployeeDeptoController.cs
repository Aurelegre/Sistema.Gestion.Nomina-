using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.EmployeeDepto;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Authorize]
    [Controller]
    public class EmployeeDeptoController(SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        [HttpGet]
        [Authorize(Policy = "EmpleadosDepto.Listar")]
        public async Task<ActionResult> Index(GetEmployeesDeptoDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
                //traer el departamento del deptoEmpleado
                var deptoEmpleado = await context.Empleados.Where(e => e.Id == session.idEmpleado).AsNoTracking().Select(e=> e.IdDepartamento).FirstOrDefaultAsync();
                if (deptoEmpleado == null)
                {
                    TempData["Error"] = "Empleado sin departamento Asignado";
                    return RedirectToAction("Index", "Home");
                }
                var depto = await context.Departamentos.AsNoTracking().SingleAsync(e => e.Id == deptoEmpleado);
                //si es Jefe no traer su registro
                //Iniciar a traer los empleados del departamento seleccionado
                var query = context.Empleados.Where(e => e.Activo == 1 && e.IdDepartamento == depto.Id && e.Id != depto.IdJefe);

                //aplicar filtros
                if (!string.IsNullOrEmpty(request.DPI))
                {
                    query = query.Where(e => e.Dpi == request.DPI);
                }
                if (!string.IsNullOrEmpty(request.Nombre))
                {
                    query = query.Where(e => e.Nombre.Contains(request.Nombre));
                }
                if (!string.IsNullOrEmpty(request.Puesto))
                {
                    query = query.Where(e => e.IdPuestoNavigation.Descripcion == request.Puesto);
                }

                var totalItems = await query.CountAsync();
                var empleados = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GetEmployeesDeptoResponse
                    {
                        Id = u.Id,
                        Nombre = u.Nombre.Substring(0, u.Nombre.IndexOf(" ") != -1 ? u.Nombre.IndexOf(" ") : u.Nombre.Length) + " " + u.Apellidos.Substring(0, u.Apellidos.IndexOf(" ") != -1 ? u.Apellidos.IndexOf(" ") : u.Apellidos.Length),
                        Puesto = u.IdPuestoNavigation.Descripcion,
                        DPI = u.Dpi,
                    })
                    .ToListAsync();
                var paginatedResult = new PaginatedResult<GetEmployeesDeptoResponse>
                {
                    Items = empleados,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                // Pasar filtros a la vista
                ViewBag.DPI = request.DPI;
                ViewBag.Nombre = request.Nombre;
                ViewBag.Puesto = request.Puesto;
                ViewBag.Departamento = depto.Descripcion;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "EmployeeDepto.Index", $"Se consultaron todos los empleados del departamento {depto.Id}", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            { 
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.Index", "Error al realizar el Get de todos los empleados de un departamento", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Empleados del Departamento";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Authorize(Policy = "EmpleadosDepto.Ver")]
        public async Task<ActionResult> Details(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var empleado = await context.Empleados
                                           .Where(u => u.Id == id)
                                           .AsNoTracking()
                                           .Select( u => new GetEmployeeDeptoDTO
                                           {
                                               Id = u.Id,
                                               Nombre = u.Nombre,
                                               Apellidos = u.Apellidos,
                                               Puesto = u.IdPuestoNavigation.Descripcion,
                                               Departamento = u.IdDepartamentoNavigation.Descripcion,
                                               DPI = u.Dpi,
                                               Sueldo = u.Sueldo,
                                               FechaContratado = DateOnly.FromDateTime(u.FechaContratado)
                                           }).FirstOrDefaultAsync();
               
                if (empleado == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Empleado";
                    return RedirectToAction("Index", "EmployeeDepto");
                }

                empleado.HorasExtra = await ObtenerTotales(id,1);
                empleado.ComisionHorasExtra = await ObtenerComisionHorasExtras(id, empleado.Sueldo);
                empleado.HorasDiaFestivo = await ObtenerTotales(id, 2);
                empleado.ComisionDiaFestivo = await ObtenerComisionDiasFestivos(id,empleado.Sueldo);
                empleado.ComisionVentas = await ObtenerComisionVentas(id,empleado.Sueldo);
                empleado.ComisionProd = await ObtenerComisionProd(id,empleado.Sueldo);
                empleado.Anticipo = await ObtenerAnticipo(id);

                await logger.LogTransaction(session.idEmpleado, session.company, "EmployeeDepto.Details", $"Se consultaron detalles del empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                return Json(empleado);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.Details", $"Error al consultar detalles del empleado con id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Empleado";
                return RedirectToAction("Index", "EmployeeDepto");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EmpleadosDepto.Actualizar")]
        public async Task<ActionResult> AumentoHoras(CreateHorasExtrasDTO request)
        {
            var session = logger.GetSessionData();
            try
            {
                int horas = request.Tiempo.Hour;
                int minutos = request.Tiempo.Minute;

                // Convertir a horas decimales
                decimal horasDecimales = horas + (minutos / 60.00m);

                Aumento horasExtras = new Aumento
                {
                    Fecha = DateTime.Now,
                    IdEmpleado = request.IdEmpleado,
                    IdTipo = request.Tipo,
                    Total = horasDecimales
                };
                context.Aumento.Add(horasExtras);
                await context.SaveChangesAsync();

                await logger.LogTransaction(session.idEmpleado, session.company, "EmployeeDepto.AumentoHoras", $"Se registraron {horasDecimales} horas extras de tipo {request.Tipo} al empleado {request.IdEmpleado}", session.nombre);

                TempData["Message"] = "Horas Extras registradas con éxito";
                return RedirectToAction("Index", "EmployeeDepto");
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.Details", $"Error al registrar horas extras al empleado con id: {request.IdEmpleado}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al registrar Horas Extras";
                return RedirectToAction("Index", "EmployeeDepto");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EmpleadosDepto.Actualizar")]
        public async Task<ActionResult> Aumentos(CreateAumentosDTO request)
        {
            var session = logger.GetSessionData();
            string handle = string.Empty;
            if (request.Tipo == 3)
            {
                handle = "Anticipo";
            }
            else if (request.Tipo == 5)
            {
                handle = "Comisión por Producción";
            }
            else if (request.Tipo == 4)
            {
                handle = "Comisión por Venta";
            }
            try
            {
                if(request.Tipo == 3)
                {
                    if (DateTime.Now.Day > 15)
                    {
                        TempData["Error"] = $"Solo se Pueden Registrar Anticipos en la Primera quincena del mes";
                        return RedirectToAction("Index", "EmployeeDepto");
                    }
                    var lastAnticipo = await context.Aumento.Where(e=> e.IdTipo == 3 && e.IdEmpleado == request.IdEmpleado)
                                                            .AsNoTracking()
                                                            .FirstOrDefaultAsync();
                    if(lastAnticipo!= null && lastAnticipo.Fecha.Month == DateTime.Now.Month)
                    {
                        TempData["Error"] = $"El Empleado ya posee un anticipo en este Mes";
                        return RedirectToAction("Index", "EmployeeDepto");
                    }
                }
                Aumento horasExtras = new Aumento
                {
                    Fecha = DateTime.Now,
                    IdEmpleado = request.IdEmpleado,
                    IdTipo = request.Tipo,
                    Total = request.Total
                };
                context.Aumento.Add(horasExtras);
                await context.SaveChangesAsync();
                
                await logger.LogTransaction(session.idEmpleado, session.company, "EmployeeDepto.Aumentos", $"Se registró {handle} de tipo {request.Tipo} al empleado {request.IdEmpleado} por: {request.Total}", session.nombre);

                TempData["Message"] = $"Se registró {handle} con éxito";
                return RedirectToAction("Index", "EmployeeDepto");
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.Aumentos", $"Error al registrar {handle} al empleado con id: {request.IdEmpleado}", ex.Message, ex.StackTrace);
                TempData["Error"] = $"Error al registrar {handle}";
                return RedirectToAction("Index", "EmployeeDepto");
            }
        }

        private async Task<decimal?> ObtenerTotales(int? Idempleado, int tipo)
        {
            try
            {
                var Horas = await context.Aumento.Where(e => e.IdEmpleado == Idempleado && e.IdTipo == tipo && e.Fecha.Month == DateTime.Now.Month).AsNoTracking().Select(e=> e.Total).ToListAsync();
                if(Horas.Count == 0)
                {
                    return 0;
                }
                decimal? totalHoras = Horas.Sum();
                return totalHoras;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.ObtenerTotales", $"Error al consultar total tipo {tipo} del empleado. {Idempleado}", ex.Message, ex.StackTrace);
                return null;
            }
        }
        private async Task<decimal?> ObtenerComisionHorasExtras(int? IdEmpleado, decimal? salario)
        {
            try
            {
                var horas = await ObtenerTotales(IdEmpleado,1);
                if (horas == null)
                {
                    return null;
                }
                var totalExtra = nominaServices.CalcularHorasExtras(salario, horas);
                return totalExtra;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.ObtenerComisionHorasExtras", $"Error al consultar horas extras del empleado. {IdEmpleado}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<decimal?> ObtenerComisionDiasFestivos(int? IdEmpleado, decimal? salario)
        {
            try
            {
                var horas = await ObtenerTotales(IdEmpleado, 2);
                if (horas == null)
                {
                    return null;
                }
                var totalExtra = nominaServices.CalcularComisionDiafestivo(salario, horas);
                return totalExtra;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.ObtenerComisionDiasFestivos", $"Error al calcular comision por dias festivos del empleado. {IdEmpleado}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<decimal?> ObtenerComisionVentas(int IdEmpleado, decimal? salario)
        {
            try
            {
                var ventas = await ObtenerTotales(IdEmpleado, 4);
                if (ventas == null)
                {
                    return null;
                }
                if (ventas <= 100000)
                {
                    return null;
                }
                var totalComi = nominaServices.CalcularComisionVenta(IdEmpleado,ventas);
                return totalComi;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.ObtenerComisionVentas", $"Error al calcular comision por ventas del empleado. {IdEmpleado}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<decimal?> ObtenerComisionProd(int IdEmpleado, decimal? salario)
        {
            try
            {
                var producido = await ObtenerTotales(IdEmpleado, 5);
                if (producido == null)
                {
                    return null;
                }
                var totalComi = nominaServices.CalcularComisionProd(IdEmpleado, producido);
                return totalComi;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.ObtenerComisionProd", $"Error al calcular comision por producción del empleado. {IdEmpleado}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<string?> ObtenerAnticipo(int? IdEmpleado)
        {
            try
            {

                var exist = await context.Aumento.AsNoTracking().AnyAsync(e => e.IdEmpleado == IdEmpleado && e.IdTipo == 3 && e.Fecha.Month == DateTime.Now.Month);
                string res = string.Empty;
                if (exist)
                {
                    res = "Entregado";
                }
                else
                {
                    res = "No solicitado";
                }
                return res;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "EmployeeDepto.ObtenerAnticipo", $"Error al consultar anticipo del empleado. {IdEmpleado}", ex.Message, ex.StackTrace);
                return null;
            }

        }
    }
}
