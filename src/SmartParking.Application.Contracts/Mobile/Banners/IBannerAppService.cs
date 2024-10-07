using SmartParking.Invoices;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Banners;

public interface IBannerAppService : IApplicationService
{
    Task<Result<BannerDto>> CreateAsync(CreateUpdateBannerDto input);
    Task<Result<List<BannerDto>>> GetListAsync();
}
