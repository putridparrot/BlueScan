using System;

namespace BlueScan.Extensions
{
    public static class AddressExtensions
    {
        public static string ToBdAddress(Guid uuid)
        {
            var id = uuid.ToString();
            var lastIndex = id.LastIndexOf("-");

            return ToBdAddress(id.Substring(lastIndex + 1));
        }

        public static string ToBdAddress(string address)
        {
            return $"{address.Substring(0, 2)}:{address.Substring(2, 2)}:{address.Substring(4, 2)}:{address.Substring(6, 2)}:{address.Substring(8, 2)}:{address.Substring(10, 2)}".ToUpperInvariant();
        }
    }
}
