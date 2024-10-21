using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Mobile.Banners;
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/mobile/Banner")]
[ApiController]
public class BannerController : AbpController
{
    private readonly IBannerAppService _bannerAppService;
    public BannerController(IBannerAppService bannerAppService)
    {
        _bannerAppService = bannerAppService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Result<BannerDto>> CreateBanner(CreateUpdateBannerDto input)
    {
        return await _bannerAppService.CreateAsync(input);
    }
    [HttpGet]
    [Authorize]
    public async Task<Result<List<BannerDto>>> GetAllBanners()
    {
        return await _bannerAppService.GetListAsync();
    }
}
