using System.Windows.Input;
using BlueScan.Common;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SimpleCharts;

namespace BlueScan.ViewModels
{
    public class ServicesPageViewModel : BindableBase
    {
        private ScanResultViewModel _model;
        private readonly INavigationService _navigationService;

        public ServicesPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            CloseCommand = new DelegateCommand(Close);
        }

        public void Initialize(ScanResultViewModel model)
        {
            _model = model;
        }

        public ICommand CloseCommand { get; }

        public void Close()
        {
            _navigationService.GoBackAsync();
        }
    }
}