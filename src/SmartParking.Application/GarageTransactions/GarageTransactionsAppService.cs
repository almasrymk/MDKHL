
using Microsoft.AspNetCore.Http;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.GarageTransactions;
using System.Linq.Dynamic.Core;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using SmartParking.Results;
using SmartParking.Vehicles;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Vehicles;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace SmartParking.GarageTransactions;

//[Authorize]
public class GarageTransactionsAppService:
	 CrudAppService<GarageTransaction, GarageTransactionDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageTransactionDto>, IGarageTransactionsAppService
{
	private readonly IRepository<GarageTransaction, int> _repository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<Organization, int> _OrganizationRepository;
    private readonly IRepository<Customer, Guid> _CustomerRepository;
    private readonly IRepository<Vehicle, int> _VehicleRepository;
    private readonly IRepository<Camera, int> _CameraRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataFilter _dataFilter;
	private readonly ICurrentUser _currentUser;

    public GarageTransactionsAppService(IRepository<GarageTransaction, int> repository, IDataFilter dataFilter, ICurrentUser currentUser, IRepository<Garage, int> garagerepository, IRepository<Organization, int> organizationRepository, IRepository<Camera, int> cameraRepository, IHttpContextAccessor httpContextAccessor, IRepository<Customer, Guid> customerRepository, IRepository<Vehicle, int> vehicleRepository) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _garageRepository = garagerepository;
        _OrganizationRepository = organizationRepository;
        _CameraRepository = cameraRepository;
        _httpContextAccessor = httpContextAccessor;
        _VehicleRepository = vehicleRepository;
        _CustomerRepository= customerRepository;
    }
    public override async Task<PagedResultDto<GarageTransactionDto>> GetListAsync(PagedAndSortedResultRequestDto input)
	{

        var queryable = await _repository.GetQueryableAsync();


        if (!input.Sorting.IsNullOrEmpty())
        {
            // Determine if the sorting should be ascending or descending
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else 
            {
                
                queryable = queryable.OrderBy(input.Sorting);
            }
        }

        using (DataFilter.Disable<IMultiTenant>())
        {
            var transactions = await base.GetListAsync(input);

            return transactions;
        }

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
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.AccurateTime.Value.ToString().ToLower() == (input.SearchingTerm.ToLower())||
                or.SnapTime.Value.ToString().ToLower()== (input.SearchingTerm.ToLower())

            ); ;
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
    public async Task<PagedResultDto<GarageTransactionDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
	{
        using (_dataFilter.Disable<IMultiTenant>())
        {

		    // Get the IQueryable<camera> from the repository
		    var queryable = await _repository.GetQueryableAsync();
		    // Apply search filtering
		    if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
		    {

                queryable = queryable.Where(or =>
                    or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.AccurateTime.Value.ToString().ToLower() == (input.SearchingTerm.ToLower())||
                    or.SnapTime.Value.ToString().ToLower()== (input.SearchingTerm.ToLower())

                ); ;
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
	}

	/// <summary>
	/// method used to get list of cameraes
	/// </summary>
	/// <param name="input">specifies max number of camera and if it filtered or not</param>
	/// <returns>cameraeDto List</returns>
	public async Task<PagedResultDto<GarageTransactionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
	{
        // Get the IQueryable<garage> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<GarageTransaction>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of garages entities
        var transactions = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var transactionsDto = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(transactions);
        Console.WriteLine(transactionsDto.Count);
        // Return the paginated result
        return new PagedResultDto<GarageTransactionDto>(
            totalCount,
            transactionsDto
        );
    }
    public async Task<PagedResultDto<GarageTransactionDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
	{
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<garage> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<GarageTransaction>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of garages entities
            var transactions = await AsyncExecuter.ToListAsync(queryable);

            // Map the garages entities to garagesDto objects
            var transactionsDto = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(transactions);
            Console.WriteLine(transactionsDto.Count);
            // Return the paginated result
            return new PagedResultDto<GarageTransactionDto>(
                totalCount,
                transactionsDto
            );
        }
    }

    public override async Task<GarageTransactionDto> CreateAsync(CreateUpdateGarageTransactionDto input)
    {
            
        var result=await base.CreateAsync(input);
        
        return result;
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

    /// <inheritdoc/>
	public async Task<PagedResultDto<GarageTransactionDto>> GetAllTransactionsByOrganizationAsync(PagedAndSortedResultRequestDto input, Guid? tenantId)
	{
		var queryable = await _repository.GetQueryableAsync();

        queryable = queryable.Where(t=>t.TenantId == tenantId);

		if (!input.Sorting.IsNullOrEmpty())
		{
			// Determine if the sorting should be ascending or descending
			if (input.Sorting.Contains("DESC"))
			{
				var sorting = input.Sorting.Split(' ').ToList();
				queryable = queryable.OrderBy(sorting[0] + " descending");
			}
			else
			{

				queryable = queryable.OrderBy(input.Sorting);
			}
		}
         
			var totalCount = await AsyncExecuter.CountAsync(queryable);
			var items = await AsyncExecuter.ToListAsync(
				queryable
					.Skip(input.SkipCount)
					.Take(input.MaxResultCount)
			);

			// Map entities to DTOs
			var transactionDtos = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(items);

			// Return paged result
			return new PagedResultDto<GarageTransactionDto>(totalCount, transactionDtos);
		 
		
	}

    private async Task<Guid> GetCurrentCustomerId()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

             var customer = await _CustomerRepository.GetQueryableAsync();
             var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
            return customerId;
        }
    }



    public async Task<Result<List<GarageTransactionDto>>> GetCustomerTransactionsInOrganization(int organizationId)
    {
        var customerId= await GetCurrentCustomerId();
        using (_dataFilter.Disable<IMultiTenant>()) {
            var query = await _VehicleRepository.GetQueryableAsync();
            query = query.Where(x => x.CustomerId == customerId);
            var garageTransactionQuery =await _repository.GetQueryableAsync();
            garageTransactionQuery = garageTransactionQuery.Where(GT=>GT.OrganizationID== organizationId);
            garageTransactionQuery = garageTransactionQuery.Where(GT => query.Any(vehicle => vehicle.ArPlateNumber == GT.PlateNumber || vehicle.EnPlateNumber == GT.PlateNumber));
            var transactions = await AsyncExecuter.ToListAsync(garageTransactionQuery);
            var transactionsDtos = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(transactions);
            return Result.Success(transactionsDtos);

        }
    }
}
