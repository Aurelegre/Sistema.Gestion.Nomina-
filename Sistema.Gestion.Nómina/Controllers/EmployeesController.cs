using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using NuGet.Protocol;
using Sistema.Gestion.Nómina.DTOs.Departamentos;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Familia;
using Sistema.Gestion.Nómina.DTOs.Puestos;
using Sistema.Gestion.Nómina.DTOs.Roles;
using Sistema.Gestion.Nómina.DTOs.Usuarios;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{   [Authorize]
    [Controller]

    public class EmployeesController(SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper, IWebHostEnvironment hostingEnvironment) : Controller
    {
        [HttpGet]
        [Authorize(Policy = "Empleados.Listar")]
        public async Task<ActionResult<IEnumerable<GETEmpleadosResponse>>> Index(GetEmployeesDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Empleados
                    .Where(e => e.IdEmpresa == session.company && !e.Dpi.Equals("0") && !e.Apellidos.Equals("Administrador"));

                // Aplicar filtros
                if (!string.IsNullOrEmpty(request.DPI))
                {
                    query = query.Where(e => e.Dpi == request.DPI);
                }
                if (!string.IsNullOrEmpty(request.Nombre))
                {
                    query = query.Where(e => e.Nombre.Contains(request.Nombre));
                }
                if (!string.IsNullOrEmpty(request.Departamento))
                {
                    query = query.Where(e => e.IdDepartamentoNavigation.Descripcion == request.Departamento);
                }
                if (!string.IsNullOrEmpty(request.Puesto))
                {
                    query = query.Where(e => e.IdPuestoNavigation.Descripcion == request.Puesto);
                }
                if (request.estado != 2)
                {
                    query = query.Where(e => e.Activo == request.estado);
                }

                var totalItems = await query.CountAsync();
                var empleados = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GETEmpleadosResponse
                    {
                        Id = u.Id,
                        Nombre = u.Nombre.Substring(0, u.Nombre.IndexOf(" ") != -1 ? u.Nombre.IndexOf(" ") : u.Nombre.Length) + " " + u.Apellidos.Substring(0, u.Apellidos.IndexOf(" ") != -1 ? u.Apellidos.IndexOf(" ") : u.Apellidos.Length),
                        Puesto = u.IdPuestoNavigation.Descripcion,
                        Departamento = u.IdDepartamentoNavigation.Descripcion,
                        DPI = u.Dpi,
                        estado = u.IdUsuarioNavigation.activo,
                        idUser = u.IdUsuario,
                        despedido = u.FechaDespido.HasValue ? true : false,
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<GETEmpleadosResponse>
                {
                    Items = empleados,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                // Pasar filtros a la vista
                ViewBag.DPI = request.DPI;
                ViewBag.Nombre = request.Nombre;
                ViewBag.Departamento = request.Departamento;
                ViewBag.Puesto = request.Puesto;
                ViewBag.Estado = request.estado;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Index", $"Se consultaron todos los empleados activos de la empresa {session.company} y se envió a la vista", session.nombre);

                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Index", "Error al realizar el Get de todos los empleados activos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Empleados";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Empleados.Ver")]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var empleado = await context.Empleados
                .Where(u => u.Id == id)
                .AsNoTracking()
                .Select(u => new GetEmpleadoDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellidos = u.Apellidos,
                    Puesto = u.IdPuestoNavigation.Descripcion,
                    Departamento = u.IdDepartamentoNavigation.Descripcion,
                    DPI = u.Dpi,
                    Sueldo = u.Sueldo,
                    FechaContratado = DateOnly.FromDateTime(u.FechaContratado),
                    Usuario = u.IdUsuarioNavigation.Usuario1
                }).FirstOrDefaultAsync();

                empleado.Family = await ObtenerFamily(id);

                if (empleado == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Empleado";
                    return RedirectToAction("Index", "Employees");
                }

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Details", $"Se consultaron detalles del empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                return Json(empleado);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Details", $"Error al consultar detalles del empleado con idPermiso: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }
        [HttpGet]
        [Authorize(Policy = "Empleados.Actualizar")]
        public async Task<ActionResult> Update(int id)
        {
            try
            {
                var empleado = await context.Empleados
               .Where(u => u.Id == id)
               .AsNoTracking()
               .Select(u => new UpdateEmpleadoViewModel
               {
                   Id = u.Id,
                   Nombre = u.Nombre,
                   Apellidos = u.Apellidos,
                   Sueldo = u.Sueldo,
                   idDepto = u.IdDepartamento,
                   Usuario = u.IdUsuarioNavigation.Usuario1,
                   idRol = u.IdUsuarioNavigation.IdRol,
                   idPuesto = u.IdPuesto
               }).FirstOrDefaultAsync();
                if (empleado == null)
                {
                    return NotFound();
                }
                empleado.Puestos = await ObtenerPuestos(empleado.idDepto);
                empleado.Departamento = await ObtenerDepartamentos();
                empleado.Roles = await ObtenerRoles();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Update", $"Se obtubieron datos para actualizar empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                return Json(empleado);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Update", $"Error al obtener datos para actualizar empleado con idPermiso: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al obtener datos para actualizar Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }
        [HttpGet]
        [Authorize(Policy = "Empleados.Crear")]
        public async Task<ActionResult> Create(int id)
        {
            try
            {
                CreateEmpleadoViewModel Datos = new CreateEmpleadoViewModel
                {
                    Departamentos = await ObtenerDepartamentos(),
                    Puestos = await ObtenerPuestos(id),
                    Roles = await ObtenerRoles()
                };
                if (Datos == null)
                {
                    TempData["Error"] = "Error al obtener datos para crear Empleado";
                    return RedirectToAction("Index", "Employees");
                }

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Create", $"Se obtubieron datos para crear empleado", session.nombre);

                return Json(Datos);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Create", $"Error al obtener datos para crear empleado", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al obtener datos para crear Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }
        [HttpGet]
        [Authorize(Policy = "Empleados.Ver")]
        public async Task<ActionResult<List<object>>> GetPuestos(int id)
        {
            try
            {
                var puestos = new
                {
                    puestos = await ObtenerPuestos(id)
                };

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.GetPuestos", $"Se obtubieron todos los puestos del departamento con id: {id}", session.nombre);

                return Json(puestos);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.GetPuestos", $"Error al obtener los puestos del departamento: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al obtener puestos por departamento";
                return RedirectToAction("Index", "Employees");
            }

        }
        [HttpGet]
        [Authorize(Policy = "Empleados.Ver")]
        public async Task<ActionResult> HistorySueldo(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var historial = await context.HistorialSueldos.Where(e => e.IdEmpleado == id)
                                                               .AsNoTracking()
                                                              .Select(e => new GetHistorySueldoModal
                                                              {
                                                                  Nuevo = e.NuevoSalario,
                                                                  Anterior = e.AnteriorSalario,
                                                                  Fecha = DateOnly.FromDateTime(e.Fecha.Value)
                                                              })
                                                              .OrderByDescending(e => e.Fecha).ToListAsync();
                if (historial == null)
                {
                    TempData["Error"] = "Error al obtener Historial de Salarios";
                    return RedirectToAction("Index", "Employees");
                }
                var nombre = await context.Empleados.Where(e => e.Id == id)
                                                    .AsNoTracking().Select(u => new
                                                    {
                                                        Nombre = u.Nombre.Substring(0, u.Nombre.IndexOf(" ") != -1 ? u.Nombre.IndexOf(" ") : u.Nombre.Length) + " " + u.Apellidos.Substring(0, u.Apellidos.IndexOf(" ") != -1 ? u.Apellidos.IndexOf(" ") : u.Apellidos.Length),
                                                    }).FirstOrDefaultAsync();
                GetHistorySueldoResponse history = new GetHistorySueldoResponse
                {
                    Nombre = nombre.Nombre,
                    History = historial
                };
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.HistorySueldo", $"Se consultó Historial de Sueldos de empleado con id: {id}", session.nombre);

                return Json(history);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Employees.HistorySueldo", $"Error al consultar Historial de Sueldos de empleado con id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Historial de Salarios";
                return RedirectToAction("Index", "Employees");
            }
        }
        [HttpGet]
        [Authorize(Policy = "Empleados.Liquidar")]
        public async Task<ActionResult> Liquidar(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var empleado = await context.Empleados.Where(e => e.Id == id).AsNoTracking().Select(u => new
                {
                    Nombre = u.Nombre.Substring(0, u.Nombre.IndexOf(" ") != -1 ? u.Nombre.IndexOf(" ") : u.Nombre.Length) + " " + u.Apellidos.Substring(0, u.Apellidos.IndexOf(" ") != -1 ? u.Apellidos.IndexOf(" ") : u.Apellidos.Length),
                    u.FechaDespido,
                    u.FechaContratado,
                    u.Sueldo,
                    u.Id
                }).FirstOrDefaultAsync();
                if (empleado == null)
                {
                    TempData["Error"] = "Empleado no encontrado";
                    return RedirectToAction("Index", "Employees");
                }
                string diferenciaFormateada = string.Empty;
                if (empleado.FechaDespido.HasValue)
                {
                    // Calculamos la diferencia de tiempo entre las dos fechas
                    TimeSpan diferencia = empleado.FechaDespido.Value.Date - empleado.FechaContratado.Date;

                    // Convertimos la diferencia en días, meses y años
                    DateTime fechaInicio = empleado.FechaContratado.Date;
                    DateTime fechaFinal = empleado.FechaDespido.Value.Date;

                    int añosTrabajados = fechaFinal.Year - fechaInicio.Year;
                    int mesesTrabajados = fechaFinal.Month - fechaInicio.Month;
                    int diasTrabajados = fechaFinal.Day - fechaInicio.Day;

                    // Si los días trabajados son negativos, ajustamos restando un mes
                    if (diasTrabajados < 0)
                    {
                        mesesTrabajados--;
                        diasTrabajados += DateTime.DaysInMonth(fechaFinal.Year, fechaFinal.Month == 1 ? 12 : fechaFinal.Month - 1);
                    }

                    // Si los meses trabajados son negativos, ajustamos restando un año
                    if (mesesTrabajados < 0)
                    {
                        añosTrabajados--;
                        mesesTrabajados += 12;
                    }

                    // Formateamos el resultado en dd/MM/yyyy (pero esto sería más para mostrar la fecha de contratación y despido)
                    diferenciaFormateada = $"{añosTrabajados} años, {mesesTrabajados} meses y {diasTrabajados} días.";
                }
                else
                {
                    TempData["Error"] = "El Empleado no ha sido Despedido";
                    return RedirectToAction("Index", "Employees");
                }
                var liquidacion = CalcularLiquidacion(empleado.FechaContratado, empleado.Sueldo);

                GetLiquidacionResponse getLiquidacion = new GetLiquidacionResponse
                {
                    Id = empleado.Id,
                    Nombre = empleado.Nombre,
                    Contratado = DateOnly.FromDateTime(empleado.FechaContratado),
                    Despedido = DateOnly.FromDateTime(empleado.FechaDespido.Value),
                    Diferencia = diferenciaFormateada,
                    Liquidacion = liquidacion,
                    Sueldo = empleado.Sueldo
                };
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Liquidar", $"Se consultó liquidación de empleado con id: {id}", session.nombre);

                return Json(getLiquidacion);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Employees.Liquidar", $"Error al calcular liquidación de empleado con id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al calcular Liquidación";
                return RedirectToAction("Index", "Employees");
            }
        }

        //actualizar empleado
        [HttpPost]
        [Authorize(Policy = "Empleados.Actualizar")]
        public async Task<ActionResult> Update(UpdateEmpleadoDTO request, int id)
        {
            try
            {
                var empleado = await context.Empleados.Where(u => u.Id == id).AsNoTracking().FirstOrDefaultAsync();
                if (empleado == null)
                {
                    TempData["Error"] = "Empleado no encontrado";
                    return RedirectToAction("Index", "Employees");
                }
                empleado.Nombre = request.Nombre;
                empleado.Apellidos = request.Apellidos;
                string nombreExpediente = string.Concat(empleado.Dpi.ToString(), ".pdf");
                empleado.PathExpediente = nombreExpediente;
                if (empleado.Sueldo != request.Sueldo)
                {
                    //si se cambia el sueldo guardar en el historial
                    await CreateHistorialSueldo(empleado.Id, empleado.Sueldo, request.Sueldo);
                    empleado.Sueldo = request.Sueldo;
                }

                var puesto = await context.Puestos.SingleAsync(p => p.Id == request.IdPuesto);
                if (puesto.Descripcion.Equals("Jefe") && empleado.IdPuesto != puesto.Id)
                {
                    var asignedEmployee = await context.Empleados.SingleAsync(e => e.IdPuesto == puesto.Id);//buscar si un empleado tiene ese puesto JEFE
                    if (asignedEmployee != null)
                    {
                        //dejar sin puesto asignado
                        asignedEmployee.IdPuesto = null;
                        context.Empleados.Update(asignedEmployee);
                    }
                    //asignar como nuevo jefe en del depto
                    var depto = await context.Departamentos.SingleAsync(d => d.Id == request.IdDepartamento);//buscar el departamento asignado
                    depto.IdJefe = empleado.Id;//asigno el jefe al dpto
                    //actualizar Depto
                    context.Departamentos.Update(depto);
                    empleado.IdPuesto = request.IdPuesto;//asigno el puesto jefe al nuevo usuario
                }
                else
                {
                    empleado.IdPuesto = request.IdPuesto;
                }
                empleado.IdDepartamento = request.IdDepartamento;
                context.Update(empleado);
                //actualizar ROl
                var user = await context.Usuarios.Where(e => e.Id == empleado.IdUsuario).AsNoTracking().FirstOrDefaultAsync();
                user.IdRol = request?.IdRol;
                context.Usuarios.Update(user);
                //Guardar Cambios
                await context.SaveChangesAsync();

                //verficar si se cambió el expediente
                if(request.expedientePDF != null)
                {
                    await EditarExpediente(request.expedientePDF, nombreExpediente);
                }

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Update", $"Se actualizó empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                TempData["Message"] = "Empleado Actualizado con Exito";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Update", $"Error al acualizar empleado con id {request.Id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo actualizar Empleado";
                return RedirectToAction("Index", "Employees");
            }

        }
        //eliminar (Despedir) Usuario
        [HttpPost]
        [Authorize(Policy = "Empleados.Despedir")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var empleado = await context.Empleados.Where(p => p.Id == id).AsNoTracking().FirstOrDefaultAsync();
                if (empleado == null)
                {
                    TempData["Error"] = "El Empleado no existe";
                    return RedirectToAction("Index", "Employees");
                }
                //verificar que no sea jefe de departamento
                var depto = await context.Departamentos.Where(d => d.IdJefe == empleado.Id).AsNoTracking().FirstOrDefaultAsync();
                if (depto != null)
                {
                    TempData["Error"] = "No se pueden Despedir Empleados jefes de Departamento";
                    return RedirectToAction("Index", "Employees");
                }

                //desactivar empleado
                empleado.Activo = 0;
                empleado.FechaDespido = DateTime.Now.Date;
                context.Update(empleado);

                var usuario = await context.Usuarios.Where(e => e.Id == empleado.IdUsuario).AsNoTracking().FirstOrDefaultAsync();
                if (usuario != null)
                {
                    //desactivar usuario
                    usuario.activo = 0;
                    context.Update(usuario);
                }

                await context.SaveChangesAsync();
                //guardar bitácora
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Delete", $"Se despidió empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                TempData["Message"] = "Empleado Despedido con Exito";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Delete", $"Error al despedir empleado con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo Despedir Empleado";
                return RedirectToAction("Index", "Employees");
            }
        }
        [HttpPost]
        //crear empleado
        [Authorize(Policy = "Empleados.Crear")]
        public async Task<ActionResult> Create(CreateEmployeeDTO request)
        {
            var session = logger.GetSessionData();
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    //estandarización
                    request.Usuario = request.Usuario.ToUpper();
                    // Verificar existencia de DPI
                    bool dpiExists = await context.Empleados.AnyAsync(e => e.Dpi == request.Dpi);
                    if (dpiExists)
                    {
                        TempData["Error"] = "El DPI ya está registrado en otro empleado";
                        return RedirectToAction("Index", "Employees");
                    }

                    // Verificar existencia de usuario (correo)
                    bool userExists = await context.Usuarios.AnyAsync(u => u.Usuario1 == request.Usuario);
                    if (userExists)
                    {
                        TempData["Error"] = "El correo ya está asignado a otro empleado";
                        return RedirectToAction("Index", "Employees");
                    }
                    if (request.expedientePDF == null || request.expedientePDF.Length == 0)
                    {
                        TempData["Error"] = "Debe seleccionar un archivo PDF válido.";
                        return RedirectToAction("Index", "Employees");  // Redirige a la vista correspondiente
                    }
                    // verificar que si el puesto es Jefe, verificar que no esté asignado a otro empleado
                    var puesto = await context.Puestos.SingleAsync(p => p.Id == request.IdPuesto);
                    if (puesto.Descripcion.Equals("Jefe"))
                    {
                        var asignedEmployee = await context.Empleados.CountAsync(e => e.IdPuesto == puesto.Id);//buscar si un empleado tiene ese puesto
                        if (asignedEmployee > 0)
                        {
                            TempData["Error"] = "Ya existe un Jefe en el Departamento asignado";
                            return RedirectToAction("Index", "Employees");
                        }
                    }
                    // Crear usuario
                    var user = new Usuario
                    {
                        Usuario1 = request.Usuario,
                        IdEmpresa = session.company,
                        activo = 0,
                        IdRol = request.IdRol,
                        Attempts = 0
                    };
                    context.Usuarios.Add(user);
                    await context.SaveChangesAsync();

                    string nombreExpediente = string.Concat(request.Dpi.ToString(), ".pdf");
                    string nombreImagen = string.Empty;//string.Concat(request.Dpi.ToString(), request.Nombre, ".png");
                    // Obtener ID del usuario recién creado
                    int idUser = user.Id; // Usar el ID directamente desde el objeto recién agregado

                    // Crear empleado
                    TimeOnly timeOnly = new TimeOnly(00, 00, 00);
                    var empleado = new Empleado
                    {
                        Activo = request.Activo,
                        Nombre = request.Nombre,
                        Apellidos = request.Apellidos,
                        IdEmpresa = session.company,
                        IdPuesto = request.IdPuesto,
                        IdDepartamento = request.IdDepartamento,
                        Sueldo = request.Sueldo,
                        FechaContratado = request.FechaContratado.ToDateTime(timeOnly),
                        IdUsuario = idUser,
                        Dpi = request.Dpi,
                        PathExpediente = nombreExpediente,
                        PathImagen = nombreImagen,
                    };
                    context.Empleados.Add(empleado);
                    await context.SaveChangesAsync();

                    // Obtener el ID del empleado recién creado
                    int idEmpleado = empleado.Id;
                    //crear historial de Sueldo
                    await CreateHistorialSueldo(idEmpleado, 0.00m, empleado.Sueldo);
                    //si el puesto asignado es Jefe, actualizar Departamento
                    if (puesto.Descripcion.Equals("Jefe"))
                    {
                        var depto = await context.Departamentos.AsNoTracking().SingleAsync(d => d.Id == request.IdDepartamento);
                        //si el departamento ya tiene un jefe, dejar sin puesto
                        if (depto.IdJefe != null)
                        {
                            var anteriorJefe = await context.Empleados.SingleAsync(e => e.Id == depto.IdJefe && e.IdEmpresa == session.company);
                            if (anteriorJefe != null)
                            {
                                //dejar sin puesto asignado
                                anteriorJefe.IdPuesto = null;
                                context.Empleados.Update(anteriorJefe);
                            }
                        }
                        depto.IdJefe = idEmpleado;
                        context.Departamentos.Update(depto);
                    }

                    // Crear familiares
                    foreach (var familia in request.FamilyEmployeeDTOs)
                    {
                        if (familia != null)
                        {
                            var familiar = new Familia
                            {
                                Nombre = familia.Nombre,
                                Edad = familia.Edad,
                                Parentesco = familia.Parentesco,
                                IdEmpleado = idEmpleado
                            };
                            context.Familias.Add(familiar);
                        }
                    }
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await SubirExpediente(request.expedientePDF, nombreExpediente);
                    // Guardar bitácora
                    await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Create", $"Se creó empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                    TempData["Message"] = "Empleado creado con éxito";
                    return RedirectToAction("Index", "Employees");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await logger.LogError(session.idEmpleado, session.company, "Employees.Create", "Error al crear empleado", ex.Message, ex.StackTrace);
                    TempData["Error"] = "No se pudo crear el empleado";
                    return RedirectToAction("Index", "Employees");
                }
            }
        }
        [HttpPost]
        //desbloquear usuario
        [Authorize(Policy = "Empleados.Actualizar")]
        public async Task<ActionResult> UnlockUser(int id)
        {
            try
            {
                var user = await context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction("Index", "Employees");
                }
                if (string.IsNullOrEmpty(user.Contraseña))
                {
                    TempData["Error"] = "Usuario sin primer inicio de sesión";
                    return RedirectToAction("Index", "Employees");
                }
                user.activo = 1;
                context.Usuarios.Update(user);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.UnlockUser", $"Se desbloqueó usuario con id: {id}", session.nombre);
                TempData["Message"] = "Usuario desbloqueado con éxito";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.UnlockUser", $"Error al desbloquear usuario con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo desbloquear usuario";
                return RedirectToAction("Index", "Employees");
            }
        }
        [HttpPost]
        //recontratar Empleado
        [Authorize(Policy = "Empleados.Recontratar")]
        public async Task<ActionResult> Recontratar(int id)
        {
            try
            {
                var empleado = await context.Empleados.Where(p => p.Id == id).AsNoTracking().FirstOrDefaultAsync();
                if (empleado == null)
                {
                    TempData["Error"] = "El Empleado no existe";
                    return RedirectToAction("Index", "Employees");
                }

                //recontratar empleado
                empleado.Activo = 1;
                empleado.FechaDespido = null;
                empleado.FechaContratado = DateTime.Now.Date;
                context.Update(empleado);

                var usuario = await context.Usuarios.Where(e => e.Id == empleado.IdUsuario).AsNoTracking().FirstOrDefaultAsync();
                if (usuario != null)
                {
                    //activar usuario
                    usuario.activo = 1;
                    context.Update(usuario);
                }

                await context.SaveChangesAsync();
                //guardar bitácora
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.Recontratar", $"Se Recontrató empleado con id: {empleado.Id}, Nombre: {empleado.Nombre}", session.nombre);

                TempData["Message"] = "Empleado Recontratado con Exito";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Recontratar", $"Error al Recontratar empleado con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo Recontratar Empleado";
                return RedirectToAction("Index", "Employees");
            }
        }
        [HttpGet]
        [Authorize(Policy = "Empleados.Ver")]
        public async Task<IActionResult> DescargarExpediente(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                // Nombre del archivo a retornar
                string nombreArchivo = await context.Empleados
                                                            .Where(e => e.Id == id)
                                                            .AsNoTracking()
                                                            .Select(e => e.PathExpediente)
                                                            .FirstOrDefaultAsync();
                if(nombreArchivo == null)
                {
                    TempData["Error"] = "El empleado no posee Expediente.";
                    return RedirectToAction("Index", "Employees");
                }
                // Ruta de la carpeta 'expedientes' dentro de wwwroot
                string rutaCarpetaExpedientes = Path.Combine(hostingEnvironment.WebRootPath, "expedientes");

                

                // Ruta completa del archivo
                string rutaArchivo = Path.Combine(rutaCarpetaExpedientes, nombreArchivo);

                // Verificar si el archivo existe
                if (!System.IO.File.Exists(rutaArchivo))
                {
                    TempData["Error"] = "El expediente no fue encontrado.";
                    return RedirectToAction("Index", "Employees");  // Redirige a la vista correspondiente si el archivo no existe
                }

                // Leer el archivo como bytes
                var archivoBytes = await System.IO.File.ReadAllBytesAsync(rutaArchivo);
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.DescargarExpediente", $"Se descargó expediente de empleado con id {id}", session.nombre);
                // Devolver el archivo al cliente
                return File(archivoBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Employees.DescargarExpediente", $"Error al descargar expediente de empleado:  {id}", ex.Message, ex.StackTrace);
                // Registrar el error en el log y devolver un mensaje de error
                TempData["Error"] = $"Error al descargar el expediente: {ex.Message}";
                return RedirectToAction("Index", "Employees");
            }
        }
        //obtención de datos
        private async Task<List<GetPuestoDTO>> ObtenerPuestos(int? idDepartamento)
        {
            try
            {
                var puestos = await context.Puestos.Where(p => p.IdDepartamento == idDepartamento).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetPuestoDTO>>(puestos);

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerPuestos", $"Se consultaron puestos del departamento {idDepartamento}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerPuestos", $"Error al consultar puestos del departamento {idDepartamento}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<List<GetDepartamentoDTO>> ObtenerDepartamentos()
        {
            try
            {
                var session = logger.GetSessionData();
                var departamentos = await context.Departamentos.Where(d => d.IdEmpresa == session.company).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetDepartamentoDTO>>(departamentos);


                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerDepartamentos", $"Se consultaron departamentos de la empresa {session.company}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerDepartamentos", $"Error al consultar departamentos", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<List<GetUsuariosDTO>> ObtenerUsuariosSinAsignar()
        {
            try
            {
                var session = logger.GetSessionData();
                var usuariosNoAsignados = await context.Usuarios
                                                       .Where(u => u.activo == 1 && u.IdEmpresa == session.company) // Filtrar usuarios activos de la empresa actual
                                                       .GroupJoin(
                                                           context.Empleados.Where(e => e.IdEmpresa == session.company),
                                                           u => u.Id,
                                                           e => e.IdUsuario,
                                                           (u, e) => new { Usuario = u, Empleados = e }
                                                       )
                                                       .SelectMany(
                                                           ue => ue.Empleados.DefaultIfEmpty(),
                                                           (ue, empleado) => new { ue.Usuario, empleado }
                                                       )
                                                       .Where(joinResult => joinResult.empleado == null) // Usuarios sin empleado asignado
                                                       .Select(joinResult => joinResult.Usuario)
                                                       .AsNoTracking()
                                                       .ToListAsync();


                //var usuariosNoAsignados = await context.Usuarios
                //                                        .Where(u => !context.Empleados.Any(e => e.IdUsuario == u.idPermiso && e.IdEmpresa == session.company)
                //                                                    && u.IdEmpresa == session.company)
                //                                        .AsNoTracking()
                //                                        .ToListAsync();

                var listado = _mapper.Map<List<GetUsuariosDTO>>(usuariosNoAsignados);

                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerUsuariosSinAsignar", $"Se consultaron Usuarios sin asignar de la empresa {session.company}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerUsuariosSinAsignar", $"Error al consultar Usuarios sin asignar de la emoesa {session.company}", ex.Message, ex.StackTrace);
                return null;
            }


        }
        private async Task<List<GetFamilyEmployeeDTO>> ObtenerFamily(int? idEmpleado)
        {
            try
            {
                var Familia = await context.Familias.Where(p => p.IdEmpleado == idEmpleado).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetFamilyEmployeeDTO>>(Familia);

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerFamily", $"Se consultaron familiares del empleado {idEmpleado}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerPuestos", $"Error al consultar familiares del empleado: {idEmpleado}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<List<GetRolResponse>> ObtenerRoles()
        {
            try
            {
                var session = logger.GetSessionData();
                var roles = await context.Roles.Where(p => p.IdEmpresa == session.company && p.activo == 1).AsNoTracking().ToListAsync();
                var listado = _mapper.Map<List<GetRolResponse>>(roles);

                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.ObtenerRoles", $"Se consultaron Roles de la empresa {session.company}", session.nombre);

                return listado;
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.ObtenerRoles", $"Error al consultar roles de la empresa: {session.company}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        private async Task<bool> CreateHistorialSueldo(int? idEmpleado, decimal? anteriorSalario, decimal? nuevoSalario)
        {
            var session = logger.GetSessionData();
            try
            {
                HistorialSueldo newsueldo = new HistorialSueldo
                {
                    AnteriorSalario = anteriorSalario,
                    NuevoSalario = nuevoSalario,
                    Fecha = DateTime.Now.Date,
                    IdEmpleado = idEmpleado,
                };
                await context.HistorialSueldos.AddAsync(newsueldo);
                await context.SaveChangesAsync();

                await logger.LogTransaction(session.idEmpleado, session.company, "Employees.CreateHistorialSueldo", $"Se agregó Historial de sueldo del empleado {idEmpleado}", session.nombre);
                return true;
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Employees.CreateHistorialSueldo", $"Error al agregar Historial de sueldo del empleado {idEmpleado}", ex.Message, ex.StackTrace);
                return false;
            }
        }
        private double? CalcularLiquidacion(DateTime fechaContratacion, decimal? salario)
        {
            try
            {
                // Obtener la fecha actual
                DateTime fechaActual = DateTime.Now;

                // Calcular la diferencia exacta de años, meses y días trabajados
                int añosTrabajados = fechaActual.Year - fechaContratacion.Year;
                int mesesTrabajados = fechaActual.Month - fechaContratacion.Month;
                int diasTrabajados = fechaActual.Day - fechaContratacion.Day;

                // Ajustar si los meses o días son negativos
                if (diasTrabajados < 0)
                {
                    mesesTrabajados--;
                    diasTrabajados += DateTime.DaysInMonth(fechaActual.Year, fechaActual.Month == 1 ? 12 : fechaActual.Month - 1); // Sumar los días del mes anterior
                }

                if (mesesTrabajados < 0)
                {
                    añosTrabajados--;
                    mesesTrabajados += 12;
                }

                // Calcular la liquidación proporcional
                double salarioAnual = (double)salario;
                double liquidacion = (añosTrabajados + (mesesTrabajados / 12.0) + (diasTrabajados / 365.0)) * salarioAnual;

                // Retornar el resultado redondeado a 2 decimales
                return Math.Round(liquidacion, 2);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al calcular la liquidación: {ex.Message}", ex);
            }
        }
        public async Task SubirExpediente(IFormFile expedientePDF , string nombreArchivo)
        {
            try
            {

                // Ruta de la carpeta 'expedientes' dentro de wwwroot
                string rutaCarpetaExpedientes = Path.Combine(hostingEnvironment.WebRootPath, "expedientes");

                // Si la carpeta 'expedientes' no existe, crearla
                if (!Directory.Exists(rutaCarpetaExpedientes))
                {
                    Directory.CreateDirectory(rutaCarpetaExpedientes);
                }

                // Ruta completa del archivo
                string rutaArchivo = Path.Combine(rutaCarpetaExpedientes, nombreArchivo);

                // Guardar el archivo PDF en la ruta especificada
                using (var fileStream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await expedientePDF.CopyToAsync(fileStream);
                }

            }
            catch (Exception ex)
            {
                // Registrar el error en el log y devolver un mensaje de error
                throw new InvalidOperationException($"Error al subir el expediente: {ex.Message}", ex);
            }
        }
        public async Task EditarExpediente(IFormFile expedientePDF, string nombreArchivo)
        {
            try
            {
                // Ruta de la carpeta 'expedientes' dentro de wwwroot
                string rutaCarpetaExpedientes = Path.Combine(hostingEnvironment.WebRootPath, "expedientes");

                // Si la carpeta 'expedientes' no existe, crearla
                if (!Directory.Exists(rutaCarpetaExpedientes))
                {
                    Directory.CreateDirectory(rutaCarpetaExpedientes);
                }

                // Ruta completa del archivo
                string rutaArchivo = Path.Combine(rutaCarpetaExpedientes, nombreArchivo);

                // Si ya existe un archivo con el mismo nombre, eliminarlo
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }
                // Guardar el nuevo archivo PDF en la ruta especificada
                using (var fileStream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await expedientePDF.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Registrar el error en el log y devolver un mensaje de error
                throw new InvalidOperationException($"Error al subir el expediente: {ex.Message}", ex);
            }
        }
    }
}
