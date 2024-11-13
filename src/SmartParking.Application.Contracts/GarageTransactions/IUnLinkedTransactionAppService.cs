using SmartParking.GarageTransactionEndPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactions;
public interface IUnLinkedTransactionAppService:
    ICrudAppService
    <UnLinkedTransactionDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateGarageTransactionEndPointDto
        >
{

    /// <summary>
    /// Retrieves a paged list of unlinked transactions based on the specified paging and sorting parameters.
    /// </summary>
    /// <param name="input">The <see cref="PagedAndSortedResultRequestDto"/> containing pagination and sorting information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PagedResultDto{UnLinkedTransactionDto}"/> with the paged list of unlinked transactions.</returns>
    Task<PagedResultDto<UnLinkedTransactionDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Retrieves an unlinked transaction asynchronously by its unique identifier.
    /// </summary>
    /// <param name="unLinkedTransactionId">The unique identifier of the unlinked transaction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="UnLinkedTransactionDto"/> representing the unlinked transaction.</returns>
    Task<UnLinkedTransactionDto> GetAsync(int id);

    /// <summary>
    /// Searches for MDKHL transactions based on the specified paging, sorting, and search criteria.
    /// </summary>
    /// <param name="input">The <see cref="PagedAndSortedAndSearchResultRequestDto"/> that contains paging, sorting, and search parameters.</param>
    /// <returns>
    /// A <see cref="Task{PagedResultDto{UnLinkedTransactionDto}}"/> representing the asynchronous operation, 
    /// which includes a paged result of <see cref="UnLinkedTransactionDto"/> representing unlinked transactions that match the search criteria.
    /// </returns>
    Task<PagedResultDto<UnLinkedTransactionDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Filters for MDKHL the garage transactions based on the provided criteria.
    /// </summary>
    /// <param name="input">
    /// The <see cref="PagedAndSortedAndFilteredResultRequestDto"/> object containing paging, sorting, and filtering parameters
    /// for the garage transactions.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="PagedResultDto{GarageTransactionDto}"/> 
    /// with the filtered list of garage transactions.
    /// </returns>
    Task<PagedResultDto<UnLinkedTransactionDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input);
}
