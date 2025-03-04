using AutoMapper.Internal.Mappers;
using SmartParking.Entities.Discounts;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Discounts;

public abstract class DiscountAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDiscountAppService _discountAppService;

    public DiscountAppService_Tests()
    {
        _discountAppService = GetRequiredService<IDiscountAppService>();
    }
    [Fact]
    public async Task CreateDiscount()
    {

    }

}
