namespace Sistema.Gestion.Nómina.DTOs.Empleados
{
    public class GetLiquidacionResponse
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public DateOnly? Contratado { get; set; }
        public DateOnly? Despedido { get; set; }
        public string? Diferencia { get; set; }
        public double? Liquidacion { get; set; }
        public decimal? Sueldo { get; set; }
    }
}
