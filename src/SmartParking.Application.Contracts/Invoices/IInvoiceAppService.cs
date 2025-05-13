using SmartParking.Entities.Invoices;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Invoices;
public interface IInvoiceAppService : ICrudAppService<InvoiceDto, int, PagedAndSortedResultRequestDto, CreateUpdateInvoiceDto>
{
    /// <summary>
    /// abstract method to create invoice
    /// </summary>
    /// <param name="input">ivoice create dto</param>
    /// <returns>Invoice Dto</returns>
    public Task<InvoiceDto> CreateAsync(CreateUpdateInvoiceDto input);

    /// <summary>
    /// abstract method to get specific invoice
    /// </summary>
    /// <param name="id">ivoice id</param>
    /// <returns>Invoice Dto</returns>
    public Task<InvoiceDto> GetAsync(int id);

    /// <summary>
    /// abstract method to get invoices
    /// </summary>
    /// <param name="input"> PagedAndSortedResultRequestDto</param>
    /// <returns>PagedResultDto of Invoice Dto</returns>
    public Task<PagedResultDto<InvoiceDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    /// <summary>
    /// abstract method to search of invoices
    /// </summary>
    /// <param name="input">specifies max number of invoice and if it sorted or not and search term</param>
    /// <returns>InvoiceDto</returns>
    public Task<PagedResultDto<InvoiceDto>> Search(PagedAndSortedAndSearchResultRequestDto input);
    public Task<PagedResultDto<InvoiceDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of invoices
    /// </summary>
    /// <param name="input">specifies max number of invoices and if it sorted or not and filter</param>
    /// <returns>InvoiceDto</returns>
    public Task<PagedResultDto<InvoiceDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    public  Task DeleteAsync(int id);
    Task<Result<string>> GetInvoiceQRCodeForOrganization(int invoiceId);
    Task<Result<string>> GetInvoiceQRCodeForMDKHL(int invoiceId);

	/// <summary>
	/// abstract method to get invoices by organization
	/// </summary>
	/// <param name="input"> PagedAndSortedResultRequestDto</param>
	/// <param name="tenantId"> id of tenant</param>
	/// <returns>PagedResultDto of Invoice Dto</returns>
    Task<PagedResultDto<InvoiceDto>> GetAllInvoicesByOrganizationAsync(PagedAndSortedResultRequestDto input,Guid? tenantId);

    Task<Result<InvoiceDto>> GetLatestInvoiceForPlateInOrganizationGarage(string PlateNumber,int OrganizationId,int GarageId);

    Task<PagedResultDto<InvoiceDto>> GetInvoiceForCustomer(Guid customerId, PagedResultRequestDto input);
    Task<Result<List<InvoiceDto>>> GetUnpaidInvoiceForKeeperInHisOrganization(int OrganizationId,string plateNumber);
    Task<Result<string>> GetImage(InvoiceDto? invoice, string? qrCodedto);

    Task<InvoiceDto?> GetLatestInvoiceForOrganization();

    Task UpdateDiscountForInvoice(int invoiceId, decimal discount);


}
