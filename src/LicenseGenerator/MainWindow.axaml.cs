using System.Threading.Tasks;
using LicenseGenerator.Services;
using LicenseGenerator.ViewModels;
using PleasantUI.Controls;

namespace LicenseGenerator;

public partial class MainWindow : PleasantWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow() : this(new ProductService(new LicensingService()))
    {
    }

    public MainWindow(IProductService productService)
    {
        InitializeComponent();
        _viewModel = new MainViewModel(new StorageService(this), productService);
        DataContext = _viewModel;
    }

    public Task InitializeProductAsync(string productName, string passphrase)
        => _viewModel.SelectProductAsync(productName, passphrase);
}
