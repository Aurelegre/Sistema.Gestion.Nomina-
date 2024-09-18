namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class UpdateEmpleadoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int IdPuesto { get; set; }
        public int IdDepartamento { get; set; }
        public int IdUsuario { get; set; }
        public decimal? Sueldo { get; set; }
    }
}
