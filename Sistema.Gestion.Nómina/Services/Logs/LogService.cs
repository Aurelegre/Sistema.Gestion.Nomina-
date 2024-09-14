using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.Services.Logs
{
    public class LogService : ILogServices
    {
        private readonly SistemaGestionNominaContext _dbcontext;

        public LogService(SistemaGestionNominaContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public async void LogTransaction(int idEmpleado, int idEmpresa, string method, string data)
        {
            var log = new LogTransaccione
            {
                IdEmpleado = idEmpleado,
                IdEmpresa = idEmpresa,
                Metodo = method,
                Descripcion = data,
                Fecha = DateTime.Now,
            };
            _dbcontext.Add(log);
            await _dbcontext.SaveChangesAsync();
        }

        public async void LogError(int idEmpleado, int idEmpresa, string method, string data, string error, string stacktrace)
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
