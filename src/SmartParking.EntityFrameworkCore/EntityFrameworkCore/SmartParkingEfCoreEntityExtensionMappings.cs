using Microsoft.EntityFrameworkCore;
using System;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace SmartParking.EntityFrameworkCore;

public static class SmartParkingEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        SmartParkingGlobalFeatureConfigurator.Configure();
        SmartParkingModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() =>
        {
            /* You can configure extra properties for the
             * entities defined in the modules used by your application.
             *
             * This class can be used to map these extra properties to table fields in the database.
             *
             * USE THIS CLASS ONLY TO CONFIGURE EF CORE RELATED MAPPING.
             * USE SmartParkingModuleExtensionConfigurator CLASS (in the Domain.Shared project)
             * FOR A HIGH LEVEL API TO DEFINE EXTRA PROPERTIES TO ENTITIES OF THE USED MODULES
             *
             * Example: Map a property to a table field:

                 ObjectExtensionManager.Instance
                     .MapEfCoreProperty<IdentityUser, string>(
                         "MyProperty",
                         (entityBuilder, propertyBuilder) =>
                         {
                             propertyBuilder.HasMaxLength(128);
                         }
                     );

             * See the documentation for more:
             * https://docs.abp.io/en/abp/latest/Customizing-Application-Modules-Extending-Entities
             */
            ObjectExtensionManager.Instance
                .MapEfCoreProperty<IdentityUser, int>(
                    "UserTypeId",
                    (entityBuilder, propertyBuilder) => { propertyBuilder.HasMaxLength(500000); }
                );
            ObjectExtensionManager.Instance
                .MapEfCoreProperty<IdentityUser, int>(
                    "OrganizationId",
                    (entityBuilder, propertyBuilder) => { propertyBuilder.HasMaxLength(500000); }
                ); 
            ObjectExtensionManager.Instance
                .MapEfCoreProperty<IdentityUser, string>(
                    "WhatsAppNum",
                    (entityBuilder, propertyBuilder) => { propertyBuilder.IsRequired(); }
                );
            /* See the documentation for more:
             * https://docs.abp.io/en/abp/latest/Customizing-Application-Modules-Extending-Entities
             */

            ObjectExtensionManager.Instance
            .MapEfCoreProperty<IdentityUser, string>(
                "OtpCode",
                (entityBuilder, propertyBuilder) =>
                {
                    propertyBuilder.HasMaxLength(4);
                }
                );

            ObjectExtensionManager.Instance
           .MapEfCoreProperty<IdentityUser, DateTime?>(
               "OtpExpirationDate",
               (entityBuilder, propertyBuilder) =>
               {
                   propertyBuilder.HasDefaultValue(null);
               }
               );
        });
    }
}
