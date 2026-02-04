using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Utilities;

public static class PackageExtension
{
    public static (int PackageDuration ,bool IsDurationDisabled) OnPackageTypeChanged(PackageCategoryTypes value)
    {
        int packageDuration = 0;
        bool IsDurationDisabled=false;
        if (value != PackageCategoryTypes.CustomPackage &&
            value != PackageCategoryTypes.Free &&
            value != PackageCategoryTypes.Daily)
        {
            switch (value)
            {
                case PackageCategoryTypes.Year:
                    packageDuration = 360;
                    break;

                case PackageCategoryTypes.HalfYear:
                    packageDuration = 180;
                    break;

                case PackageCategoryTypes.Monthly:
                    packageDuration = 30;
                    break;

            }

            IsDurationDisabled = true;
        }
        else
        {
            IsDurationDisabled = false;
        }
        return (packageDuration, IsDurationDisabled);

    }


}
