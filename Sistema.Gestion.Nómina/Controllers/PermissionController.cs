using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Permisos;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class PermissionController (SistemaGestionNominaContext context, ILogServices logger, IMapper _mapper) : Controller
    {
        // GET: PermissionController
        public async Task<ActionResult> Index(int idRol)
        {
            try
            {
                var allPermissions = await context.Permisos
                                          .Where(u => u.Padre == null)
                                          .Select(u => new GetAllPermissions
                                          {
                                              idPadre = u.Id,
                                              Padre = u.Nombre,
                                              Hijos = context.Permisos.Where(b => b.Padre == u.Id).ToList()
                                          }).ToListAsync();
                var asignedPermissions = await context.RolesPermisos.Include(p => p.IdPermisoNavigation)
                                                      .Where(p => p.IdRol == idRol && p.IdPermisoNavigation.Padre !=null)
                                                      .Select(u => new GetAssignedPermissions
                                                      {
                                                          idPadre = u.IdPermisoNavigation.Padre,
                                                          Padre = context.Permisos.Where(a => a.Id == u.IdPermisoNavigation.Padre).Select(a => a.Nombre).FirstOrDefault(),
                                                          idPermiso = u.IdPermisoNavigation.Id,
                                                          Nombre = u.IdPermisoNavigation.Nombre
                                                      }).ToListAsync();
                GetPermissionViewModel getPermissionViewModel = new GetPermissionViewModel
                {
                    assignedPermissions = asignedPermissions,
                    allPermissions = allPermissions,
                    idRol = idRol,
                };

                var session = logger.GetSessionData();
                await logger.LogTransaction(session.idEmpleado, session.company, "Permission.Index", $"Se consultaron los permisos del rol: {idRol}", session.nombre);
                return View(getPermissionViewModel);
            }
            catch (Exception ex)
            {
                var session = logger.GetSessionData();
                await logger.LogError(session.idEmpleado, session.company, "Permission.Index", $"Error al consultar permisos del rol: {idRol}", ex.Message, ex.StackTrace);
                TempData["Error"] = "No se pudo consultar Permisos";
                return RedirectToAction("Index", "Rol");
            }
        }


        // POST: PermissionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditAsignedPermissionDTO request, int idRol)
        {
            //crear transacción para evitar inconsistencias
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Obtener permisos actuales
                    var oldPermissions = await context.RolesPermisos
                                        .Where(p => p.IdRol == idRol)
                                        .AsNoTracking()
                                        .ToListAsync();

                    // Permisos a eliminar
                    var toDelete = oldPermissions.Where(op => !request.permissions.Any(r => r.idPermiso == op.IdPermiso && !string.IsNullOrEmpty(r.estado))).ToList();

                    // Permisos a agregar
                    var toAdd = request.permissions.Where(r => !string.IsNullOrEmpty(r.estado) && !oldPermissions.Any(op => op.IdPermiso == r.idPermiso)).ToList();

                    // Eliminar permisos
                    if (toDelete.Any())
                    {
                        context.RolesPermisos.RemoveRange(toDelete);
                        await context.SaveChangesAsync();
                    }

                    // Agregar nuevos permisos
                    foreach (var permission in toAdd)
                    {
                        RolesPermiso newPermission = new RolesPermiso
                        {
                            IdRol = idRol,
                            IdPermiso = permission.idPermiso
                        };
                        context.RolesPermisos.Add(newPermission);
                    }

                    if (toAdd.Any())
                    {
                        await context.SaveChangesAsync();
                    }
                    await transaction.CommitAsync();

                    var session = logger.GetSessionData();
                    await logger.LogTransaction(session.idEmpleado, session.company, "Permission.Edit", $"Se editaron los permisos del rol: {idRol}", session.nombre);
                    TempData["Message"] = "Permisos actualizado con Exito";
                    //return RedirectToAction("Index", "Permission", new { idRol = idRol });
                    return RedirectToAction("Index", "Rol");

                }
                catch (Exception ex)
                {
                    var session = logger.GetSessionData();
                    await logger.LogError(session.idEmpleado, session.company, "Permission.Edit", $"Error al acualizar permisos del rol: {idRol}", ex.Message, ex.StackTrace);
                    TempData["Error"] = "No se pudo actualizar Permisos";
                    return RedirectToAction("Index", "Rol");
                }
            }
        }


    }
}
