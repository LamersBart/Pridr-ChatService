using System.Security.Cryptography;
using System.Text;

namespace ChatService.Data.Encryption;

public static class EncryptionHelper
{
    private static string? _encryptionKey;

    public static void Initialize()
    {
        _encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY")!;
        if (string.IsNullOrEmpty(_encryptionKey))
        {
            throw new InvalidOperationException("Encryption key is missing in appsettings.");
        }
    }

    private static byte[] GetKey()
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey!));
        }
    }

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using (Aes aes = Aes.Create())
        {
            aes.Key = GetKey();
            aes.GenerateIV(); // Dynamische IV voor elke encryptie
            var iv = aes.IV;

            using (var ms = new MemoryStream())
            {
                // Sla de IV op als de eerste 16 bytes
                ms.Write(iv, 0, iv.Length);

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = GetKey();

            // Haal de IV uit de eerste 16 bytes van de ciphertext
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);

            // Haal de echte ciphertext eruit
            var cipherBytes = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using (var ms = new MemoryStream(cipherBytes))
            using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
            using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cryptoStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}