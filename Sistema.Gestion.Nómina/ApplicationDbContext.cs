using Microsoft.EntityFrameworkCore;

namespace Sistema.Gestion.Nómina
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
