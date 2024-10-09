using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Empresa;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Helpers;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Empresa;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Authorize(Roles = "SuperRol")]
    public class EmpresaController(SistemaGestionNominaContext context, ILogServices logger, IEmpresaServices empresaServices, Hasher hasher) : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Index(GetEmpresasDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Empresas.Where(e=> e.Id != session.company);
                // Aplicar filtros
                if (!string.IsNullOrEmpty(request.Telefono))
                {
                    query = query.Where(e => e.Teléfono == request.Telefono);
                }
                if (!string.IsNullOrEmpty(request.Nombre))
                {
                    query = query.Where(e => e.Nombre.Contains(request.Nombre));
                }
                var totalItems = await query.CountAsync();
                var empresas = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GetEmpresasResponce
                    {
                        Id = u.Id,
                        Nombre = u.Nombre,
                        Direccion = u.Direccion,
                        Telefono = u.Teléfono,
                        Active = u.Active
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<GetEmpresasResponce>
                {
                    Items = empresas,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                // Pasar filtros a la vista
                ViewBag.Nombre = request.Nombre;
                ViewBag.Telefono = request.Telefono;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Empresa.Index", $"Se consultaron todos las Empresas y se envió a la vista", session.nombre);

                return View(paginatedResult);
            }
            catch(Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Empresa.Index", "Error al realizar el Get de todos las Empresas", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Empresas";
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var empresa = await context.Empresas
                    .Where(e => e.Id == id)
                    .AsNoTracking()
                    .Select(e => new GetEmpresaResponse
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        Direccion = e.Direccion,
                        Telefono = e.Teléfono
                    })
                    .FirstOrDefaultAsync();

                if (empresa == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Empresa";
                    return RedirectToAction("Index", "Empresa");
                }

                var admin = await context.Empleados
                    .Where(e => e.IdEmpresa == id && e.Apellidos == "Administrador")
                    .AsNoTracking()
                    .Select(e => new
                    {
                        Nombre = e.Nombre,
                        Usuario = e.IdUsuarioNavigation.Usuario1,
                        IdUsuario = e.IdUsuario
                    })
                    .FirstOrDefaultAsync();

                // Validar si no se encontró un administrador
                if (admin == null)
                {
                    TempData["Error"] = "No se encontró administrador para la empresa.";
                    return RedirectToAction("Index", "Empresa");
                }
                empresa.Administrador = admin.Nombre;
                empresa.IdUsuario = admin.IdUsuario;
                empresa.Usuario = admin.Usuario;

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Empresa.Details", $"Se consultaron detalles de la empresa con id: {empresa.Id}, Nombre: {empresa.Nombre}", session.nombre);

                return Json(empresa);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Empresa.Details", $"Error al consultar detalles de la Empresa con idPermiso: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Empresa";
                return RedirectToAction("Index", "Empresa");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateEmpresaDTO request)
        {
            
                try
                {
                    var exist = await context.Empresas.CountAsync(e => e.Nombre == request.Nombre);
                    if (exist > 0)
                    {
                        TempData["Error"] = "La empresa ya existe";
                        return RedirectToAction("Index", "Empresa");
                    }

                    Empresa empresa = new Empresa
                    {
                        Nombre = request.Nombre,
                        Direccion = request.Direccion,
                        Teléfono = request.Telefono,
                        Active = 1
                    };
                    context.Empresas.Add(empresa);
                    await context.SaveChangesAsync();

                    var idEmpresa = empresa.Id;
                    int createUser = await empresaServices.CreateUser(idEmpresa, request.Usuario, request.Contrasena);
                    if (createUser == 0)
                    {
                        TempData["Error"] = "Error al crear usuario administrador, Vuelva a Intentar.";
                        return RedirectToAction("Index", "Empresa");
                    }
                    int crearEmpleado = await empresaServices.CreateAdmin(idEmpresa, request.Nombre, "0", createUser);
                    if (crearEmpleado == 0)
                    {
                        TempData["Error"] = "Error al crear Empleado Administrador, Vuelva a Intentar.";
                        return RedirectToAction("Index", "Empresa");
                    }

                    var session = logger.GetSessionData();
                    await logger.LogTransaction(session.idEmpleado, session.company, "Empresa.Create", $"Se creó empresa con el Nombre: {empresa.Nombre}, Usuario id: {createUser}, Empleado Id: {crearEmpleado}", session.nombre);

                    TempData["Message"] = "Empresa creada con éxito";
                    return RedirectToAction("Index", "Empresa");
                }
                catch (Exception ex)
                {
                    var session = logger.GetSessionData();
                    await logger.LogError(session.idEmpleado, session.company, "Empresa.Create", "Error al crear Empresa", ex.Message, ex.StackTrace);
                    TempData["Error"] = "Error al crear Empresa, Vuelva a Intentar.";
                    return RedirectToAction("Index", "Empresa");
                }
            
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditEmpresaDTO request)
        {
            try
            {
                var empresa = await context.Empresas.SingleAsync(e => e.Id == request.Id);
                if(empresa == null)
                {
                    TempData["Error"] = "Empresa no encontrada";
                    return RedirectToAction("Index", "Empresa");
                }
                empresa.Nombre = request.Nombre;
                empresa.Direccion = request.Direccion;
                empresa.Teléfono = request.Telefono;
                context.Empresas.Update(empresa);

                var user = await context.Usuarios.AsNoTracking().SingleAsync(u => u.Id == request.IdUsuario);
                user.Usuario1 = request.Usuario;
                if (!string.IsNullOrEmpty(request.Contrasena))
                {
                    user.Contraseña = hasher.HashPassword(request.Contrasena);
                }
                context.Usuarios.Update(user);
                await context.SaveChangesAsync();
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Empresa.Edit", $"Se actualizó empresa con id: {empresa.Id}, Nombre: {empresa.Nombre}", session.nombre);

                TempData["Message"] = "Empresa Actualizada con Exito";
                return RedirectToAction("Index", "Empresa");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Empresa.Edit", $"Error al acualizar empresa con id {request.Id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo actualizar empresa";
                return RedirectToAction("Index", "Empresa");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Disable(int id)
        {
            try
            {
                var empresa = await context.Empresas.AsNoTracking().SingleAsync(e => e.Id == id);
                if (empresa == null)
                {
                    TempData["Error"] = "Empresa no encontrada";
                    return RedirectToAction("Index", "Empresa");
                }
                empresa.Active = 0;//desactivar empresa
                context.Empresas.Update(empresa);
                await context.SaveChangesAsync();
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Empresa.Disable", $"Se desactivó la empresa con id: {id}", session.nombre);
                TempData["Message"] = "Empresa desactivada con éxito";
                return RedirectToAction("Index", "Empresa");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Empresa.Disable", $"Error al desactivar empresa con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo desactivar Empresa";
                return RedirectToAction("Index", "Empresa");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Enable(int id)
        {
            try
            {
                var empresa = await context.Empresas.AsNoTracking().SingleAsync(e => e.Id == id);
                if (empresa == null)
                {
                    TempData["Error"] = "Empresa no encontrada";
                    return RedirectToAction("Index", "Empresa");
                }
                empresa.Active = 1;//activar empresa
                context.Empresas.Update(empresa);
                await context.SaveChangesAsync();
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Empresa.Enable", $"Se activó la empresa con id: {id}", session.nombre);
                TempData["Message"] = "Empresa activada con éxito";
                return RedirectToAction("Index", "Empresa");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Empresa.Enable", $"Error al activar empresa con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo activar Empresa";
                return RedirectToAction("Index", "Empresa");
            }
        }
    }
}
