using Abp.UI;
using Microsoft.AspNetCore.Http;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace SmartParking.GarageTransactions;

public class GarageTransactionsAppService:
	 CrudAppService<GarageTransaction, GarageTransactionDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageTransactionDto>, IGarageTransactionsAppService
{
	private readonly IRepository<GarageTransaction, int> _repository;
    private readonly IRepository<Garage, int> _GarageRepository;
    private readonly IRepository<Organization, int> _OrganizationRepository;
    private readonly IRepository<Camera, int> _CameraRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataFilter _dataFilter;
	private readonly ICurrentUser _currentUser;

    public GarageTransactionsAppService(IRepository<GarageTransaction, int> repository, IDataFilter dataFilter, ICurrentUser currentUser, IRepository<Garage, int> garagerepository, IRepository<Organization, int> organizationRepository, IRepository<Camera, int> cameraRepository, IHttpContextAccessor httpContextAccessor) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _GarageRepository = garagerepository;
        _OrganizationRepository = organizationRepository;
        _CameraRepository = cameraRepository;
        _httpContextAccessor = httpContextAccessor;
    }
    public override async Task<PagedResultDto<GarageTransactionDto>> GetListAsync(PagedAndSortedResultRequestDto input)
	{
		if (_currentUser.IsInRole("admin"))
		{
			using (_dataFilter.Disable<IMultiTenant>())
			{
				return await base.GetListAsync(input);
			}
		}
		else
			return await base.GetListAsync(input);
	}
    public async override Task<GarageTransactionDto> GetAsync(int id)
    {

        // Get the current base URL dynamically
        var request = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

        // Add full URL to VehiclePicture field

        if (_currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var garageTransaction = await base.GetAsync(id);
                garageTransaction.VehiclePicture = $"{baseUrl}{garageTransaction.VehiclePicture}";
                return garageTransaction;
            }
        }
        else
        {
            var garageTransaction = await base.GetAsync(id);
            garageTransaction.VehiclePicture = $"{baseUrl}{garageTransaction.VehiclePicture}";
            return garageTransaction;
        }
    }
    public async Task<PagedResultDto<GarageTransactionDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
	{
		// Get the IQueryable<camera> from the repository
		var queryable = await _repository.GetQueryableAsync();
		// Apply search filtering
		if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
		{

			queryable = queryable.Where(or =>
				or.PlateNnumber.StartsWith(input.SearchingTerm) ||
				or.Type.StartsWith(input.SearchingTerm) 
				//or..StartsWith(input.SearchingTerm)

			);
		}
		// Get the total count of filtered records
		var totalCount = await AsyncExecuter.CountAsync(queryable);

		// Apply pagination
		queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
		// Execute the query and get the list of camera entities
		var garageTransactions = await AsyncExecuter.ToListAsync(queryable);

		// Map the hotel entities to cameraDto objects
		var garageTransactionsDto = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(garageTransactions);
		// Return the paginated result
		return new PagedResultDto<GarageTransactionDto>(
			totalCount,
			garageTransactionsDto
		);
	}

	/// <summary>
	/// method used to get list of cameraes
	/// </summary>
	/// <param name="input">specifies max number of camera and if it filtered or not</param>
	/// <returns>cameraeDto List</returns>
	public async Task<PagedResultDto<GarageTransactionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
	{
		// Get the IQueryable<camera> from the repository
		var queryable = await _repository.GetQueryableAsync();
		// Apply filtering
		var filterExpression = BuildExpression<Camera>.BuildFilterExpression(input.filters);
		if (filterExpression is not null)
		{
			queryable = queryable.Where(filterExpression);
		}
		// Get the total count of filtered records
		var totalCount = await AsyncExecuter.CountAsync(queryable);

		// Apply pagination
		queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
		// Execute the query and get the list of organizations entities
		var garageTransactions = await AsyncExecuter.ToListAsync(queryable);

		// Map the organizations entities to organizationsDto objects
		var garageTransactionsDto = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(garageTransactions);
		// Return the paginated result
		return new PagedResultDto<GarageTransactionDto>(
			totalCount,
			garageTransactionsDto
		);
	}

    public override Task<GarageTransactionDto> CreateAsync(CreateUpdateGarageTransactionDto input)
    {
      
           input.VehiclePicture = SaveBase64Image(input);
        if (input.VehiclePicture == null)
            return null;
        
      


        
        return base.CreateAsync(input);
    }
    public async override Task DeleteAsync(int id)
    {
        if (_currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                 await base.DeleteAsync(id);
            }
        }
        else
             await base.DeleteAsync(id);
    }

    // Function to convert base64 string to image and save it
    private string SaveBase64Image(CreateUpdateGarageTransactionDto input)
    {
        string base64Image=input.VehiclePicture;
        try
        {
            // Validate the base64 string format
            if (string.IsNullOrEmpty(base64Image) || !base64Image.Contains(","))
            {
                throw new ArgumentException("Invalid base64 image format");
            }

            // Define the path where the image will be saved
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "GarageTransactionsImages",input.OrganizationID.ToString(), input.GarageId.ToString(), input.CameraId.ToString());

            // Ensure the directory exists
            if (!Directory.Exists(wwwRootPath))
            {
                Directory.CreateDirectory(wwwRootPath);
            }

            // Extract the image format and base64 content
            var base64Parts = base64Image.Split(',');
            if (base64Parts.Length < 2)
            {
                throw new ArgumentException("Invalid base64 image data");
            }

            var imageFormat = base64Parts[0].Contains("png") ? "png" : "jpg"; // Adjust formats as needed
            var imageBytes = Convert.FromBase64String(base64Parts[1]);

            // Generate a unique name for the image
            string fileName = $"{Guid.NewGuid()}.{imageFormat}";
            string fullPath = Path.Combine(wwwRootPath, fileName);

            // Save the image to the specified path
            File.WriteAllBytes(fullPath, imageBytes);

            // Return the relative path for storing in the database
            return $"/GarageTransactionsImages/{input.OrganizationID}/{input.GarageId}/{input.CameraId}/{fileName}";
        }
        //catch (FormatException ex)
        //{
        //    throw new UserFriendlyException("The provided image is not a valid Base64 string", ex);
        //}
        //catch (UnauthorizedAccessException ex)
        //{
        //    throw new UserFriendlyException("Failed to save the image due to insufficient permissions", ex);
        //}
        catch (Exception ex)
        {
            return null;
            // throw new UserFriendlyException("An error occurred while saving the image", ex);
        }
    }


  
}
