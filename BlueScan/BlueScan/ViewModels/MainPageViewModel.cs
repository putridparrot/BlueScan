using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using Acr.UserDialogs;
using BlueScan.Services;
using Bluetooth.Service;
using Plugin.BluetoothLE;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

namespace BlueScan.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private const string StartScan = "Start Scan";
        private const string StopScan = "Stop Scan";

        private readonly IDictionary<string, Tuple<IScanResult, ScanResultViewModel>> _scanResults;
        private readonly IBluetoothScanner _bluetoothScanner;
        private readonly INavigationService _navigationService;
        private readonly IOptions _options;
        private readonly IUserDialogs _userDialogs;
        private readonly IPersist _persist;
        private IDisposable _disposable;
        private string _scanStatus;
        private bool _isRefreshing;
        private bool _bluetoothDisabled;

        public MainPageViewModel(INavigationService navigationService, 
            IBluetoothScanner bluetoothScanner, IOptions options,
            IPersist persist, IUserDialogs userDialogs)
        {
            _bluetoothScanner = bluetoothScanner;
            _navigationService = navigationService;
            _options = options;
            _persist = persist;
            _userDialogs = userDialogs;

            _scanResults = new Dictionary<string, Tuple<IScanResult, ScanResultViewModel>>();

            _options
                .Timeout
                .Subscribe(i =>
                {
                    Interval = i;
                    if (ScanStatus == StopScan)
                    {
                        Refresh();
                    }
                });

            ScanCommand = new DelegateCommand(Scan, CanScan);
            AboutCommand = new DelegateCommand(About, CanAbout);
            OptionsCommand = new DelegateCommand(Options, CanOptions);
            CaptureCommand = new DelegateCommand(Capture, CanCapture);
            RefreshCommand = new DelegateCommand(Refresh);
            ClearCaptureCommand = new DelegateCommand(ClearCapture);
            EnableCommand = new DelegateCommand(Enable);

            Devices = new ObservableCollection<ScanResultViewModel>();
            Devices.CollectionChanged += (sender, args) => { RaisePropertyChanged(nameof(DevicesFound)); };
                //.RxWhenAnyPropertyChanged()
                //.Subscribe(p => RaisePropertyChanged(nameof(DevicesFound)));

            ScanStatus = StartScan;

            _bluetoothScanner
                .ObserveAdapterStatus()
                .Subscribe(s => { BluetoothDisabled = s != AdapterStatus.PoweredOn; });
        }

        private double Interval { get; set; }

        private void Refresh()
        {
            if (ScanStatus == StartScan)
            {
                Scan();
            }
            else
            {
                ScanStop();
                Scan();
            }
        }

        public string ScanStatus
        {
            get => _scanStatus;
            set
            {
                if (_scanStatus != value)
                {
                    _scanStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand ScanCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CaptureCommand { get; }
        public ICommand ClearCaptureCommand { get; }
        public ICommand EnableCommand { get; }

        public ObservableCollection<ScanResultViewModel> Devices { get; }

        public bool DevicesFound => Devices.Count > 0;

        private bool CanCapture()
        {
            return Devices.Count > 0;
        }

        private async void Capture()
        {
            try
            {
                // note: this will cause a enum exception if we are adding to the collection
                // need to fix
                await _persist.SaveAsync(Devices.Select(d => new ScanResultDataItem(d.ScanResult)));
            }
            catch (Exception e)
            {
                //Crashes.TrackError(e);
            }
        }

        private void Enable()
        {
            if (_bluetoothScanner.CanSetAdapterState())
            {
                _bluetoothScanner.SetAdapterStatus(true);
            }
            else
            {
                _userDialogs.Alert("Cannot change bluetooth adapter state.");
            }
        }

        private async void ClearCapture()
        {
            await _persist.ClearLocalAsync();
        }

        private bool CanAbout()
        {
            return true;
        }

        private async void About()
        {
            await _navigationService.NavigateAsync("AboutPage");
        }

        private bool CanOptions()
        {
            return true;
        }

        private async void Options()
        {
            await _navigationService.NavigateAsync("OptionsPage");
        }

        private bool CanScan()
        {
            return true;
        }

        private void ScanStop()
        {
            _disposable?.Dispose();
            _bluetoothScanner.StopScan();
        }

        private void Scan()
        {
            if (ScanStatus == StartScan)
            {
                IsRefreshing = true;

                ScanStatus = StopScan;

                ((DelegateCommand)CaptureCommand).RaiseCanExecuteChanged();

                _scanResults.Clear();
                Devices.Clear();

                _disposable?.Dispose();

                _disposable = _bluetoothScanner.Scan()
                    .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(Interval)))
                    .ObserveOn(Scheduler.Default) // need to look into whether this correctly marshal to ui
                    .Finally(() => { ScanStatus = StartScan; })
                    .Subscribe(sr =>
                    {
                        var key = sr.Device.ToString();
                        if (!_scanResults.TryGetValue(key, out var existing))
                        {
                            var vm = new ScanResultViewModel(sr, _navigationService);
                            _scanResults.Add(key, new Tuple<IScanResult, ScanResultViewModel>(sr, vm));
                            Devices.Add(vm);

                            ((DelegateCommand)CaptureCommand).RaiseCanExecuteChanged();
                        }
                        else
                        {
                            existing.Item2.Proximity.Current = sr.Rssi;
                            existing.Item2.Updated = DateTime.Now.ToString();
                        }

                        //sr.Device.WhenAnyCharacteristicDiscovered()
                        //sr.Device.DiscoverServices()
                    }, e =>
                    {
                        //Crashes.TrackError(e);
                        _userDialogs.Alert(e.ToString(), "Error");
                    });

                // we don't want the activity indicator to be
                // in the way, so show it then hide it
                IsRefreshing = false;
            }
            else
            {
                ScanStop();
            }
        }

        public bool BluetoothDisabled
        {
            get => _bluetoothDisabled;
            set
            {
                if (_bluetoothDisabled != value)
                {
                    _bluetoothDisabled = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
