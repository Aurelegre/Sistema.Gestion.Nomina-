namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class UpdateEmpleadoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public int IdPuesto { get; set; }
        public int IdDepartamento { get; set; }
        public int IdRol { get; set; }
        public decimal? Sueldo { get; set; }
        public IFormFile expedientePDF { get; set; }
    }
}
