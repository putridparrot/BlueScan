using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace BlueScan.Services
{
    public class Options : IOptions
    {
        public Options()
        {
            Timeout = Observable.Create<double>(o =>
            {
                o.OnNext(60 * 10); // 10 mins
                //o.OnNext(30.0);
                return Disposable.Empty;
            });
        }

        public IObservable<double> Timeout { get; }
    }
}