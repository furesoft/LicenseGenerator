namespace LicenseGenerator.Models;

public class ProductSelectionResult
{
    public string ProductName { get; set; } = string.Empty;
    public bool IsNewProduct { get; set; }
    public string NewProductPassphrase { get; set; } = string.Empty;
}
