using System;
using System.Collections.Generic;
using System.Text;
using Plugin.BluetoothLE;

namespace Bluetooth.Service
{
    public interface IBluetoothScanner
    {
        IObservable<IScanResult> Scan();
        IObservable<IScanResult> Scan(IList<Guid> guids);

        void StopScan();

        IObservable<AdapterStatus> ObserveAdapterStatus();

        void SetAdapterStatus(bool enable);

        bool CanSetAdapterState();
    }
}
