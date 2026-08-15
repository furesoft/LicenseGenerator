using System;
using System.Collections.Generic;

namespace LicenseGenerator.Models;

public class LicenseModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerCompany { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public int MaxUsages { get; set; } = 1;
    public Dictionary<string, string> AdditionalAttributes { get; set; } = new();
    public List<string> ProductFeatures { get; set; } = new();
}
