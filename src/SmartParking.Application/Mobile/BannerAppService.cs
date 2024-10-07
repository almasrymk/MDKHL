using SmartParking.Entities.Banners;
using SmartParking.Entities.Invoices;
using SmartParking.Invoices;
using SmartParking.Mobile.Banners;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Mobile;

public class BannerAppService : ApplicationService, IBannerAppService
{
    private readonly IRepository<Banner, int> _bannerRepository;
    public BannerAppService(IRepository<Banner,int> bannerRepository)
    {
        _bannerRepository = bannerRepository;
    }
    public async Task<Result<BannerDto>> CreateAsync(CreateUpdateBannerDto input)
    {
        try
        {
            // Check if the user is authenticated
            //if (!_currentUser.IsInRole("admin"))
            //{
            //    return Result<VehicleDto>.Error("User is not authenticated");
            //}

            if (!string.IsNullOrWhiteSpace(input.ImageUrl) && !Uri.IsWellFormedUriString(input.ImageUrl, UriKind.Absolute))
            {
                return Result<BannerDto>.Error("Invalid image URL.");
            }
            var mappedBanner = ObjectMapper.Map<CreateUpdateBannerDto, Banner>(input);

            var banner = await _bannerRepository.InsertAsync(mappedBanner, autoSave: true);

            var bannerDto = ObjectMapper.Map<Banner, BannerDto>(banner);

            return Result<BannerDto>.Success(bannerDto, "Banner Add Succesuffuly");
        }
        catch (Exception ex)
        {
            return Result<BannerDto>.Error(ex.Message);
        }
    }

    public async Task<Result<List<BannerDto>>> GetListAsync()
    {
        var banner = await _bannerRepository.GetListAsync();
        
        // Map the Banner entities to BannerDto objects
        var bannersDto = ObjectMapper.Map<List<Banner>, List<BannerDto>>(banner);
        // Return the paginated result
        return new Result<List<BannerDto>>(
           new List<BannerDto>
           (bannersDto)
       );
    }
}
