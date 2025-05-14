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
    /// <summary>
    /// Asynchronously creates a new banner based on the provided input data.
    /// </summary>
    /// <param name="input">An object containing the data to create a new banner.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Result{BannerDto}"/> object with the created banner's details or an error message.</returns>
    Task<Result<BannerDto>> CreateAsync(CreateUpdateBannerDto input);

    /// <summary>
    /// Asynchronously retrieves a list of banners.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Result{List{BannerDto}}"/> object with the list of banners.</returns>
    Task<Result<List<BannerDto>>> GetListAsync();

}
