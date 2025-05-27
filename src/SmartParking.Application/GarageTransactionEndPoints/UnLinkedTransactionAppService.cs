

using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Filters;
using SmartParking.Garages;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.GarageTransactionEndPoints;

public class UnLinkedTransactionAppService : CrudAppService<UnLinkedTransaction, UnLinkedTransactionDto, int,
    PagedAndSortedResultRequestDto, CreateUpdateGarageTransactionEndPointDto>,
    IUnLinkedTransactionAppService
{
    private readonly IRepository<UnLinkedTransaction, int> _repository;
    private readonly IDataFilter _dataFilter;

    public UnLinkedTransactionAppService(IRepository<UnLinkedTransaction, int> repository,IDataFilter dataFilter) :base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }


    public async override Task<UnLinkedTransactionDto> CreateAsync(CreateUpdateGarageTransactionEndPointDto input)
    {
        input.VehiclePicture = SaveBase64Image(input) ?? input.VehiclePicture;
        var result = await base.CreateAsync(input);
        return result;
    }
    ///<inheritdoc/>
    public async Task<PagedResultDto<UnLinkedTransactionDto>> GetListAsync(PagedAndSortedAndSearchResultRequestDto input )
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {
                string search = input.SearchingTerm.ToLower();
                queryable = queryable.Where(x =>
                    x.PlateNumber.ToLower().Contains(search.ToLower()) ||
                    x.DeviceId.ToLower().Contains(search.ToLower()) ||
                    x.AccurateTime.HasValue.ToString().ToLower().Contains(search.ToLower()) ||
                    x.SnapTime.HasValue.ToString().ToLower().Contains(search.ToLower())

                );
            }
            // Sorting
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC", StringComparison.OrdinalIgnoreCase))
                {
                    var sorting = input.Sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
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

            var dtos = ObjectMapper.Map<List<UnLinkedTransaction>, List<UnLinkedTransactionDto>>(items);

            return new PagedResultDto<UnLinkedTransactionDto>(totalCount, dtos);
        }
    }
    

    ///<inheritdoc/>
    public async override  Task<UnLinkedTransactionDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }

    ///<inheritdoc/>
    public async Task<PagedResultDto<UnLinkedTransactionDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<unLinkedTransaction> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) 
            );
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
        // Execute the query and get the list of hotel entities
        var unLinkedTransactions = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var unLinkedTransactionDtos = ObjectMapper.Map<List<UnLinkedTransaction>, List<UnLinkedTransactionDto>>(unLinkedTransactions);
        // Return the paginated result
        return new PagedResultDto<UnLinkedTransactionDto>(
            totalCount,
            unLinkedTransactionDtos
        );
    }
    ///<inheritdoc/>
    public async Task<PagedResultDto<UnLinkedTransactionDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<unLinkedTransaction> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<Garage>.BuildFilterExpression(input.filters);
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
            // Execute the query and get the list of unLinkedTransactions entities
            var unLinkedTransactions = await AsyncExecuter.ToListAsync(queryable);

            // Map the unLinkedTransaction entities to unLinkedTransactionDto objects
            var unLinkedTransactionDto = ObjectMapper.Map<List<UnLinkedTransaction>, List<UnLinkedTransactionDto>>(unLinkedTransactions);
            // Return the paginated result
            return new PagedResultDto<UnLinkedTransactionDto>(
                totalCount,
                unLinkedTransactionDto
            );
        }
    }

    // Function to convert base64 string to image and save it
    private string SaveBase64Image(CreateUpdateGarageTransactionEndPointDto input)
    {
        string base64Image = input.VehiclePicture;
        try
        {

            var imageFormat = "jpg"; // Adjust formats as needed
                                     // CheckBase64(ref base64Image, ref imageFormat);
            CreateBase64Image(ref base64Image);
            var imageBytes = Convert.FromBase64String(base64Image);

            var imageDirectoryPath = CreateImageDirectory(input);




            // Generate a unique name for the image
            string fileName = $"{Guid.NewGuid()}.{imageFormat}";
            string fullPath = Path.Combine(imageDirectoryPath, fileName);

            // Save the image to the specified path
            File.WriteAllBytes(fullPath, imageBytes);

            // Return the relative path for storing in the database
            return $"/UnlinkedGarageTransactionsImages/{input.DeviceID}/{input.PlateNumber}/{fileName}";
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

    private string CreateImageDirectory(CreateUpdateGarageTransactionEndPointDto input)
    {
        // Define the path where the image will be saved
        string ImageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UnlinkedGarageTransactionsImages", input.DeviceID, input.PlateNumber);

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
