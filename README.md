# LicenseGenerator

A modern desktop app for creating, managing, and validating licenses based on **Standard.Licensing**.

## Highlights

- Manage products locally and generate a key pair per product
- Export Public/Private keys or load them from files
- Create and save licenses as **XML**
- Validate licenses and inspect metadata with ease
- Automatic update checks via **Velopack**

## Features

### License creation

- Capture customer details
- Set an expiration date or create a perpetual license
- Limit maximum usage
- Add product features
- Output and save the license as XML

### Validation & inspection

- Load or paste license XML
- Use a Public Key for signature verification
- Validate the license
- View license details such as ID, type, customer, features, and attributes

### Product management

- Select an existing product or create a new one at startup
- Product data is stored locally under `%AppData%\LicenseGenerator\products`
- Export key files as `key.pub` and `key.priv`

## Requirements

- Windows
- .NET 10 SDK

## Running

```powershell
dotnet restore .\src\LicenseGenerator\LicenseGenerator.csproj
dotnet run --project .\src\LicenseGenerator\LicenseGenerator.csproj
```

## Project structure

- `src\LicenseGenerator\App.axaml.cs` - startup flow and update check
- `src\LicenseGenerator\Services\` - licensing, product, and storage services
- `src\LicenseGenerator\ViewModels\` - application logic
- `src\LicenseGenerator\Views\` - UI for creating and validating licenses

## Technology

- Avalonia UI
- CommunityToolkit.Mvvm
- Standard.Licensing
- Velopack
