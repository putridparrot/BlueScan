using System.Windows.Input;
using BlueScan.Common;
using Prism.Commands;
using SimpleCharts;
using Prism.Mvvm;
using Prism.Navigation;

namespace BlueScan.ViewModels
{
    public class HistoryPageViewModel : BindableBase
    {
        private ScanResultViewModel _model;
        private readonly INavigationService _navigationService;

        public HistoryPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            CloseCommand = new DelegateCommand(Close);
        }

        public void Initialize(ScanResultViewModel model)
        {
            _model = model;
            RaisePropertyChanged(nameof(Entries));
        }

        public FixedSizeObservableCollection<Entry> Entries => _model?.Proximity.Entries;
        public ICommand CloseCommand {get; }

        public void Close()
        {
            _navigationService.GoBackAsync();
        }
    }
}
