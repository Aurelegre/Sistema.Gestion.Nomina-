using Sistema.Gestion.Nómina.Models;

namespace Sistema.Gestion.Nómina.Services.Logs
{
    public interface ILogServices
    {
        public UserDataSession GetSessionData();
        public void LogTransaction(int idEmpleado, int idEmpresa, string method, string data, string usuario);
        public void LogError(int idEmpleado, int idEmpresa, string method, string data, string error, string stacktrace);

    }
}
