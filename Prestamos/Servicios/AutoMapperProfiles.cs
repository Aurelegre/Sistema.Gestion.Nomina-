using AutoMapper;
using Microsoft.Identity.Client;
using Prestamos.Entitys;
using Prestamos.Models;

namespace Prestamos.Servicios
{
    public class AutoMapperProfiles : Profile
    { 
        public AutoMapperProfiles() 
        {
            CreateMap<PrestamoDTO, Prestamo>();

        }
    
    }
    
}
