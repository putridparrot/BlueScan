using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms;

namespace BlueScan.Extensions
{
    public static class ManufacturerImages
    {
        private static readonly Dictionary<string, ImageSource> Images;

        static ManufacturerImages()
        {
            Images = new Dictionary<string, ImageSource>
            {
                {
                    "Bluetooth",
                    ImageSource.FromResource("BlueScan.Images.Bluetooth24.png", typeof(ImageResourceExtension).GetTypeInfo().Assembly)
                },
                {
                    CompanyIdentifiers.GetDisplayName(0x0006),
                    ImageSource.FromResource("BlueScan.Images.Windows24.png", typeof(ImageResourceExtension).GetTypeInfo().Assembly)
                },
                {
                    CompanyIdentifiers.GetDisplayName(0x004C),
                    ImageSource.FromResource("BlueScan.Images.Apple24.png", typeof(ImageResourceExtension).GetTypeInfo().Assembly)
                }
            };
        }

        public static ImageSource Get(string companyName)
        {
            return !String.IsNullOrEmpty(companyName) && Images.TryGetValue(companyName, out ImageSource image) ?
                image :
                Images["Bluetooth"];
        }
    }
}
