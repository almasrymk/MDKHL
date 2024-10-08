using SmartParking.Entities.Subscriptions;
using SmartParking.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Subscriptions;

public interface ISubscriptionAppService : ICrudAppService<Subscription,int, PagedAndSortedResultRequestDto, CreateUpdateSubscriptionDto>
{
    /// <summary>
    /// abstract method to create invoice
    /// </summary>
    /// <param name="input">ivoice create dto</param>
    /// <returns>Invoice Dto</returns>
    public Task<SubscriptionDto> CreateAsync(CreateUpdateSubscriptionDto input);

    /// <summary>
    /// abstract method to get specific invoice
    /// </summary>
    /// <param name="input">ivoice create dto</param>
    /// <returns>Invoice Dto</returns>
    public Task<SubscriptionDto> GetAsync(int id);

    /// <summary>
    /// abstract method to get invoices
    /// </summary>
    /// <param name="input"> PagedAndSortedResultRequestDto</param>
    /// <returns>PagedResultDto of Invoice Dto</returns>
    public Task<PagedResultDto<SubscriptionDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    /// <summary>
    /// abstract method to search of invoices
    /// </summary>
    /// <param name="input">specifies max number of invoice and if it sorted or not and search term</param>
    /// <returns>InvoiceDto</returns>
    public Task<PagedResultDto<SubscriptionDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of invoices
    /// </summary>
    /// <param name="input">specifies max number of invoices and if it sorted or not and filter</param>
    /// <returns>InvoiceDto</returns>
    public Task<PagedResultDto<SubscriptionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
}
