using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class changeforcedelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CameraMainTypes_CameraBrands_CameraBrandId",
                table: "CameraMainTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_CameraModels_CameraBrands_CameraBrandId",
                table: "CameraModels");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_State_StateId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactInfos_Organizations_OrganizationId",
                table: "ContactInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Customers_CustomerId",
                table: "CustomerPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Organizations_OrganizationId",
                table: "CustomerPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_GarageCategories_Garages_GarageId",
                table: "GarageCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Garages_Organizations_OrganizationId",
                table: "Garages");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_LicenceTypes_PlateColors_PlateColorId",
                table: "LicenceTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Cities_CityId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Countries_CountryId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Regions_RegionId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_State_StateId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Subscriptions_SubscriptionId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Organizations_OrganizationId",
                table: "Packages");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_PackageTypes_PackageTypeId",
                table: "Packages");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Countries_CountryId",
                table: "Regions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_SubscriptionTypes_SubscriptionTypeId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehiclesBrandId",
                table: "VehicleModels");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_LicenceTypes_LicenceTypeId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_PlateColors_PlateColorId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleBrands_VehicleBrandId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleModels_VehicleModelId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IdentityUserId",
                table: "Customers");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Customers_IdentityUserId",
            //    table: "Customers",
            //    column: "IdentityUserId",
            //    unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CameraMainTypes_CameraBrands_CameraBrandId",
                table: "CameraMainTypes",
                column: "CameraBrandId",
                principalTable: "CameraBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CameraModels_CameraBrands_CameraBrandId",
                table: "CameraModels",
                column: "CameraBrandId",
                principalTable: "CameraBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras",
                column: "BrandNameId",
                principalTable: "CameraBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras",
                column: "MainTypeId",
                principalTable: "CameraMainTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras",
                column: "ModelId",
                principalTable: "CameraModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_State_StateId",
                table: "Cities",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInfos_Organizations_OrganizationId",
                table: "ContactInfos",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Customers_CustomerId",
                table: "CustomerPackages",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Organizations_OrganizationId",
                table: "CustomerPackages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers",
                column: "IdentityUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GarageCategories_Garages_GarageId",
                table: "GarageCategories",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Garages_Organizations_OrganizationId",
                table: "Garages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LicenceTypes_PlateColors_PlateColorId",
                table: "LicenceTypes",
                column: "PlateColorId",
                principalTable: "PlateColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Cities_CityId",
                table: "Organizations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Countries_CountryId",
                table: "Organizations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Regions_RegionId",
                table: "Organizations",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_State_StateId",
                table: "Organizations",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Subscriptions_SubscriptionId",
                table: "Organizations",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Organizations_OrganizationId",
                table: "Packages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_PackageTypes_PackageTypeId",
                table: "Packages",
                column: "PackageTypeId",
                principalTable: "PackageTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Countries_CountryId",
                table: "Regions",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_SubscriptionTypes_SubscriptionTypeId",
                table: "Subscriptions",
                column: "SubscriptionTypeId",
                principalTable: "SubscriptionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehiclesBrandId",
                table: "VehicleModels",
                column: "VehiclesBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_LicenceTypes_LicenceTypeId",
                table: "Vehicles",
                column: "LicenceTypeId",
                principalTable: "LicenceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_PlateColors_PlateColorId",
                table: "Vehicles",
                column: "PlateColorId",
                principalTable: "PlateColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleBrands_VehicleBrandId",
                table: "Vehicles",
                column: "VehicleBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleModels_VehicleModelId",
                table: "Vehicles",
                column: "VehicleModelId",
                principalTable: "VehicleModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CameraMainTypes_CameraBrands_CameraBrandId",
                table: "CameraMainTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_CameraModels_CameraBrands_CameraBrandId",
                table: "CameraModels");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_State_StateId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactInfos_Organizations_OrganizationId",
                table: "ContactInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Customers_CustomerId",
                table: "CustomerPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Organizations_OrganizationId",
                table: "CustomerPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_GarageCategories_Garages_GarageId",
                table: "GarageCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Garages_Organizations_OrganizationId",
                table: "Garages");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_LicenceTypes_PlateColors_PlateColorId",
                table: "LicenceTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Cities_CityId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Countries_CountryId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Regions_RegionId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_State_StateId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Subscriptions_SubscriptionId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Organizations_OrganizationId",
                table: "Packages");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_PackageTypes_PackageTypeId",
                table: "Packages");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Countries_CountryId",
                table: "Regions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_SubscriptionTypes_SubscriptionTypeId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehiclesBrandId",
                table: "VehicleModels");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_LicenceTypes_LicenceTypeId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_PlateColors_PlateColorId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleBrands_VehicleBrandId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleModels_VehicleModelId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IdentityUserId",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IdentityUserId",
                table: "Customers",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CameraMainTypes_CameraBrands_CameraBrandId",
                table: "CameraMainTypes",
                column: "CameraBrandId",
                principalTable: "CameraBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CameraModels_CameraBrands_CameraBrandId",
                table: "CameraModels",
                column: "CameraBrandId",
                principalTable: "CameraBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras",
                column: "BrandNameId",
                principalTable: "CameraBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras",
                column: "MainTypeId",
                principalTable: "CameraMainTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras",
                column: "ModelId",
                principalTable: "CameraModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_State_StateId",
                table: "Cities",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInfos_Organizations_OrganizationId",
                table: "ContactInfos",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Customers_CustomerId",
                table: "CustomerPackages",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Organizations_OrganizationId",
                table: "CustomerPackages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers",
                column: "IdentityUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GarageCategories_Garages_GarageId",
                table: "GarageCategories",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Garages_Organizations_OrganizationId",
                table: "Garages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LicenceTypes_PlateColors_PlateColorId",
                table: "LicenceTypes",
                column: "PlateColorId",
                principalTable: "PlateColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Cities_CityId",
                table: "Organizations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Countries_CountryId",
                table: "Organizations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Regions_RegionId",
                table: "Organizations",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_State_StateId",
                table: "Organizations",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Subscriptions_SubscriptionId",
                table: "Organizations",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Organizations_OrganizationId",
                table: "Packages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_PackageTypes_PackageTypeId",
                table: "Packages",
                column: "PackageTypeId",
                principalTable: "PackageTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Countries_CountryId",
                table: "Regions",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_SubscriptionTypes_SubscriptionTypeId",
                table: "Subscriptions",
                column: "SubscriptionTypeId",
                principalTable: "SubscriptionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleBrands_VehiclesBrandId",
                table: "VehicleModels",
                column: "VehiclesBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_LicenceTypes_LicenceTypeId",
                table: "Vehicles",
                column: "LicenceTypeId",
                principalTable: "LicenceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_PlateColors_PlateColorId",
                table: "Vehicles",
                column: "PlateColorId",
                principalTable: "PlateColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleBrands_VehicleBrandId",
                table: "Vehicles",
                column: "VehicleBrandId",
                principalTable: "VehicleBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleModels_VehicleModelId",
                table: "Vehicles",
                column: "VehicleModelId",
                principalTable: "VehicleModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
