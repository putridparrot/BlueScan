using BlueScan.Common;
using Bluetooth.Service;
using Prism.Mvvm;
using SimpleCharts;
using SkiaSharp;

namespace BlueScan.ViewModels
{
    public class ProximityViewModel : BindableBase
    {
        private int _rssi;
        private int _runningTotal;
        private int _samples;
        private int _txPower;
        private string _approxDistance;

        public ProximityViewModel(int rssi)
        {
            Entries = new FixedSizeObservableCollection<Entry>(10);
            Current = rssi;
        }

        public FixedSizeObservableCollection<Entry> Entries { get; }

        public int Current
        {
            get => _rssi;
            set
            {
                if (_rssi != value)
                {
                    _rssi = value;
                    RaisePropertyChanged();
                }

                Entries.Add(new Entry(_rssi)
                {
                    Label = $"{_rssi} dbm",
                    Color = SKColors.Red
                });

                _samples++;
                _runningTotal += _rssi;

                ApproxDistance = _txPower != 0
                    ? ProximityRange.GetProximity(_rssi, _txPower).ToString("N2")
                    : ProximityRange.GetApproximateProximity(_rssi).ToString();

                RaisePropertyChanged(nameof(Average));
            }
        }

        public float Average => _runningTotal / _samples;

        public int TxPower
        {
            get => _txPower;
            set
            {
                if (_txPower != value)
                {
                    _txPower = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ApproxDistance
        {
            get => _approxDistance;
            set
            {
                if (_approxDistance != value)
                {
                    _approxDistance = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}