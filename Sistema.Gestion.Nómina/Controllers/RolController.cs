using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Roles;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    [Authorize]
    public class RolController(SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        // GET: RolController
        public async Task<ActionResult> Index(GetRolesDTO request)
        {
            try
            {
                var session = logger.GetSessionData();
                var query = context.Roles.Where(r => r.IdEmpresa == session.company && r.activo == 1);
                
                // Aplicar filtros
                if (!string.IsNullOrEmpty(request.Descripcion))
                {
                    query = query.Where(e => e.Descripcion.Contains(request.Descripcion));
                }
                //paginación
                var totalItems = await query.CountAsync();
                var roles = await query
                    .AsNoTracking()
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(u => new GetRolesResponse
                    {
                        Id = u.Id,
                        Descripcion = u.Descripcion,
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<GetRolesResponse>
                {
                    Items = roles,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };

                // Pasar filtros a la vista
                ViewBag.Descripcion = request.Descripcion;

                // Registrar en bitácora
                await logger.LogTransaction(session.idEmpleado, session.company, "Roles.Index", $"Se consultaron todos los Roles activos de la empresa {session.company} y se envió a la vista", session.nombre);

                return View(paginatedResult);
            }
            catch(Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Employees.Index", "Error al realizar el Get de todos los empleados activos", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar Empleados";
                return View();
            }
        }

        // GET: RolController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var rol = await context.Roles.Where(r => r.Id == id)
                                       .AsNoTracking()
                                       .Select(r => new GetRolResponse
                                       {
                                           Id = r.Id,
                                           Descripcion = r.Descripcion
                                       }).FirstOrDefaultAsync();
                if (rol == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Rol";
                    return RedirectToAction("Index", "Rol");
                }

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Rol.Details", $"Se consultaron detalles del rol con id: {id}, Nombre: {rol.Descripcion}", session.nombre);

                return Json(rol);
            }
            catch(Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Rol.Details", $"Error al consultar detalles del rol con Id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Rol";
                return RedirectToAction("Index", "Rol");
            }
            
        }

        // GET: RolController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RolController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateRolDTO request)
        {
            try
            {
                var existe = context.Roles.Where(u=> u.Descripcion == request.Descripcion).AsNoTracking().FirstOrDefault();
                if(existe != null)
                {
                    TempData["Error"] = "Ya Existe un Rol con este Nombre";
                    return RedirectToAction("Index", "Rol");
                }
                var session = logger.GetSessionData();
                Role rol = new Role
                {
                    Descripcion = request.Descripcion,
                    IdEmpresa = session.company,
                    activo =1
                };
                context.Add(rol);
                await context.SaveChangesAsync();
                //guardar bitacora
                await logger.LogTransaction(session.idEmpleado, session.company, "Rol.Create", $"Se creó Rol con Nombre: {request.Descripcion}", session.nombre);
                TempData["Message"] = "Rol creado con Exito";
                return RedirectToAction("Index", "Rol");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Rol.Create", $"Error al crear Rol", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo crear Rol";
                return RedirectToAction("Index", "Rol");
            }
        }

        // GET: RolController/Edit/5
        public ActionResult Edit()
        {
                return View();

        }

        // POST: RolController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditRolDTO request)
        {
            try
            {
                var rol = context.Roles.Where(u => u.Id == request.Id)
                                       .AsNoTracking()
                                       .FirstOrDefault();
                if (rol == null)
                {
                    TempData["Error"] = "Error al actualizar Rol";
                    return RedirectToAction("Index", "Employees");
                }
                rol.Descripcion = request.Descripcion;
                context.Update(rol);
                await context.SaveChangesAsync();

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Rol.Edit", $"Se actualizó Rol con id: {rol.Id}, Nombre: {rol.Descripcion}", session.nombre);

                TempData["Message"] = "Rol actualizado con Exito";
                return RedirectToAction("Index", "Rol");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Rol.Edit", $"Error al acualizar rol con id {request.Id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo actualizar Rol";
                return RedirectToAction("Index", "Rol");
            }
        }


        // POST: RolController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var rol = await context.Roles.Where(p => p.Id == id).AsNoTracking().FirstOrDefaultAsync();
                if (rol == null)
                {
                    TempData["Error"] = "El Rol no existe";
                    return RedirectToAction("Index", "Rol");
                }
                //verificar que no esté asignado a algún usuario Activo
                var usuario = await context.Usuarios.Where(p => p.IdRol == id && p.activo == 1 ).AsNoTracking().FirstOrDefaultAsync(); 
                if (usuario != null)
                {
                    TempData["Error"] = "No se puede eliminar Rol con usuarios asignados";
                    return RedirectToAction("Index", "Rol");
                }
                //desactivar ROL
                rol.activo = 0;
                context.Update(rol);
                await context.SaveChangesAsync();

                //guardar bitácora
                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Rol.Delete", $"Se eliminó Rol con id: {rol.Id}, Nombre: {rol.Descripcion}", session.nombre);

                TempData["Message"] = "Rol Eliminado con Exito";
                return RedirectToAction("Index", "Rol");
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Rol.Delete", $"Error al eliminar Rol con id {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo eliminar Empleado";
                return RedirectToAction("Index", "Rol");
            }
        }
    }
}
