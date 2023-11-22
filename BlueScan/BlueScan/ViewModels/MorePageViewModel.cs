using System.Windows.Input;
using Prism.Mvvm;
using Prism.Navigation;

namespace BlueScan.ViewModels
{
    public class MorePageViewModel : BindableBase, INavigatedAware
    {
        private readonly INavigationService _navigationService;

        public MorePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;


            History = new HistoryPageViewModel(_navigationService);
            Services = new ServicesPageViewModel(_navigationService);
        }

        public ICommand CloseCommand { get; }

        public void Close()
        {
            _navigationService.GoBackAsync();
        }

        public HistoryPageViewModel History { get; }

        public ServicesPageViewModel Services { get; }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            var model = parameters["model"] as ScanResultViewModel;
            if (model != null)
            {
                History.Initialize(model);
            }
        }
    }
}
