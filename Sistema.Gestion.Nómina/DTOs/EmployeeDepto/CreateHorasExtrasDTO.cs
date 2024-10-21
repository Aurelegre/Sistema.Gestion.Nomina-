namespace Sistema.Gestion.Nómina.DTOs.EmployeeDepto
{
    public class CreateHorasExtrasDTO
    {
        public TimeOnly Tiempo { get; set; }
        public int IdEmpleado { get; set; }
        public int Tipo { get; set; }

    }
}
