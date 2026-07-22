using Shouldly;
using SmartParking.Payments.PosTerminal;
using Xunit;

namespace SmartParking.Payments;

public class PosAesEncryptionServiceTests
{
    private readonly PosAesEncryptionService _service = new();
    private const string MasterKeyHex = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";

    [Fact]
    public void EncryptAndDecrypt_ShouldReturnOriginalPlaintext()
    {
        string original = "org_api_key_secret_2026";

        string encryptedBase64 = _service.EncryptApiKey(original, MasterKeyHex);
        encryptedBase64.ShouldNotBeNullOrEmpty();

        string decrypted = _service.DecryptApiKey(encryptedBase64, MasterKeyHex);
        decrypted.ShouldBe(original);
    }
}
