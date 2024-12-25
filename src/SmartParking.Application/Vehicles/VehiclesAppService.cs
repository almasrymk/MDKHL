
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
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
public class VehiclesAppService:
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
	private readonly ICurrentUser _currentUser; // Inject ICurrentUser
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;

    public VehiclesAppService(IRepository<Vehicle, int> repository,
		IDataFilter dataFilter, ICurrentUser currentUser,IRepository<LicenceType,int> licenceTypeRepository)
	   : base(repository)
	{
		_repository = repository;
		_dataFilter = dataFilter;
		_currentUser = currentUser;
        _licenceTypeRepository = licenceTypeRepository;
    }

    public override async Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input)
    {

        // check plate number must include both plate letters and plate numbers, either in Arabic or English.

        input.ArPlateNumber = input.ArPlateNumber.Replace(" ", string.Empty) + input.ArPlateLetters.Replace(" ", string.Empty);
        input.EnPlateNumber = input.EnPlateNumber.Replace(" ", string.Empty) + input.EnPlateLetters.Replace(" ", string.Empty);
        if(!CheckPlateNumberCriteria(input.ArPlateNumber, input.EnPlateNumber))
        {
            input.ArPlateNumber = input.EnPlateNumber=null;
            throw new InvalidOperationException("Plate Numbers are Missing");
        }
        await CheckVehicleIfFound(input);

        LicenceType licenceType;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            licenceType = await _licenceTypeRepository.FirstOrDefaultAsync(l => l.Id == input.LicenceTypeId);
        }

        if (licenceType == null)
        {
            throw new InvalidOperationException("licence type not found");

        }
        var existingPlatNumber = await _repository.FirstOrDefaultAsync(x => x.ArPlateNumber == input.ArPlateNumber || x.EnPlateNumber == input.EnPlateNumber);
        input.PlateColorId = licenceType.PlateColorId;
        if (existingPlatNumber != null)
        {
            throw new InvalidOperationException("PlatNumber already exists");
        }
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.CreateAsync(input);
            }
        }
        else
            return await base.CreateAsync(input);
    }

    private bool CheckPlateNumberCriteria(string ArabicPlateNumber , string EnglishPlateNumber)
    {
        string arabicLetterPattern = @"[\u0600-\u06FF]"; 
        string arabicDigitPattern = @"[\u0660-\u0669]"; 

        string englishLetterPattern = @"[A-Za-z]";  
        string englishDigitPattern = @"[0-9]";      

        bool isArabicPlateValid = !string.IsNullOrWhiteSpace(ArabicPlateNumber) &&
                                  System.Text.RegularExpressions.Regex.IsMatch(ArabicPlateNumber, arabicLetterPattern) &&
                                  System.Text.RegularExpressions.Regex.IsMatch(ArabicPlateNumber, arabicDigitPattern);

        bool isEnglishPlateValid = !string.IsNullOrWhiteSpace(EnglishPlateNumber) &&
                                   System.Text.RegularExpressions.Regex.IsMatch(EnglishPlateNumber, englishLetterPattern) &&
                                   System.Text.RegularExpressions.Regex.IsMatch(EnglishPlateNumber, englishDigitPattern);

        return isArabicPlateValid && isEnglishPlateValid;

    }

    private async Task CheckVehicleIfFound(CreateUpdateVehicleDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var existVehicle = await _repository.FirstOrDefaultAsync(_ => (_.ArPlateNumber == input.ArPlateNumber || _.EnPlateNumber == input.EnPlateNumber));
            if (existVehicle is not null)
            {
                throw new InvalidOperationException("this vehicle is exist");
            }
        }
    }

    public override async Task<PagedResultDto<VehicleDto>> GetListAsync(PagedAndSortedResultRequestDto input)
	{
        if (CurrentTenant.Id == null)
        {
            using (_dataFilter.Disable<IMultiTenant>())
			{
				return await base.GetListAsync(input);
			}
		}
		else
			return await base.GetListAsync(input);
	}
	public override async Task<VehicleDto> UpdateAsync(int id, CreateUpdateVehicleDto input)
	{
        using (_dataFilter.Disable<IMultiTenant>())
        {
            input.ArPlateNumber = input.ArPlateNumber.Replace(" ", string.Empty) + input.ArPlateLetters.Replace(" ", string.Empty);
            input.EnPlateNumber = input.EnPlateNumber.Replace(" ", string.Empty) + input.EnPlateLetters.Replace(" ", string.Empty);
            if (!CheckPlateNumberCriteria(input.ArPlateNumber, input.EnPlateNumber))
            {
                input.ArPlateNumber = input.EnPlateNumber = null;
                throw new InvalidOperationException("Plate Numbers are Missing");
            }
            var licenceType = await _licenceTypeRepository.FirstOrDefaultAsync(l => l.Id == input.LicenceTypeId);

            if (licenceType == null)
            {
                throw new InvalidOperationException("licence type not found");

            }
            var existingPlatNumber = await _repository.FirstOrDefaultAsync(x => x.ArPlateNumber == input.ArPlateNumber && x.EnPlateNumber == input.EnPlateNumber);
            input.PlateColorId = licenceType.PlateColorId;
            if (existingPlatNumber != null && existingPlatNumber.LicenceTypeId == input.LicenceTypeId && existingPlatNumber.PlateColorId == input.PlateColorId && existingPlatNumber.VehicleBrandId == input.VehicleBrandId && existingPlatNumber.BelongToCompany == input.BelongToCompany && existingPlatNumber.CompanyName == input.CompanyName)
            {
                throw new InvalidOperationException("PlatNumber already exists");
            }

            return await base.UpdateAsync(id,input);
        }
    }
    [RemoteService(IsEnabled = false)]
    public async Task<(string numbers, string letters)> SplitPlateAsync(string plate)
    {
        
        await Task.Yield();

        // Regex to capture groups of Arabic/English letters and numbers
        var matches = Regex.Matches(plate, @"[\d]+|[\p{L}]+");

        string letters = "";
        string numbers = "";

        foreach (Match match in matches)
        {
            if (Regex.IsMatch(match.Value, @"^[\p{L}]+$")) // Letters
            {
                letters += match.Value; // Append letters
            }
            else if (Regex.IsMatch(match.Value, @"^\d+$")) // Numbers
            {
                numbers += match.Value; // Append numbers
            }
        }


        return (numbers, letters);
    }
    [RemoteService(IsEnabled = false)]
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


    public override async Task<VehicleDto> GetAsync(int id)
	{
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
	}

	public override async Task DeleteAsync(int id)
	{
		var vehicle = await _repository.FindAsync(id);
		if (vehicle == null )
			throw new EntityNotFoundException(typeof(Vehicle), id);
            using (_dataFilter.Disable<IMultiTenant>())
			{

                await base.DeleteAsync(id);
            }
    }

   
}
