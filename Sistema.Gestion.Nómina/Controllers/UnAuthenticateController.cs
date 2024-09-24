using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.UnAuthenticate;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.UnAuthenticate;

namespace Sistema.Gestion.Nómina.Controllers
{
    public class UnAuthenticateController(SistemaGestionNominaContext context, ILogServices logger, IUnAuthenticateServices unAuthenticateServices) : Controller
    {
        // GET: UnAuthenticateController
        public ActionResult FirstSession()
        {
            return View();
        }

        // GET: UnAuthenticateController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UnAuthenticateController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UnAuthenticateController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActiveUser(ActiveUserDTO request)
        {
            try
            {
                //estandarización
                request.Usuario = request.Usuario.ToUpper();
                if (request.Password1 != request.Password2)
                {
                    TempData["Error"] = "Las contraseñas no coinciden";
                    return RedirectToAction("FirstSession", "UnAuthenticate");
                }
                //verificar que el dpi exista en los empleados
                var employeeWithUser = await context.Empleados.Where(u => u.Dpi == request.Dpi)
                                                              .AsNoTracking()
                                                              .Select(e => new
                                                              {
                                                                  Employe = e,
                                                                  User = e.IdUsuarioNavigation
                                                              }).FirstOrDefaultAsync();
                if (employeeWithUser == null || employeeWithUser.User == null)
                {
                    TempData["Error"] = "El Usuario no existe";
                    return RedirectToAction("FirstSession", "UnAuthenticate");
                }
         
                var user = employeeWithUser.User;

                // Verificar si el usuario ya está activo
                if (user.activo == 1 || !string.IsNullOrEmpty(user.Contraseña))
                {
                    TempData["Error"] = "El Usuario ya se encuentra activo";
                    return RedirectToAction("FirstSession", "UnAuthenticate");
                }
                //setear contraseña
                var result = await unAuthenticateServices.SetPassword(request.Password1, user.Id);
                if (!result)
                {
                    TempData["Error"] = "Error al activar usurio, vuelva a intentar";
                    return RedirectToAction("FirstSession", "UnAuthenticate");
                }
                //guardar sessión
                await logger.LogTransaction(employeeWithUser.Employe.Id, employeeWithUser.Employe.IdEmpresa, "ActiveUser", $"Activación del usuario con id:{user.Id}", user.Usuario1);
                TempData["Message"] = "Usuario activado con éxito";
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                await logger.LogError(1, 1, "ActiveUser", $"Error al Activar usuario:{request.Usuario} y con DPI {request.Dpi}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al activar usurio, vuelva a intentar";
                return RedirectToAction("FirstSession", "UnAuthenticate");
            }
        }

        // GET: UnAuthenticateController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UnAuthenticateController/Edit/5
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

        // GET: UnAuthenticateController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UnAuthenticateController/Delete/5
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
