using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.Services.Empresa
{
    public interface IEmpresaServices
    {
        public Task<int> CreateUser (int idEmprsa, string usuario, string contraseña);
        public Task<int> CreateAdmin (int idEmprsa, string nombre, string dpi, int idUsuario);


    }
}
