namespace Sistema.Gestion.Nómina.DTOs.Permisos
{
    public class GetAssignedPermissions
    {
        public int? idPadre { get; set; }
        public string Padre { get; set; }
        public int idPermiso { get; set; }
        public string Nombre { get; set; }
    }
}
