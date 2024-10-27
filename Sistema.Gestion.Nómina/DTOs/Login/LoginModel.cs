using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.DTOs.Login
{
    public class LoginModel
    {
        public Usuario Usuario { get; set; }
        public int IdEmployee {  get; set; }
        public int IdUser {  get; set; }
        public bool isBloqued {  get; set; }
    }
}
