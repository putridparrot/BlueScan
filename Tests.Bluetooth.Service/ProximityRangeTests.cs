using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bluetooth.Service;
using NUnit.Framework;

namespace Tests.Bluetooth.Service
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ProximityRangeTests
    {
        [TestCase(-60, 0, 1000.0d)] // this is meaningless with a 0 txPower
        [TestCase(-60, -69, 0.35d)]
        [TestCase(-69, -69, 1.0d)]
        [TestCase(-80, -69, 3.54d)]
        public void GetProximity_WithKnownValues(int rssi, int txPower, double expected)
        {
            Assert.AreEqual(expected, ProximityRange.GetProximity(rssi, txPower), 0.01);
        }
    }
}
