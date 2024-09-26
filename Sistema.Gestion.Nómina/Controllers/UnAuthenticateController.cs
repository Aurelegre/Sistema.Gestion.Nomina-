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
        // GET: UnAuthenticateController/FirstSession
        public ActionResult FirstSession()
        {
            return View();
        }

        // GET: UnAuthenticateController/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: UnAuthenticateController/ActiveUser
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
                    return RedirectToAction("login", "login");
                }
         
                var user = employeeWithUser.User;

                // Verificar si el usuario ya está activo
                if (user.activo == 1 || !string.IsNullOrEmpty(user.Contraseña))
                {
                    TempData["Error"] = "El Usuario ya se encuentra activo";
                    return RedirectToAction("login", "login");
                }
                //setear contraseña
                var result = await unAuthenticateServices.SetPassword(request.Password1, user.Id);
                if (!result)
                {
                    TempData["Error"] = "Error al activar usuario, vuelva a intentar";
                    return RedirectToAction("login", "login");
                }
                //guardar sessión
                await logger.LogTransaction(employeeWithUser.Employe.Id, employeeWithUser.Employe.IdEmpresa, "ActiveUser", $"Activación del usuario con id:{user.Id}", user.Usuario1);
                TempData["Message"] = "Usuario activado con éxito";
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                await logger.LogError(1, 1, "ActiveUser", $"Error al Activar usuario:{request.Usuario} y con DPI {request.Dpi}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al activar usuario, vuelva a intentar";
                return RedirectToAction("login", "login");
            }
        }

        // POST: UnAuthenticateController/RestorePassWord/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestorePassWord(RestorePasswordDTO request)
        {
            try
            {
                //estandarización
                request.Usuario = request.Usuario.ToUpper();
                if (request.Password1 != request.Password2)
                {
                    TempData["Error"] = "Las contraseñas no coinciden";
                    return RedirectToAction("ForgotPassword", "UnAuthenticate");
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
                    return RedirectToAction("login", "login");
                }
                var user = employeeWithUser.User;
                //verificar que esté activo
                if (string.IsNullOrEmpty(user.Contraseña))
                {
                    TempData["Error"] = "Usuario no configurado, debe activarlo";
                    return RedirectToAction("login", "login");
                }
                // Verificar si el usuario está bloqueado
                if (user.activo == 0)
                {
                    TempData["Error"] = "El Usuario está bloqueado, comuniquese con soporte.";
                    return RedirectToAction("login", "login");
                }
                
                //setear contraseña
                var result = await unAuthenticateServices.SetPassword(request.Password1, user.Id);
                if (!result)
                {
                    TempData["Error"] = "Error al activar usuario, vuelva a intentar";
                    return RedirectToAction("login", "login");
                }
                //guardar sessión
                await logger.LogTransaction(employeeWithUser.Employe.Id, employeeWithUser.Employe.IdEmpresa, "RestorePassWord", $"recuperación de contraseña del usuario con id:{user.Id}", user.Usuario1);
                TempData["Message"] = "Contraseña recuperada con éxito";
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                await logger.LogError(1, 1, "RestorePassWord", $"Error al recuperar contraseña del usuario:{request.Usuario} y con DPI {request.Dpi}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al recuperar contraseña, vuelva a intentar";
                return RedirectToAction("login", "login");
            }
        }

    }
}
