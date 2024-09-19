namespace Sistema.Gestion.Nómina.DTOs.Permisos
{
    public class EditAsignedPermissionDTO
    {
        public List<check> permissions { get; set; }

    }

    public class check
    {
        public int idPermiso { get;set; }
        public string? estado { get; set; }
    }
}
