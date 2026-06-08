using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Localization;
using SmartParking.PDF.Dtos;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.PDF.Contracts;

public interface ILocalizationFactoryAppService : IApplicationService
{
    InvoiceLocalizationData Create(InvoiceLocalizationCreateDto dto);
    PackageLocalizationData CreatePackageLocalization(PackageLocalizationCreateDto dto);
    UnitPackageLocalizationData CreateUnitPackageLocalization(UnitPackageLocalizationCreateDto dto);
}