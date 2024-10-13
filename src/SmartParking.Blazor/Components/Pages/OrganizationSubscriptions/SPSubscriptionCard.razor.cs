using Microsoft.AspNetCore.Components;
using SmartParking.Subscriptions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
namespace SmartParking.Blazor.Components.Pages.OrganizationSubscriptions;

public partial class SPSubscriptionCard
{
    [Parameter] public PagedResultDto<SubscriptionDto> subscriptions { get; set; } = new PagedResultDto<SubscriptionDto>();


    protected override async Task OnInitializedAsync()
    {
        var input = new PagedAndSortedResultRequestDto();
      
        var result = await SubscriptionsAppService.GetListAsync(input);

        subscriptions = result;


    }

}
