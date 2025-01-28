using SmartParking.Cameras;
using SmartParking.Garages;
using SmartParking.Lookups.PlateColor;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactions
{
    public interface IGarageTransactionsAppService :
	 ICrudAppService<
		GarageTransactionDto,
		int,
		PagedAndSortedResultRequestDto,
		CreateUpdateGarageTransactionDto
		>
	{
		public Task<PagedResultDto<GarageTransactionDto>> Search(PagedAndSortedAndSearchResultRequestDto input);
		Task<PagedResultDto<GarageTransactionDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input);

		/// <summary>
		/// abstract method to filter of cameras
		/// </summary>
		/// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
		/// <returns>Camera Dto</returns>
		public Task<PagedResultDto<GarageTransactionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
		Task<PagedResultDto<GarageTransactionDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input);
		public Task<GarageTransactionDto> CreateAsync(CreateUpdateGarageTransactionDto input);
        public Task<GarageTransactionDto> GetAsync(int id);


        public Task DeleteAsync(int id);

        /// <summary>
        /// Method used to get all garage transactions
        /// </summary>
        /// <param name="input">Input used for pagination</param>
        /// <returns>Paged Result of garage transactions</returns>
        Task<PagedResultDto<GarageTransactionDto>> GetAllListAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

		/// <summary>
		/// Method used to get all garage transactions by organization
		/// </summary>
		/// <param name="input">Input used for pagination</param>
		/// <param name="tenantId">id of tenant</param>
		/// <returns>Paged Result of garage transactions</returns>
		Task<PagedResultDto<GarageTransactionDto>> GetAllTransactionsByOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

		Task<Result<List<GarageTransactionDto>>> GetCustomerTransactionsInOrganization(int organizationId);
		string SaveBase64Image(CreateUpdateGarageTransactionDto input);

		Task<int> GetCameraIdByGarageTransactionId(int garageTransactionId);

		
		


    }
}
