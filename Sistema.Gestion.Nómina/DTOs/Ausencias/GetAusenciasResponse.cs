using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Sistema.Gestion.Nómina.DTOs.Ausencias
{
    public class GetAusenciasResponse
    {
        public bool IsJefe { get; set; }
        public int Id { get; set; }
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public DateOnly? FechaSolicitud { get; set; }
        public string Detalle { get; set; }
        public int? Estado { get; set; }
        public int? Deducible { get; set; }
    }
}
