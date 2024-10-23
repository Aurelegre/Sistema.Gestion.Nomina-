namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class GetEmployeesDTO
    {
        public string DPI { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public string Departamento { get; set; }
        public int estado { get; set; } = 1; //traer por defaul activos
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 2;
    }
}
