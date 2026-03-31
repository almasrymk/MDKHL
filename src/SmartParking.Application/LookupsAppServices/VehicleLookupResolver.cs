using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Lookups.VehicleLookups;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;

public class VehicleLookupResolver : SmartParkingAppService, IVehicleLookupResolver
{
    private readonly IRepository<VehicleBrand, int> _brandRepository;
    private readonly IRepository<VehicleModel, int> _modelRepository;
    private readonly IRepository<PlateColor, int> _plateColorRepository;
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;


    public VehicleLookupResolver(
        IRepository<VehicleBrand, int> brandRepository,
        IRepository<VehicleModel, int> modelRepository,
        IRepository<PlateColor, int> plateColorRepository,
        IRepository<LicenceType, int> licenceTypeRepository)
    {
        _brandRepository = brandRepository;
        _modelRepository = modelRepository;
        _plateColorRepository = plateColorRepository;
        _licenceTypeRepository = licenceTypeRepository;
    }

    public async Task<int> ResolveBrandIdAsync(string brandName)
    {
        brandName = Normalize(brandName);

        var entity = await _brandRepository.FirstOrDefaultAsync(x => x.ArabicName.ToLower() == brandName || x.EnglishName.ToLower() == brandName);

        if (entity == null)
            throw new BusinessException($"Invalid Brand: {brandName}");

        return entity.Id;
    }

    public async Task<int> ResolveModelIdAsync(string modelName)
    {
        modelName = Normalize(modelName);

        var entity = await _modelRepository.FirstOrDefaultAsync(x => x.ArabicName.ToLower() == modelName || x.EnglishName.ToLower() ==modelName);

        if (entity == null)
            throw new BusinessException($"Invalid Model: {modelName}");

        return entity.Id;
    }

    public async Task<int> ResolvePlateColorIdAsync(string plateColorName)
    {
        plateColorName = Normalize(plateColorName);

        var entity = await _plateColorRepository.FirstOrDefaultAsync(x => x.ArabicName.ToLower() == plateColorName || x.EnglishName.ToLower() ==plateColorName);

        if (entity == null)
            throw new BusinessException($"Invalid PlateColor: {plateColorName}");

        return entity.Id;
    }

    public async Task<int> ResolveLicenceTypeIdAsync(string licenceTypeName)
    {
        licenceTypeName = Normalize(licenceTypeName);

        var entity = await _licenceTypeRepository.FirstOrDefaultAsync(x => x.ArabicName.ToLower() == licenceTypeName || x.EnglishName.ToLower() == licenceTypeName);

        if (entity == null)
            throw new BusinessException($"Invalid LicenceType: {licenceTypeName}");

        return entity.Id;
    }

    private string Normalize(string input)
    {
        return input?.Trim().ToLower() ?? string.Empty;
    }
}