using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;

namespace Alimnetar2
{
    public static class CryptoHelper
    {
        public static string EncryptAES(string plainText, string password)
        {
            // En un escenario real, querrás usar un salt aleatorio y guardarlo.
            // Para simplicidad, aquí no usamos salt dinámico.

            // Convertir todo a bytes
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Ajustar tamaño del password para la clave (por ejemplo, 32 bytes -> AES-256)
            // Podemos usar SHA256 para derivar la clave rápidamente (no es PBKDF2, pero sirve de demo):
            using (SHA256 sha = SHA256.Create())
            {
                passwordBytes = sha.ComputeHash(passwordBytes);
            }

            byte[] encrypted;

            // Crear objeto AES
            using (Aes aes = Aes.Create())
            {
                aes.Key = passwordBytes;
                aes.IV = new byte[16]; // IV de 16 bytes en cero (para demo)
                                       // Lo recomendable es usar IV aleatorio distinto por cada cifrado.

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                    }
                    encrypted = ms.ToArray();
                }
            }
            // Retornar en Base64
            return Convert.ToBase64String(encrypted);
        }

        // (Opcional) Método para desencriptar, si más adelante necesitas leerlo
        public static string DecryptAES(string cipherTextBase64, string password)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using (SHA256 sha = SHA256.Create())
            {
                passwordBytes = sha.ComputeHash(passwordBytes);
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = passwordBytes;
                aes.IV = new byte[16]; // IV en cero, igual que en el cifrado

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.FlushFinalBlock();

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}
