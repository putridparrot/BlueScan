using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueScan.Services
{
    public interface IPersist
    {
        Task SaveAsync(IEnumerable<ScanResultDataItem> data);
        Task SynchronizeAsync();
        Task ClearLocalAsync();
    }
}
