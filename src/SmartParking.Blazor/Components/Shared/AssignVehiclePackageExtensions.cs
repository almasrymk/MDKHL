using MudBlazor;
using SmartParking.Blazor.Components.Pages.Customer.CustomerPackage;
using SmartParking.Blazor.Components.Pages.Customer.CustomerUnitPackages;
using SmartParking.Customers;
using SmartParking.Cutomers;
using SmartParking.Vehicles;

namespace SmartParking.Blazor.Components.Shared
{
    public static class AssignVehiclePackageExtensions
    {


        public static async Task AssignPackgeToVehicleCases(int vehicleIdParsed, Guid customerIdParsed, VehicleDto vehicle, CustomerDto customer, (bool VehcileAccess, bool UserAccess) access , IDialogService DialogService)
        {
            switch (access)
            {
                case (true, false):
                    await DialogService.ShowAsync<AssignPackgeDialog>(
                        "view customer package",
                        new DialogParameters
                        {
                            ["Customer"] = customer,
                            ["VehicleId"] = vehicleIdParsed,
                        });
                    break;

                case (false, true):
                    await DialogService.ShowAsync<AssignVehicleToPackageDialog>(
                        "view customer package",
                        new DialogParameters
                        {
                            ["Vehicle"] = vehicle,
                            ["CustomerId"] = customerIdParsed,
                        });
                    break;

                default:
                    await DialogService.ShowAsync<AssignPackgeDialog>(
                                  "view customer package",
                                  new DialogParameters
                                  {
                                      ["Customer"] = customer,
                                      ["VehicleId"] = vehicleIdParsed,
                                  });
                    break;
            }
        }

    }
}
