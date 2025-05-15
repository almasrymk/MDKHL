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
    /// <summary>
    /// Asynchronously deletes an entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(int id);

    /// <summary>
    /// Asynchronously retrieves the QR code associated with the invoice for the specified organization.
    /// </summary>
    /// <param name="invoiceId">The identifier of the invoice for which the QR code is being fetched.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result{T}"/> object with the QR code string, or an error message.</returns>
    Task<Result<string>> GetInvoiceQRCodeForOrganization(int invoiceId);

    /// <summary>
    /// Asynchronously retrieves the QR code associated with the invoice for MDKHL.
    /// </summary>
    /// <param name="invoiceId">The identifier of the invoice for which the QR code is being fetched.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result{T}"/> object with the QR code string, or an error message.</returns>
    Task<Result<string>> GetInvoiceQRCodeForMDKHL(int invoiceId);
    /// <summary>
    /// abstract method to get invoices by organization
    /// </summary>
    /// <param name="input"> PagedAndSortedResultRequestDto</param>
    /// <param name="tenantId"> id of tenant</param>
    /// <returns>PagedResultDto of Invoice Dto</returns>
    Task<PagedResultDto<InvoiceDto>> GetAllInvoicesByOrganizationAsync(PagedAndSortedResultRequestDto input,Guid? tenantId);

    /// <summary>
    /// Asynchronously retrieves the latest invoice for a customer in a specific organization and garage, based on the vehicle's plate number.
    /// </summary>
    /// <param name="PlateNumber">The plate number of the vehicle associated with the customer.</param>
    /// <param name="OrganizationId">The identifier of the organization to filter the invoice by.</param>
    /// <param name="GarageId">The identifier of the garage to filter the invoice by.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Result{InvoiceDto}"/> object with the latest invoice for the specified customer and garage, or an error message.</returns>
    Task<Result<InvoiceDto>> GetLatestInvoiceForCustomerInOrganizationGarage(string PlateNumber, int OrganizationId, int GarageId);

    /// <summary>
    /// Asynchronously retrieves a paged list of invoices for a customer, with pagination applied.
    /// </summary>
    /// <param name="customerId">The identifier of the customer whose invoices are being retrieved.</param>
    /// <param name="input">An object containing pagination details (e.g., page size and skip count).</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="PagedResultDto{InvoiceDto}"/> object with the invoices for the customer.</returns>
    Task<PagedResultDto<InvoiceDto>> GetInvoiceForCustomer(Guid customerId, PagedResultRequestDto input);

    /// <summary>
    /// Asynchronously retrieves the list of unpaid invoices for a keeper within their organization, filtered by plate number.
    /// </summary>
    /// <param name="OrganizationId">The identifier of the organization to filter unpaid invoices by.</param>
    /// <param name="plateNumber">The plate number of the vehicle associated with the unpaid invoices.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result{List{InvoiceDto}}"/> object with the unpaid invoices for the keeper.</returns>
    Task<Result<List<InvoiceDto>>> GetUnpaidInvoiceForKeeperInHisOrganization(int OrganizationId, string plateNumber);

    /// <summary>
    /// Asynchronously retrieves an image associated with an invoice, using the provided QR code DTO.
    /// </summary>
    /// <param name="invoice">The invoice for which the image is being retrieved.</param>
    /// <param name="qrCodedto">A QR code DTO to retrieve the corresponding image.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result{string}"/> object with the image or an error message.</returns>
    Task<Result<string>> GetImage(InvoiceDto? invoice, string? qrCodedto);

    /// <summary>
    /// Asynchronously retrieves the latest invoice for an organization.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the latest <see cref="InvoiceDto"/> for the organization, or <c>null</c> if no invoices exist.</returns>
    Task<InvoiceDto?> GetLatestInvoiceForOrganization();

    /// <summary>
    /// Asynchronously updates the discount for a specified invoice.
    /// </summary>
    /// <param name="invoiceId">The identifier of the invoice to update the discount for.</param>
    /// <param name="discount">The new discount value to apply to the invoice.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateDiscountForInvoice(int invoiceId, decimal discount);


}
