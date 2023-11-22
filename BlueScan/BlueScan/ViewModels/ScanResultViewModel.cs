using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using BlueScan.Extensions;
using Plugin.BluetoothLE;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Xamarin.Forms;

namespace BlueScan.ViewModels
{
    public class ScanResultViewModel : BindableBase
    {
        private readonly IScanResult _scanResult;
        private readonly INavigationService _navigationService;
        private readonly ProximityViewModel _proximity;
        private int _services;
        private string _manufacturerData;
        private ImageSource _manufacturerImage;
        private string _updated;
        private string _company;
        private string _name;

#if DEBUG
        private readonly IDictionary<string, List<IGattCharacteristic>> _gattServices = new ConcurrentDictionary<string, List<IGattCharacteristic>>();
#endif

        public ScanResultViewModel(IScanResult scanResult,
            INavigationService navigationService)
        {
            _scanResult = scanResult;
            _navigationService = navigationService;

            _proximity = new ProximityViewModel(_scanResult.Rssi);
            Updated = DateTime.Now.ToString();

            var ad = _scanResult.AdvertisementData;
            if (ad != null)
            {
                _proximity.TxPower = ad.TxPower;
                Services = ad.ServiceUuids?.Length ?? 0;
                if (ad.ManufacturerData != null)
                {
                    ManufacturerData = BitConverter.ToString(ad.ManufacturerData);

                    var companyId = ad.ManufacturerData[0] + (ad.ManufacturerData[1] << 8);
                    Company = CompanyIdentifiers.GetDisplayName(companyId);
                }

                ManufacturerImage = ManufacturerImages.Get(Company);
            }

            _name = String.IsNullOrEmpty(_scanResult.Device.Name) ? "N/A" : _scanResult.Device.Name;

            Debug.WriteLine(_scanResult.Device.Uuid);

            MoreCommand = new DelegateCommand(More);
        }

        public ICommand MoreCommand { get; }

        private async void More()
        {
            var navigationParams = new NavigationParameters
            {
                {"model", this }
            };
            await _navigationService.NavigateAsync("MorePage", navigationParams, true);
        }

        public string BdAddress => AddressExtensions.ToBdAddress(_scanResult.Device.Uuid);

        public string Updated
        {
            get => _updated;
            set
            {
                if (_updated != value)
                {
                    _updated = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ProximityViewModel Proximity => _proximity;

        public int Services
        {
            get => _services;
            set
            {
                if (_services != value)
                {
                    _services = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ManufacturerData
        {
            get => _manufacturerData;
            set
            {
                if (_manufacturerData != value)
                {
                    _manufacturerData = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ImageSource ManufacturerImage
        {
            get => _manufacturerImage;
            set
            {
                if (_manufacturerImage != value)
                {
                    _manufacturerImage = value;
                    RaisePropertyChanged();
                }
            }
        }


        public string Company
        {
            get => _company;
            set
            {
                if (_company != value)
                {
                    _company = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }


        public IScanResult ScanResult => _scanResult;
    }
}
