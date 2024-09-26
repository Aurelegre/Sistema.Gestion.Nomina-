using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.DTOs.Login
{
    public class LoginModel
    {
        public Usuario Usuario { get; set; }
        public bool isBloqued {  get; set; }
    }
}
