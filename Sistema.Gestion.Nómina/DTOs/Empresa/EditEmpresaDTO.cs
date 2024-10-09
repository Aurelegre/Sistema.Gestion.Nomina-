namespace Sistema.Gestion.Nómina.DTOs.Empresa
{
    public class EditEmpresaDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Usuario { get; set; }
        public string? Contrasena { get; set; }
    }
}
