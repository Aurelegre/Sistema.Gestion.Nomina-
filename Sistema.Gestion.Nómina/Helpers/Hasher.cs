namespace Sistema.Gestion.Nómina.Helpers
{
    public class Hasher
    {
        public string HashPassword(string password)
        {
            // Genera un hash de la contraseña
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Verifica si el hash de la contraseña ingresada coincide con el hash almacenado
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }
}
