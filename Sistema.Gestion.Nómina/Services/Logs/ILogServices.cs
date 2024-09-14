using Sistema.Gestion.Nómina.Models;

namespace Sistema.Gestion.Nómina.Services.Logs
{
    public interface ILogServices
    {
        public UserDataSession GetSessionData();
        public Task LogTransaction(int idEmpleado, int idEmpresa, string method, string data, string usuario);
        public Task LogError(int idEmpleado, int idEmpresa, string method, string data, string error, string stacktrace);

    }
}
