using LicenseGenerator.Services;
using LicenseGenerator.ViewModels;
using PleasantUI.Controls;

namespace LicenseGenerator;

public partial class MainWindow : PleasantWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel(new StorageService(this), new SettingsService());
        DataContext = _viewModel;
        Opened += async (_, _) => await _viewModel.InitializeAsync();
    }
}
