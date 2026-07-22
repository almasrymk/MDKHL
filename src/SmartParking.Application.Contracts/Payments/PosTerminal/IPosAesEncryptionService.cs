namespace SmartParking.Payments.PosTerminal;

public interface IPosAesEncryptionService
{
    string DecryptApiKey(string encryptedBase64, string masterKeyHex);
    string EncryptApiKey(string plaintext, string masterKeyHex);
}
