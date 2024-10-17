﻿namespace Sistema.Gestion.Nómina.DTOs.Prestamos
{
    public class GetHistorialResponse
    {
        public int Id { get; set; }
        public int? CPendientes { get; set; }
        public decimal? TotalPediente { get; set; }
        public List<GetHistorialDTO> Pagos { get; set;}
    }
}
