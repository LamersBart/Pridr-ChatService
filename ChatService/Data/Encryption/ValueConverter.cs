using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChatService.Data.Encryption;

public class EncryptedReferenceConverter<T> : ValueConverter<T?, string?> where T : class
{
    public EncryptedReferenceConverter()
        : base(
            v => v != null ? EncryptionHelper.Encrypt(v.ToString()!) : null, // Encrypt if not null
            v => !string.IsNullOrEmpty(v)
                ? (T?)Convert.ChangeType(EncryptionHelper.Decrypt(v), typeof(T)) 
                : null // Decrypt
        )
    { }
}
