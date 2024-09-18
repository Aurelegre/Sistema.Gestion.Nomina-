namespace Sistema.Gestion.Nómina.DTOs.Usuarios
{
    public class GetUsuariosDTO
    {
        public int Id { get; set; }

        public string? Usuario1 { get; set; }

        public string? Contraseña { get; set; }

        public int? IdRol { get; set; }
    }
}
