using System.Security.Cryptography;
using System.Text;

namespace PROG_CMCS_Part1.Services
{
    public class FileEncryptionService
    {
        // Encryption key (must be 32 bytes for AES-256)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("MyUltraSecureKey1234567890123456");
        // Initialization vector (must be 16 bytes for AES)
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("MyInitVector1234");
        // Encrypts the provided input stream and saves it to the specified output path
        public async Task EncryptFileAsync(Stream input, string outputPath)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                // Use CBC mode for secure encryption
                aes.Mode = CipherMode.CBC;
                // Ensure proper padding
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                // Write encrypted data to file
                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                {
                    await input.CopyToAsync(cryptoStream);
                }
            }
        }
        // Decrypts a file at the given path and returns its contents as a MemoryStream
        public async Task<MemoryStream> DecryptFileAsync(string encryptedFilePath)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                // Use CBC mode to match encryption
                aes.Mode = CipherMode.CBC;
                // Ensure padding is correctly removed
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (FileStream fileStream = new FileStream(encryptedFilePath, FileMode.Open))
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                {
                    MemoryStream decryptStream = new MemoryStream();
                    // Copy decrypted bytes into memory stream
                    await cryptoStream.CopyToAsync(decryptStream);
                    // Reset stream position so it can be read from the beginning
                    decryptStream.Position = 0;
                    return decryptStream;
                }
            }
        }
    }
}
