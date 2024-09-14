using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    [Authorize]
    public class Employees : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
