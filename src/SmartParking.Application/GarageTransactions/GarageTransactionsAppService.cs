using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.GarageTransactions.Factory;
using SmartParking.Localization;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.GarageTransactions;

//[Authorize]
public class GarageTransactionsAppService :
     CrudAppService<GarageTransaction, GarageTransactionDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageTransactionDto>, IGarageTransactionsAppService
{
    private readonly IRepository<GarageTransaction, int> _repository;
    private readonly IRepository<Customer, Guid> _CustomerRepository;
    private readonly IRepository<Vehicle, int> _VehicleRepository;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;

    private readonly IDataFilter _dataFilter;
    private readonly IGarageTransactionsActionsFactoryAppService _actionsFactoryService;


    public GarageTransactionsAppService(IRepository<GarageTransaction, int> repository, IDataFilter dataFilter, IRepository<Customer, Guid> customerRepository, IRepository<Vehicle, int> vehicleRepository, IGarageTransactionsActionsFactoryAppService actionsFactoryService, IRepository<CustomerPackage, int> customerPackageRepository) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _VehicleRepository = vehicleRepository;
        _CustomerRepository = customerRepository;
        _actionsFactoryService = actionsFactoryService;
        _customerPackageRepository = customerPackageRepository;
    }
    #region public methods
    public async Task<PagedResultDto<GarageTransactionDto>> GetAllListAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var queryable = from garageTransaction in await _repository.GetQueryableAsync()
                            where tenantId == null || garageTransaction.TenantId == tenantId
                            join vehicle in await _VehicleRepository.GetQueryableAsync()
                                on garageTransaction.PlateNumber equals vehicle.EnPlateNumber
                                into vehicleJoin1
                            from v1 in vehicleJoin1.DefaultIfEmpty()
                            join vehicle2 in await _VehicleRepository.GetQueryableAsync()
                                on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                                into vehicleJoin2
                            from v2 in vehicleJoin2.DefaultIfEmpty()
                            orderby garageTransaction.Garage.NameEn
                            select new GarageTransactionDto
                            {
                                Id = garageTransaction.Id,
                                PlateNumber = garageTransaction.PlateNumber,
                                AccurateTime = garageTransaction.AccurateTime,
                                CameraType = garageTransaction.CameraType,
                                GarageEnglishName = garageTransaction.Garage.NameEn,
                                GarageArabicName = garageTransaction.Garage.NameAr,
                                OrganizationEnglishName = garageTransaction.Organization.EnglishName,
                                OrganizationArabicName = garageTransaction.Organization.ArabicName,
                                ArabicName = (v1 == null) ? (v2 == null) ? "NotCustomer" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                                TicketId = garageTransaction.TicketId,
                                SnapTime = garageTransaction.SnapTime,
                                CustomerCategory = garageTransaction.CustomerCategory,
                                IsMapped = garageTransaction.IsMapped
                            };

            queryable = ApplyFilterTenant(tenantId, queryable);
            queryable = ApplySearch(input, queryable);
            queryable = ApplySort(input, queryable);
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            List<GarageTransactionDto> garageTransactions = await ApplyPaged(input, queryable);

            return new PagedResultDto<GarageTransactionDto>(
                totalCount,
                garageTransactions
            );
        }
    }

    public async override Task<GarageTransactionDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var garagetransaction = await base.GetAsync(id);
            if (garagetransaction == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Transaction") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return garagetransaction;

        }

    }
    public async Task<PagedResultDto<GarageTransactionDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<camera> from the repository
        var queryable = from garageTransaction in await _repository.GetQueryableAsync()                       
                        join vehicle in await _VehicleRepository.GetQueryableAsync()
                            on garageTransaction.PlateNumber equals vehicle.EnPlateNumber
                            into vehicleJoin1
                        from v1 in vehicleJoin1.DefaultIfEmpty()
                        join vehicle2 in await _VehicleRepository.GetQueryableAsync()
                            on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                            into vehicleJoin2
                        from v2 in vehicleJoin2.DefaultIfEmpty()
                        orderby garageTransaction.Garage.NameEn
                        select new GarageTransactionDto
                        {
                            Id = garageTransaction.Id,
                            PlateNumber = garageTransaction.PlateNumber,
                            AccurateTime = garageTransaction.AccurateTime,
                            CameraType = garageTransaction.CameraType,
                            GarageEnglishName = garageTransaction.Garage.NameEn,
                            GarageArabicName = garageTransaction.Garage.NameAr,
                            OrganizationEnglishName = garageTransaction.Organization.EnglishName,
                            OrganizationArabicName = garageTransaction.Organization.ArabicName,
                            ArabicName = (v1 == null) ? (v2 == null) ? "NotCustomer" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                            TicketId = garageTransaction.TicketId,
                            SnapTime = garageTransaction.SnapTime,
                            CustomerCategory = garageTransaction.CustomerCategory,
                            IsMapped = garageTransaction.IsMapped
                        };
        // Apply search filtering
        queryable = ApplySearch(input, queryable);
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
        //queryable = queryable.OrderByDescending(x => x.Id);
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of camera entities
        var garageTransactions = await AsyncExecuter.ToListAsync(queryable);

       // Return the paginated result
        return new PagedResultDto<GarageTransactionDto>(
            totalCount,
            garageTransactions
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
                    or.AccurateTime.Value.ToString().ToLower() == (input.SearchingTerm.ToLower()) ||
                    or.SnapTime.Value.ToString().ToLower() == (input.SearchingTerm.ToLower()) ||
                    or.CustomerCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.CameraType.ToString().ToLower().Contains(input.SearchingTerm.ToLower())
                );
            }
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
            queryable = queryable.OrderByDescending(x => x.Id);

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
        input.VehiclePicture = SaveBase64Image(input) ?? input.VehiclePicture;
        var result = await base.CreateAsync(input);

        return result;
    }
    public async override Task DeleteAsync(int id)
    {
        var transactionDto = await GetAsync(id);
        if (transactionDto == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Transaction") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        if (transactionDto.CameraType == WatcherType.Exit)
        {
            await _actionsFactoryService.UnMappingEntryTransactionForDeletedExit(transactionDto);
        }
        await base.DeleteAsync(id);
        await CurrentUnitOfWork.SaveChangesAsync();

    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageTransactionDto>> GetAllTransactionsByOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        var queryable = from garageTransaction in await _repository.GetQueryableAsync()
                    where tenantId == null || garageTransaction.TenantId == tenantId
                    join vehicle in await _VehicleRepository.GetQueryableAsync()
                        on garageTransaction.PlateNumber equals vehicle.EnPlateNumber
                        into vehicleJoin1
                    from v1 in vehicleJoin1.DefaultIfEmpty()
                    join vehicle2 in await _VehicleRepository.GetQueryableAsync()
                        on garageTransaction.PlateNumber equals vehicle2.ArPlateNumber
                        into vehicleJoin2
                    from v2 in vehicleJoin2.DefaultIfEmpty()
                    orderby garageTransaction.Garage.NameEn
                    select new GarageTransactionDto
                    {
                        Id = garageTransaction.Id,
                        PlateNumber = garageTransaction.PlateNumber,
                        AccurateTime = garageTransaction.AccurateTime,
                        CameraType = garageTransaction.CameraType,
                        GarageEnglishName = garageTransaction.Garage.NameEn,
                        GarageArabicName = garageTransaction.Garage.NameAr,
                        OrganizationEnglishName = garageTransaction.Organization.EnglishName,
                        OrganizationArabicName = garageTransaction.Organization.ArabicName,
                        ArabicName = ( v1 == null) ? (v2 == null) ? "NotCustomer" : v2.Customer.ArabicName : v1.Customer.ArabicName,
                        TicketId = garageTransaction.TicketId,
                        SnapTime = garageTransaction.SnapTime,
                        CustomerCategory = garageTransaction.CustomerCategory,
                        IsMapped = garageTransaction.IsMapped
                    };
              
        queryable = ApplySearch(input, queryable);

        if (!input.Sorting.IsNullOrEmpty())
        {            if (input.Sorting.Contains("DESC"))
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
        return new PagedResultDto<GarageTransactionDto>(totalCount, items);
    }

    // Function to convert base64 string to image and save it
    public string SaveBase64Image(CreateUpdateGarageTransactionDto input)
    {
        string base64Image = input.VehiclePicture;
        try
        {
            var imageFormat = "jpg"; 
            CreateBase64Image(ref base64Image);
            var imageBytes = Convert.FromBase64String(base64Image);
            var imageDirectoryPath = CreateImageDirectory(input);
            string fileName = $"{Guid.NewGuid()}.{imageFormat}";
            string fullPath = Path.Combine(imageDirectoryPath, fileName);
            File.WriteAllBytes(fullPath, imageBytes);
            return $"/GarageTransactionsImages/{input.OrganizationID}/{input.GarageId}/{input.CameraId}/{fileName}";
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<Result<List<GarageTransactionDto>>> GetCustomerCarsInOrganization(int organizationId)
    {
        var customerId = await GetCurrentCustomerId();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customerPackagesQuery = await _customerPackageRepository.GetQueryableAsync();
            customerPackagesQuery = customerPackagesQuery.Where(x => x.CustomerId == customerId && x.OrganizationId==organizationId);
            var garageTransactionsQuery = await _repository.GetQueryableAsync();
            garageTransactionsQuery = garageTransactionsQuery
            .Where(gt => gt.OrganizationID == organizationId && gt.IsIn==true)
            .Where(gt =>
                customerPackagesQuery.Any(cp =>
                    cp.ArPlateNumber == gt.PlateNumber ||
                    cp.EnPlateNumber == gt.PlateNumber ||
                    (
                           cp.PackageCode != null &&
                           gt.TicketId != null &&
                           cp.PackageCode == gt.TicketId
                     )));
            var transactions = await AsyncExecuter.ToListAsync(garageTransactionsQuery);
            var transactionsDtos = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(transactions);
            return Result.Success(transactionsDtos);

        }
    }

    #endregion
    #region Private Methods
    private static IQueryable<GarageTransactionDto> ApplyFilterTenant(Guid? tenantId, IQueryable<GarageTransactionDto> queryable)
    {
        if (tenantId.HasValue)
            queryable = queryable.Where(t => t.TenantId == tenantId);
        return queryable;
    }

    private async Task<List<GarageTransactionDto>> ApplyPaged(PagedAndSortedAndSearchResultRequestDto input, IQueryable<GarageTransactionDto> queryable)
    {
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var garageTransactions = await AsyncExecuter.ToListAsync(queryable);
        return garageTransactions;
    }

    private static IQueryable<GarageTransactionDto> ApplySort(PagedAndSortedResultRequestDto input, IQueryable<GarageTransactionDto> queryable)
    {
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

        return queryable;
    }


    private static IQueryable<GarageTransactionDto> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<GarageTransactionDto> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.AccurateTime.Value.ToString().ToLower().Contains(input.SearchingTerm) ||
                or.SnapTime.Value.ToString().ToLower().Contains(input.SearchingTerm) ||
                or.CustomerCategory.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.CameraType.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.OrganizationArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.OrganizationEnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.TicketId.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }

        return queryable;
    }


    private async Task UnMappingEntryTransactionForDeletedExit(string plateNumber)
    {
        var query = await _repository.GetQueryableAsync();

        var latestTransaction = query
                .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == WatcherType.Entry && gt.IsIn == false && gt.IsMapped == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .FirstOrDefault();
        latestTransaction.IsIn = true;
        latestTransaction.IsMapped = false;
        await CurrentUnitOfWork.SaveChangesAsync();
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

    private async Task<Guid> GetCurrentCustomerId()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _CustomerRepository.GetQueryableAsync();
            var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
            return customerId;
        }
    }
    #endregion
    #region Un used
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
    #endregion
}
