using SmartParking.Entities.Vehicles;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;


namespace SmartParking.Mobile.Vehicles;

[RemoteService(IsEnabled = false)]

public class VehicleAppService : ApplicationService, IVehicleAppService
{
    public readonly IRepository<Vehicle, int> _vehicleRepository;
    public readonly ICurrentUser _currentUser;
    private IDataFilter _dataFilter;

    public VehicleAppService(IRepository<Vehicle, int> vehicleRepository, ICurrentUser currentUser, IDataFilter dataFilter)
    {
        _vehicleRepository = vehicleRepository;
        _currentUser = currentUser;
        _dataFilter = dataFilter;
    }
    public async Task<Result<VehicleDto>> CreateAsync(CreateUpdateVehicleDto input)
    {
        try
        {
            // Check if the user is authenticated
            //if (!_currentUser.IsInRole("admin"))
            //{
            //    return Result<VehicleDto>.Error("User is not authenticated");
            //}
            var mappedVehicle = ObjectMapper.Map<CreateUpdateVehicleDto, Vehicle>(input);

            var vehicle = await _vehicleRepository.InsertAsync(mappedVehicle, autoSave: true);

            var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);

            return Result<VehicleDto>.Success(vehicleDto, "Vehicle Add Succesuffuly");
        }
        catch (Exception ex)
        {
            return Result<VehicleDto>.Error(ex.Message);
        }

    }

    public async Task<Result<PagedResultDto<VehicleDto>>> GetListAsync(PagedAndSortedResultRequestDto input)
    {

        // Check if the user is authenticated
        //if (!_currentUser.IsInRole("admin"))
        //{
        //    return Result<VehicleDto>.Error("User is not authenticated");
        //}

        var queryable = await _vehicleRepository.GetQueryableAsync();

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
        //Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        //Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        //Execute the query and get the list of hotel entities
        var vehicles = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var vehiclesDto = ObjectMapper.Map<List<Vehicle>, List<VehicleDto>>(vehicles);
        // Return the paginated result
        return new Result<PagedResultDto<VehicleDto>>(
           new PagedResultDto<VehicleDto>
           (totalCount, vehiclesDto)
       );

    }



}