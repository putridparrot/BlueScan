using System;
using System.Collections.Generic;
using System.Text;

namespace Bluetooth.Service
{
    public enum Proximity
    {
        Undefined,
        Near,
        Medium,
        Far
    }

    public static class ProximityRange
    {
        // https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/ble-beacon#gauging-distance
        // https://stackoverflow.com/questions/22784516/estimating-beacon-proximity-distance-based-on-rssi-bluetooth-le
        // https://www.semanticscholar.org/paper/Evaluation-of-the-reliability-of-RSSI-for-indoor-Dong-Dargie/65228221cfa4fa93654b2b24aa7b41f4d04c82d0
        // https://appelsiini.net/2017/trilateration-with-n-points/
        // https://github.com/ARMmbed/ble/issues/30
        /*
           RSSI = -10 n log d + A
         
           where
            d = distance
            A = txPower
            n = signal propagation constant
            RSSI = dBm

            n can be set to 2 in free spaced but the constant may vary depending on physical constraints
            etc. i.e. walls.

            txPower Radio transmit power in dBm (accepted values are -40, -30, -20, -16, -12, -8, -4, 0, and 4 dBm).
         */

        public static double GetProximity(int rssi, int txPower, int signalPropagationConstant = 2)
        {
            // currently getting a txPower of zero, so this is not much use
            // result is in metres
            return Math.Pow(10d, ((double) txPower - rssi) / (10 * signalPropagationConstant));
        }

        public static Proximity GetApproximateProximity(int rssi)
        {
            if (rssi < 0 && rssi >= -50)
                return Proximity.Near;

            if (rssi < -50 && rssi >= -90)
                return Proximity.Medium;

            if (rssi < -90)
                return Proximity.Far;

            return Proximity.Undefined;
        }
    }
}
