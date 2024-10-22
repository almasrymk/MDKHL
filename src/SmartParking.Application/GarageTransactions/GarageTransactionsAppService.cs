using Abp.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
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
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
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

        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
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
				or.PlateNumber.StartsWith(input.SearchingTerm) 
				//or.Type.StartsWith(input.SearchingTerm) 
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

    public override async Task<GarageTransactionDto> CreateAsync(CreateUpdateGarageTransactionDto input)
    {
      
           input.VehiclePicture = SaveBase64Image(input);
            if (input.VehiclePicture == null)
            return null;
        
      
            

        
        var result=await base.CreateAsync(input);
        if(result!=null)
        {
            var garage =await _GarageRepository.GetAsync(result.GarageId);
            if(input.CameraType==CameraType.Entry)
            {
                garage.CurrnetCapicity++;
            }
            else if(input.CameraType==CameraType.Exit)
            {
                garage.CurrnetCapicity--;
            }
           // return result;
        }
        return result;
        //return result;
    }
    public async override Task DeleteAsync(int id)
    {
        
             await base.DeleteAsync(id);
    }

    // Function to convert base64 string to image and save it
    private string SaveBase64Image(CreateUpdateGarageTransactionDto input)
    {
        string base64Image=input.VehiclePicture;
        try
        {

            var imageFormat = "jpg"; // Adjust formats as needed
                                     // CheckBase64(ref base64Image, ref imageFormat);
            CreateBase64Image(ref base64Image);
           var imageBytes = Convert.FromBase64String(base64Image);

            var imageDirectoryPath =  CreateImageDirectory(input);


            

            // Generate a unique name for the image
            string fileName = $"{Guid.NewGuid()}.{imageFormat}";
            string fullPath = Path.Combine(imageDirectoryPath, fileName);

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

    private void CreateBase64Image(ref string base64Image)
    {
        if (!base64Image.Contains(","))
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            base64Image = Convert.ToBase64String(imageBytes);
        }
        else
        {
            base64Image = base64Image.Split(',')[1];
        }
    }

    private string CreateImageDirectory(CreateUpdateGarageTransactionDto input)
    {
        // Define the path where the image will be saved
        string ImageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "GarageTransactionsImages", input.OrganizationID.ToString(), input.GarageId.ToString(), input.CameraId.ToString());

        // Ensure the directory exists
        if (!Directory.Exists(ImageDirectoryPath))
        {
            Directory.CreateDirectory(ImageDirectoryPath);
        }
        return ImageDirectoryPath;
    }
    
    private void CheckBase64(ref string base64Image, ref string imageFormat)
    {

        // Validate the base64 string format
        if (!base64Image.Contains(","))
        {
            // Extract the image format and base64 content
            var base64Parts = base64Image.Split(',');
            base64Image = base64Parts[1];
            imageFormat = base64Parts[0] switch
            {
                var x when x.Contains("jpeg") => "jpg",
                var x when x.Contains("webp") => "webp",
                var x when x.Contains("png") => "png",
                _ => "jpg"
            };
           
        }

        if (string.IsNullOrEmpty(base64Image))
        {
            throw new ArgumentException("The provided image is not a valid Base64 string");
        }
       

    }
}
