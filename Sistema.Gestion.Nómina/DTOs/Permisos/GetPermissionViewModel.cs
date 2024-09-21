namespace Sistema.Gestion.Nómina.DTOs.Permisos
{
    public class GetPermissionViewModel
    {
        public int idRol { get; set; }
        public string nameRol { get; set; }
        public List<GetAllPermissions> allPermissions { get; set; }
        public List<GetAssignedPermissions> assignedPermissions { get; set; }
    }
}
