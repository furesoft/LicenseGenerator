using LicenseGenerator.Services;
using LicenseGenerator.ViewModels;
using PleasantUI.Controls;

namespace LicenseGenerator;

public partial class MainWindow : PleasantWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new StorageService(this));
    }
}
