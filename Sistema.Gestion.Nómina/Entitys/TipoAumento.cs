namespace Sistema.Gestion.Nómina.Entitys
{
    public class TipoAumento
    {
        public int Id { get; set; }

        public string? Descripcion { get; set; }

        public virtual ICollection<Aumento> Aumento { get; set; } = new List<Aumento>();
    }
}
