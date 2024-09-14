using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using System.Security.Claims;

namespace Sistema.Gestion.Nómina.Services.Logs
{
    public class LogService : ILogServices
    {
        private readonly SistemaGestionNominaContext _dbcontext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Inyecta IHttpContextAccessor en el constructor
        public LogService(SistemaGestionNominaContext dbcontext, IHttpContextAccessor httpContextAccessor)
        {
           this._dbcontext = dbcontext;
            _httpContextAccessor = httpContextAccessor;
        }

        public UserDataSession GetSessionData()
        {
            // Accede al HttpContext para obtener el usuario
            var user = _httpContextAccessor.HttpContext.User;

            // Obtener los datos de los claims
            var usuario = user.FindFirst(ClaimTypes.Name)?.Value;        // Nombre de usuario
            var rol = user.FindFirst(ClaimTypes.Role)?.Value;            // Rol del usuario
            var company = user.FindFirst("Company")?.Value;            // ID de la empresa
            var empleadoId = user.FindFirst("IdEmployed")?.Value;        // ID del empleado

            // Retornar los datos como sea necesario
            return new UserDataSession
            {
                nombre = usuario,
                rol = rol,
                idEmpleado = int.Parse(empleadoId),
                company = int.Parse(company)
            };
        }

        public async Task LogTransaction(int idEmpleado, int idEmpresa, string method, string data, string usuario)
        {
            var log = new LogTransaccione
            {
                IdEmpleado = idEmpleado,
                IdEmpresa = idEmpresa,
                Metodo = method,
                Descripcion = data,
                Fecha = DateTime.Now,
                Usuario = usuario
            };
            _dbcontext.Add(log);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task LogError(int idEmpleado, int idEmpresa, string method, string data, string error, string stacktrace)
        {
            var log = new LogError
            {
                IdEmpleado = idEmpleado,
                IdEmpresa = idEmpresa,
                Metodo = method,
                Descripcion = data,
                Error = error,
                StackTrace = stacktrace,
                Fecha = DateTime.Now,
            };
            _dbcontext.Add(log);
            await _dbcontext.SaveChangesAsync();
        }
    }
}
