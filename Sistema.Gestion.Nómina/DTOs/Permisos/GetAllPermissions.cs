using Sistema.Gestion.Nómina.Entitys;

namespace Sistema.Gestion.Nómina.DTOs.Permisos
{
    public class GetAllPermissions
    {
        public int? idPadre { get; set; }
        public string Padre { get; set; }
        public List<Permiso> Hijos { get; set; } = new List<Permiso>();
    }
}
