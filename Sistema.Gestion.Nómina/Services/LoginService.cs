using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Helpers;

namespace Sistema.Gestion.Nómina.Services
{
    public class LoginService
    {
        private readonly SistemaGestionNominaContext context;
        private readonly Hasher hasher;
        public LoginService(SistemaGestionNominaContext dbcontext, Hasher hasher) { 
            context = dbcontext;
            this.hasher = hasher;
        }

        public async Task<Usuario> LoginUser(string? userName, string? password)
        {
            var usuario = await context.Usuarios.SingleOrDefaultAsync(u => u.Usuario1 == userName);

            if (usuario == null || !hasher.VerifyPassword(password, usuario.Contraseña))
            {
                return null;
            }

            return usuario;
        }

    }
}
