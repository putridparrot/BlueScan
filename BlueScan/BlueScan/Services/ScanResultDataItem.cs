using System;
using BlueScan.Extensions;
using Plugin.BluetoothLE;
using SQLite;

namespace BlueScan.Services
{
    public class ScanResultDataItem
    {
        public ScanResultDataItem()
        {
        }

        public ScanResultDataItem(IScanResult scanResult)
        {
            if(scanResult == null)
                throw new ArgumentNullException(nameof(scanResult));

            if (scanResult.Device != null)
            {
                Name = scanResult.Device.Name ?? "N/A";
                BdAddress = AddressExtensions.ToBdAddress(scanResult.Device.Uuid);
            }
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public string BdAddress { get; set; }
    }
}
