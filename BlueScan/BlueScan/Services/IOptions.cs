using System;
using System.Collections.Generic;
using System.Text;

namespace BlueScan.Services
{
    public interface IOptions
    {
        IObservable<double> Timeout { get; }
    }
}
