using System;
using System.Collections.Generic;
using System.Linq;
using LicenseGenerator.Models;
using Standard.Licensing;
using Standard.Licensing.Security.Cryptography;
using Standard.Licensing.Validation;

namespace LicenseGenerator.Services;

public class LicensingService
{
    public KeyPairModel GenerateKeyPair(string passphrase)
    {
        var keyGenerator = KeyGenerator.Create();
        var keyPair = keyGenerator.GenerateKeyPair();

        return new KeyPairModel
        {
            PublicKey = keyPair.ToPublicKeyString(),
            PrivateKey = keyPair.ToEncryptedPrivateKeyString(passphrase),
            Passphrase = passphrase
        };
    }

    public string CreateLicense(LicenseModel model, KeyPairModel keys)
    {
        var builder = License.New()
            .WithUniqueIdentifier(Guid.NewGuid())
            .As(LicenseType.Standard)
            .WithMaximumUtilization(model.MaxUsages)
            .LicensedTo(customer =>
            {
                customer.Name = model.CustomerName;
                customer.Email = model.CustomerEmail;
                customer.Company = model.CustomerCompany;
            });

        if (model.ExpirationDate.HasValue)
            builder = builder.ExpiresAt(model.ExpirationDate.Value);

        if (model.AdditionalAttributes.Count > 0)
            builder = builder.WithAdditionalAttributes(model.AdditionalAttributes);

        if (model.ProductFeatures.Count > 0)
        {
            var features = new Dictionary<string, string>();
            foreach (var f in model.ProductFeatures)
                features[f] = "enabled";
            builder = builder.WithProductFeatures(features);
        }

        var license = builder.CreateAndSignWithPrivateKey(keys.PrivateKey, keys.Passphrase);
        return license.ToString();
    }

    public (bool IsValid, IList<IValidationFailure> Failures) ValidateLicense(string licenseXml, string publicKey)
    {
        License license;
        try
        {
            license = License.Load(licenseXml);
        }
        catch (Exception ex)
        {
            return (false, [new GeneralValidationFailure { Message = "Invalid license format", HowToResolve = ex.Message }]);
        }

        // ToList() is critical — AssertValidLicense() returns a lazy IEnumerable
        var failures = license.Validate()
            .ExpirationDate()
            .And()
            .Signature(publicKey)
            .AssertValidLicense()
            .ToList();

        return (failures.Count == 0, failures);
    }

    public License? LoadLicense(string licenseXml)
    {
        try
        {
            return License.Load(licenseXml);
        }
        catch
        {
            return null;
        }
    }
}
