namespace Sistema.Gestion.Nómina.DTOs.EmployeeDepto
{
    public class GetEmployeesDeptoDTO
    {
        public string DPI { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
