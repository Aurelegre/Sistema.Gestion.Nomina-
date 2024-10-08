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
    public class EmpresaController(SistemaGestionNominaContext context, ILogServices logger, IEmpresaServices empresaServices) : Controller
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
                        Telefono = u.Teléfono
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
                        Usuario = e.IdUsuarioNavigation.Usuario1
                    })
                    .FirstOrDefaultAsync();

                // Validar si no se encontró un administrador
                if (admin == null)
                {
                    TempData["Error"] = "No se encontró administrador para la empresa.";
                    return RedirectToAction("Index", "Empresa");
                }
                empresa.Administrador = admin.Nombre;

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
                        Teléfono = request.Telefono
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

    }
}
