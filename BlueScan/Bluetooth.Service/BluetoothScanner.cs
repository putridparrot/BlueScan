using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Plugin.BluetoothLE;

namespace Bluetooth.Service
{
    public class BluetoothScanner : IBluetoothScanner
    {
        public IObservable<IScanResult> Scan()
        {
            if (CrossBleAdapter.Current.IsScanning)
            {
                CrossBleAdapter.Current.StopScan();
            }

            return CrossBleAdapter.Current.Scan();
        }

        public IObservable<IScanResult> Scan(IList<Guid> guids)
        {
            if (guids == null)
            {
                return Scan();
            }

            if (CrossBleAdapter.Current.IsScanning)
            {
                CrossBleAdapter.Current.StopScan();
            }

            return CrossBleAdapter.Current.Scan(new ScanConfig
            {
                ServiceUuids = new List<Guid>(guids)
            });
        }

        public void StopScan()
        {
            if (CrossBleAdapter.Current.IsScanning)
            {
                CrossBleAdapter.Current.StopScan();
            }
        }

        public IObservable<AdapterStatus> ObserveAdapterStatus()
        {
            return CrossBleAdapter.Current
                .WhenStatusChanged()
                .StartWith(CrossBleAdapter.Current.Status);
        }

        public void SetAdapterStatus(bool enable)
        {
            CrossBleAdapter.Current.SetAdapterState(enable);
        }

        public bool CanSetAdapterState()
        {
            return CrossBleAdapter.Current.CanControlAdapterState();
        }
    }
}