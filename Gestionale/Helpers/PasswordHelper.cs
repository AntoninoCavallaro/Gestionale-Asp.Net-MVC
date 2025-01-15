using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    public static string HashPassword(string password, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = password + salt;  // Combina la password con il salt
            var bytes = Encoding.UTF8.GetBytes(saltedPassword);  // Converti in byte array
            var hash = sha256.ComputeHash(bytes);  // Calcola l'hash
            return Convert.ToBase64String(hash);  // Restituisci come stringa base64
        }
    }
}

