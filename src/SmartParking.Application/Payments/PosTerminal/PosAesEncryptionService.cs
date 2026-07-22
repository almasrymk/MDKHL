using System;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace SmartParking.Payments.PosTerminal;

public class PosAesEncryptionService : IPosAesEncryptionService, ISingletonDependency
{
    public string DecryptApiKey(string encryptedBase64, string masterKeyHex)
    {
        if (string.IsNullOrWhiteSpace(encryptedBase64))
            throw new ArgumentException("Encrypted API key payload cannot be empty.", nameof(encryptedBase64));

        if (string.IsNullOrWhiteSpace(masterKeyHex) || masterKeyHex.Length != 64)
            throw new ArgumentException("AES master key must be a 64-character hex string (32 bytes).", nameof(masterKeyHex));

        byte[] key = Convert.FromHexString(masterKeyHex);
        byte[] fullBytes = Convert.FromBase64String(encryptedBase64);

        if (fullBytes.Length < 30)
            throw new InvalidOperationException("Invalid encrypted payload length (minimum 30 bytes required).");

        byte version = fullBytes[0];
        if (version != 0x01)
            throw new InvalidOperationException($"Unsupported encrypted payload version: {version}");

        ReadOnlySpan<byte> nonce = fullBytes.AsSpan(1, 12);
        ReadOnlySpan<byte> tag = fullBytes.AsSpan(13, 16);
        ReadOnlySpan<byte> ciphertext = fullBytes.AsSpan(29);

        byte[] plaintextBytes = new byte[ciphertext.Length];
        using (var aesGcm = new AesGcm(key, 16))
        {
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);
        }

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    public string EncryptApiKey(string plaintext, string masterKeyHex)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
            throw new ArgumentException("Plaintext cannot be empty.", nameof(plaintext));

        if (string.IsNullOrWhiteSpace(masterKeyHex) || masterKeyHex.Length != 64)
            throw new ArgumentException("AES master key must be a 64-character hex string (32 bytes).", nameof(masterKeyHex));

        byte[] key = Convert.FromHexString(masterKeyHex);
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] tag = new byte[16];

        using (var aesGcm = new AesGcm(key, 16))
        {
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        byte[] result = new byte[1 + 12 + 16 + ciphertext.Length];
        result[0] = 0x01; // Version
        Buffer.BlockCopy(nonce, 0, result, 1, 12);
        Buffer.BlockCopy(tag, 0, result, 13, 16);
        Buffer.BlockCopy(ciphertext, 0, result, 29, ciphertext.Length);

        return Convert.ToBase64String(result);
    }
}
