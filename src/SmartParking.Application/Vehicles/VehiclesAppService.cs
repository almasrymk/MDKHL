
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
namespace SmartParking.Vehicles;

[Authorize]
public class VehiclesAppService :
    CrudAppService<
        Vehicle,
        VehicleDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateVehicleDto
    >,
    IVehiclesAppService
{
    private readonly IRepository<Vehicle, int> _repository;
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;

    public VehiclesAppService(
        IRepository<Vehicle, int> repository,
        IDataFilter dataFilter,
        ICurrentUser currentUser,
        IRepository<LicenceType, int> licenceTypeRepository)
        : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _licenceTypeRepository = licenceTypeRepository;
    }

    #region Public Methods
    /// <inheritdoc/>
 
    public override async Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input)
    {
        input.CreatorId = _currentUser.Id;

        ValidatePlateNumbers(input);
        ValidateModelYear(input.ModelYear);

        await CheckVehicleIfExistsAsync(input);
        await AssignPlateColorFromLicenceTypeAsync(input);

        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.CreateAsync(input);
            }
        }

        return await base.CreateAsync(input);
    }
    /// <inheritdoc/>

    public override async Task<VehicleDto> UpdateAsync(int id, CreateUpdateVehicleDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var vehicle = await _repository.GetAsync(id) ?? throw new InvalidOperationException("Vehicle not found");

            ValidatePlateNumbers(input);
            ValidateModelYear(input.ModelYear);

            await AssignPlateColorFromLicenceTypeAsync(input);

            var existingVehicle = await _repository.FirstOrDefaultAsync(x =>
                x.ArPlateNumber == input.FullArPlate &&
                x.EnPlateNumber == input.FullEnPlate &&
                x.Id != id &&
                x.LicenceTypeId == input.LicenceTypeId &&
                x.PlateColorId == input.PlateColorId &&
                x.VehicleBrandId == input.VehicleBrandId &&
                x.BelongToCompany == input.BelongToCompany &&
                x.CompanyName == input.CompanyName
            );

            if (existingVehicle != null)
            {
                throw new InvalidOperationException("Plate number already exists");
            }

            input.LastModificationTime = DateTime.Now;
            input.LastModifierId = _currentUser.Id;

            return await base.UpdateAsync(id, input);
        }
    }
    /// <inheritdoc/>

    public override async Task<PagedResultDto<VehicleDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (CurrentTenant.Id == null)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }

        return await base.GetListAsync(input);
    }
    /// <inheritdoc/>

    public override async Task<VehicleDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }
    /// <inheritdoc/>

    public override async Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var vehicle = await _repository.FindAsync(id);
            if (vehicle == null)
                throw new EntityNotFoundException(typeof(Vehicle), id);

            await base.DeleteAsync(id);
        }
    }

    [RemoteService(IsEnabled = false)]
    /// <inheritdoc/>

    public async Task<(string numbers, string letters)> SplitPlateAsync(string plate)
    {
        await Task.Yield();

        var matches = Regex.Matches(plate, @"[\d]+|[\p{L}]+");

        string letters = "";
        string numbers = "";

        foreach (Match match in matches)
        {
            if (Regex.IsMatch(match.Value, @"^[\p{L}]+$"))
            {
                letters += match.Value;
            }
            else if (Regex.IsMatch(match.Value, @"^\d+$"))
            {
                numbers += match.Value;
            }
        }

        return (numbers, letters);
    }

    [RemoteService(IsEnabled = false)]
    /// <inheritdoc/>

    public void SplitPlate(string plate, out string numbers, out string letters)
    {
        var match = Regex.Match(plate, @"(?<letters>[A-Za-z\u0600-\u06FF]+)(?<numbers>\d+)|(?<numbers>\d+)(?<letters>[A-Za-z\u0600-\u06FF]+)");
        if (match.Success)
        {
            letters = match.Groups["letters"].Value;
            numbers = match.Groups["numbers"].Value;
        }
        else
        {
            letters = "";
            numbers = plate;
        }
    }

    #endregion

    #region Private Methods
    /// <summary>
    /// Checks if a vehicle already exists based on its Arabic or English plate number.
    /// </summary>
    /// <param name="input">The DTO containing the details of the vehicle to check.</param>
    /// <exception cref="InvalidOperationException">Thrown if a vehicle with the same plate number already exists.</exception>
    private async Task CheckVehicleIfExistsAsync(CreateUpdateVehicleDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var existingVehicle = await _repository.FirstOrDefaultAsync(_ =>
                _.ArPlateNumber == input.FullArPlate || _.EnPlateNumber == input.FullEnPlate);

            if (existingVehicle is not null)
            {
                throw new InvalidOperationException("This vehicle already exists");
            }
        }
    }
    /// <summary>
    /// Validates the vehicle's plate numbers to ensure they meet the required criteria.
    /// </summary>
    /// <param name="input">The DTO containing the vehicle's plate numbers.</param>
    /// <exception cref="InvalidOperationException">Thrown if the plate numbers are missing or invalid.</exception>
    private void ValidatePlateNumbers(CreateUpdateVehicleDto input)
    {
        if (!VehicleValidation.CheckPlateNumberCriteria(input.FullArPlate, input.FullEnPlate))
        {
            throw new InvalidOperationException("Plate numbers are missing or invalid");
        }
    }
    /// <summary>
    /// Validates the vehicle's model year.
    /// </summary>
    /// <param name="year">The model year to validate.</param>
    /// <exception cref="UserFriendlyException">Thrown if the model year is invalid.</exception>
    private void ValidateModelYear(string year)
    {
        if (VehicleValidation.ValidateYear(year) is not null)
        {
            throw new UserFriendlyException(LocalizationHelper.GetLocalizedString("NotValidYear"));
        }
    }
    /// <summary>
    /// Assigns a plate color to the vehicle based on its licence type.
    /// </summary>
    /// <param name="input">The DTO containing the vehicle details and licence type.</param>
    /// <exception cref="InvalidOperationException">Thrown if the licence type is not found.</exception>
    private async Task AssignPlateColorFromLicenceTypeAsync(CreateUpdateVehicleDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var licenceType = await _licenceTypeRepository.FirstOrDefaultAsync(l => l.Id == input.LicenceTypeId);
            if (licenceType == null)
            {
                throw new InvalidOperationException("Licence type not found");
            }

            input.PlateColorId = licenceType.PlateColorId;
        }
    }

    #endregion
}
