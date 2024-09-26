using System.ComponentModel.DataAnnotations;

namespace Sistema.Gestion.Nómina.DTOs.UnAuthenticate
{
    public class RestorePasswordDTO
    {
        public string? Usuario { get; set; }
        public string Password1 { get; set; }
        public string Password2 { get; set; }
        [StringLength(13, ErrorMessage = "El DPI debe tener 13 caracteres")]
        public string? Dpi { get; set; }
    }
}
