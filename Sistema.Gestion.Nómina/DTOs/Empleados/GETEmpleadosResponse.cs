using Sistema.Gestion.Nómina.DTOs.Departamentos;

namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
	public class GETEmpleadosResponse
	{
		public int Id { get; set; }
		public string Nombre { get; set; }
		public string Puesto { get; set; }
		public string Departamento { get; set; }
		public string DPI { get; set; }
		public int? estado {  get; set; }
		public int? idUser {get; set; }

    }
}
