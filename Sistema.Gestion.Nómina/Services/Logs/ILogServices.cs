namespace Sistema.Gestion.Nómina.Services.Logs
{
    public interface ILogServices
    {
        public void LogTransaction(int idEmpleado, int idEmpresa, string method, string data);
        public void LogError(int idEmpleado, int idEmpresa, string method, string data, string error, string stacktrace);

    }
}
