namespace Sistema.Gestion.Nómina.Services.UnAuthenticate
{
    public interface IUnAuthenticateServices
    {
        public Task<bool> SetPassword(string password, int idUser);

    }
}
