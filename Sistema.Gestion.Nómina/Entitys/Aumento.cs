namespace Sistema.Gestion.Nómina.Entitys
{
    public class Aumento
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; } 
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
        public int IdTipo { get; set; }
        public virtual Empleado? IdEmpleadoNavigation { get; set; }

        public virtual TipoAumento? IdTipoNavigation { get; set; }
    }
}
