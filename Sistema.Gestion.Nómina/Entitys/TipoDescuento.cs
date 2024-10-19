namespace Sistema.Gestion.Nómina.Entitys
{
    public class TipoDescuento
    {
        public int Id { get; set; }

        public string? Descripcion { get; set; }

        public virtual ICollection<Descuento> Descuento { get; set; } = new List<Descuento>();
    }
}
