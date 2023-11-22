using System;

namespace BlueScan.Services
{
    public interface IOptions
    {
        IObservable<double> Timeout { get; }
    }
}
