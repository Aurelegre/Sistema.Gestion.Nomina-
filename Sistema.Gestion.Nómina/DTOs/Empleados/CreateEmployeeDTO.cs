using Sistema.Gestion.Nómina.DTOs.Familia;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class CreateEmployeeDTO
    {
        [Required(ErrorMessage = "El DPI es obligatorio")]
        [StringLength(13, ErrorMessage = "El DPI debe tener 13 caracteres")]
        public string? Dpi { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder los 100 caracteres")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de contratación es obligatoria")]
        [DataType(DataType.Date, ErrorMessage = "Fecha inválida")]
        public DateOnly FechaContratado { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string? Usuario { get; set; }

        [Required(ErrorMessage = "El sueldo es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El sueldo debe ser un valor positivo")]
        public decimal? Sueldo { get; set; }

        [Required(ErrorMessage = "El puesto es obligatorio")]
        public int? IdPuesto { get; set; }

        [Required(ErrorMessage = "La empresa es obligatoria")]
        public int? IdEmpresa { get; set; }

        public int? Activo { get; set; } = 1;

        [Required(ErrorMessage = "El departamento es obligatorio")]
        public int? IdDepartamento { get; set; }
        [Required(ErrorMessage = "El rol es obligatorio")]
        public int? IdRol { get; set; }

        public List<GetFamilyEmployeeDTO> FamilyEmployeeDTOs { get; set; }
    }
}
