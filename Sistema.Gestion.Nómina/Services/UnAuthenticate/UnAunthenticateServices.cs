using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Helpers;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Services.UnAuthenticate
{
    public class UnAunthenticateServices(SistemaGestionNominaContext context, Hasher hasher, ILogServices logger) : IUnAuthenticateServices
    {
        public async Task<bool> SetPassword(string password,int idUser)
        {
            try
            {
                //seleccionar usuario
                var user = await context.Usuarios.Where(o => o.Id == idUser).AsNoTracking().FirstOrDefaultAsync();
                //hashear contraseña
                string hash =  hasher.HashPassword(password);
                //setear hash
                user.Contraseña = hash;
                //actualizar en BD
                context.Usuarios.Update(user);
                await context.SaveChangesAsync();

                //guardar bitácora
                await logger.LogTransaction(1, user.IdEmpresa, "SetPassword", $"Se cambió contraseña a usuario con id: {idUser}", "UnAuthenticate");
                return true;
            }catch (Exception ex)
            {
                await logger.LogError(1, 1, "SetPassword", $"Error al setear contraseña a usuario con id: {idUser}", ex.Message, ex.StackTrace);
                return false;
            }
        }
    }
}
