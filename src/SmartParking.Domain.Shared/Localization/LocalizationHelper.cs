using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Localization;

public static class LocalizationHelper
{
    private static IServiceProvider _serviceProvider;

    public static void Configure(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static string GetLocalizedString(string key)
    {
        var localizer = _serviceProvider.GetRequiredService<IStringLocalizer<SmartParkingResource>>();
        return localizer[key];
    }
}
