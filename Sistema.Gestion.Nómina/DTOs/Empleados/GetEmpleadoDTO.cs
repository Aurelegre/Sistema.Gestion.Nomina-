using Sistema.Gestion.Nómina.DTOs.Familia;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class GetEmpleadoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Puesto { get; set; }
        public string Departamento { get; set; }
        public string DPI { get; set; }
        public decimal? Sueldo { get; set; }
        public DateOnly FechaContratado { get; set; }
        public string Usuario { get; set; }
        public List<GetFamilyEmployeeDTO>  Family { get; set;}
    }
}
