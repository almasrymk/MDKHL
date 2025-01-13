using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.Localization;


namespace SmartParking.StringLocalizer;

public static class BreadCrumbsRoutes
{
    public static string GetCueerntURLRoute(string CurrentURI )
    {
        return CurrentURI switch
        {
            "dashboard" => LocalizationHelper.GetLocalizedString("Dashboard"),
            "organizations" => LocalizationHelper.GetLocalizedString("Organizations"),
            "garages" => LocalizationHelper.GetLocalizedString("Garages"),
            "cameras" => LocalizationHelper.GetLocalizedString("Cameras"),
            "cities" => LocalizationHelper.GetLocalizedString("Cities"),
            "contact-data" => LocalizationHelper.GetLocalizedString("Contact Data"),
            "countries" => LocalizationHelper.GetLocalizedString("Countries"),
            "customers" => LocalizationHelper.GetLocalizedString("Customers"),
            "garageTransactions" => LocalizationHelper.GetLocalizedString("GarageTransactions"),
            "invoices" => LocalizationHelper.GetLocalizedString("Invoices"),
            "licencetypes" => LocalizationHelper.GetLocalizedString("LicenceTypes"),
            "subscription" => LocalizationHelper.GetLocalizedString("Organization Subscription"),
            "package" => LocalizationHelper.GetLocalizedString("Packages"),
            "packagetypes" => LocalizationHelper.GetLocalizedString("PackageTypes"),
            "platecolors" => LocalizationHelper.GetLocalizedString("PlateColors"),
            "regions" => LocalizationHelper.GetLocalizedString("Regions"),
            "states" => LocalizationHelper.GetLocalizedString("States"),
            "subscriptions" => LocalizationHelper.GetLocalizedString("Subscriptions"),
            "subscriptiontypes" => LocalizationHelper.GetLocalizedString("SubscriptionType"),
            "vehiclebrands" => LocalizationHelper.GetLocalizedString("VehicleBrands"),
            "vehiclemodels" => LocalizationHelper.GetLocalizedString("VehicleModel"),
            "profile" => LocalizationHelper.GetLocalizedString("Profile"),
            "customers/register" => LocalizationHelper.GetLocalizedString("Customers") + " / " + LocalizationHelper.GetLocalizedString("Add NEW"),
            "customers/view" => LocalizationHelper.GetLocalizedString("Customers") + " / " + LocalizationHelper.GetLocalizedString("Details"),
            "customers/edit" => LocalizationHelper.GetLocalizedString("Customers") + " / " + LocalizationHelper.GetLocalizedString("Edit"),
            "Camerabrands" => LocalizationHelper.GetLocalizedString("CameraBrands"),
            "Cameramaintypes" => LocalizationHelper.GetLocalizedString("Cameramaintypes"),
            "Cameramodels" => LocalizationHelper.GetLocalizedString("Cameramodels"),
            "invoices/add" => LocalizationHelper.GetLocalizedString("Invoices") + " / " + LocalizationHelper.GetLocalizedString("Add NEW"),
            "invoices/details" => LocalizationHelper.GetLocalizedString("Invoices") + " / " + LocalizationHelper.GetLocalizedString("Details"),
            "edit-my-account" => LocalizationHelper.GetLocalizedString("Organization") + " / " + LocalizationHelper.GetLocalizedString("Edit"),
            "view-my-account" => LocalizationHelper.GetLocalizedString("Organization") + " / "+LocalizationHelper.GetLocalizedString("Details"),
            "users" => LocalizationHelper.GetLocalizedString("Users") ,
            "payments" => LocalizationHelper.GetLocalizedString("Payments") ,
            "unLinkedTransaction" => LocalizationHelper.GetLocalizedString("UnLinkedTransaction") ,
            "plateColors" => LocalizationHelper.GetLocalizedString("PlateColors") ,
            "licenceTypes" => LocalizationHelper.GetLocalizedString("LicenceTypes") ,
            "vehicleBrands" => LocalizationHelper.GetLocalizedString("VehicleBrands") ,
            "vehicleModels" => LocalizationHelper.GetLocalizedString("VehicleModels") ,
            "CameraModels" => LocalizationHelper.GetLocalizedString("Cameramodels") ,
            "edit-profile" => LocalizationHelper.GetLocalizedString("Edit Profile") ,
            "roles/mdkhl" => LocalizationHelper.GetLocalizedString("Roles") ,
            "add-user" => LocalizationHelper.GetLocalizedString("Users") +" / "+ LocalizationHelper.GetLocalizedString("Add NEW"),
            "roles" => LocalizationHelper.GetLocalizedString("Roles"),
            _ => LocalizationHelper.GetLocalizedString("Dashboard") // Default title if no match found
        };
    }
}
