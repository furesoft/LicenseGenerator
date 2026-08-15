namespace LicenseGenerator.Models;

public class KeyPairModel
{
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string Passphrase { get; set; } = string.Empty;
}
