using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.DefaultPackages
{
    public class DefaultPackagesAppService : ApplicationService, IDefaultPackagesAppService
    {
        private readonly IRepository<DefaultPackage, int> _defaultPackageRepository;
        public DefaultPackagesAppService(IRepository<DefaultPackage, int> defaultPackageRepository)
        {
            _defaultPackageRepository = defaultPackageRepository;
        }

        public IRepository<DefaultPackage, int> DefaultPackageRepository { get; }

        public async Task<DefaultPackageDto> CreateAsync(CreateUpdateDefaultPackageDto input)
        {
            var oldPackage = _defaultPackageRepository.FirstOrDefaultAsync(_ => _.OrgnizationId == input.OrgnizationId && _.Status == Enums.Status.Active);
            var defaultPackage = ObjectMapper.Map<CreateUpdateDefaultPackageDto, DefaultPackage>(input);
            var newPacage = new DefaultPackage();
            if (oldPackage != null)
                newPacage = await _defaultPackageRepository.InsertAsync(defaultPackage, autoSave: true);
            else
                newPacage = await _defaultPackageRepository.UpdateAsync(defaultPackage, autoSave: true);
            return ObjectMapper.Map<DefaultPackage, DefaultPackageDto>(newPacage);
        }

        //public async Task<string> DeleteByIdAsync(int id)
        //{
        //    var defaultPackage = await  _defaultPackageRepository.GetAsync(id);

        //    if (defaultPackage is null)
        //        throw new EntityNotFoundException(typeof(DefaultPackage), id);
        //    else
        //    {
        //        defaultPackage.Status = Status.Deleted;
        //        return EnumValues.GetEnumDescription(OrganizationCreationValidations.OrganizationDeleteion);
        //    }
        //}

        public async Task<DefaultPackageDto> GetAsync(int id)
        {
            var defaultPackage = await _defaultPackageRepository.WithDetailsAsync(_ => _.Organization)
                                                                            .Result
                                                                            .AsQueryable()
                                                                            .FirstOrDefaultAsync(_=>_.Id==id);
            if (defaultPackage is null)
                throw new EntityNotFoundException(typeof(DefaultPackage), id);
            else
            
                return ObjectMapper.Map<DefaultPackage, DefaultPackageDto>(defaultPackage);
        }

        public async Task<DefaultPackageDto> GetbyOrganizationAsync(int OrgnizationId)
        {
            var defaultPackage = await _defaultPackageRepository.WithDetailsAsync(_ => _.Organization)
                                                                            .Result
                                                                            .AsQueryable()
                                                                            .FirstOrDefaultAsync(_ => _.OrgnizationId == OrgnizationId);
            if (defaultPackage is null)
                throw new EntityNotFoundException(typeof(DefaultPackage), OrgnizationId);
            else

                return ObjectMapper.Map<DefaultPackage, DefaultPackageDto>(defaultPackage);
        }

        public async Task<PagedResultDto<DefaultPackageDto>> GetListAsync(DefaultPackageListDto input)
        {
            if(input.Sorting.IsNullOrWhiteSpace())
                input.Sorting=nameof(DefaultPackage.Id);
            var defaultPackages = await _defaultPackageRepository.WithDetailsAsync(_ => _.Organization)
                                                                 .Result
                                                                 .AsQueryable()
                                                                 .WhereIf(!input.Filter.IsNullOrWhiteSpace()
                                                                          , _ => _.Organization.ArabicName.Contains(input.Filter)
                                                                              || _.Organization.EnglishName.Contains(input.Filter))
                                                                 .Skip(input.SkipCount)
                                                                 .Take(input.MaxResultCount)
                                                                 .OrderBy(input.Sorting)
                                                                 .ToListAsync();
            var totalCount = input.Filter == null 
                ? await _defaultPackageRepository.CountAsync()
                : await _defaultPackageRepository.CountAsync(_ => _.Organization.ArabicName.Contains(input.Filter)
                                                               || _.Organization.EnglishName.Contains(input.Filter));

            return new PagedResultDto<DefaultPackageDto>(
                totalCount, 
                ObjectMapper.Map<List<DefaultPackage>, List<DefaultPackageDto>>(defaultPackages));
        }

        public async Task<DefaultPackageDto> UpdateAsync(int id, CreateUpdateDefaultPackageDto input)
        {
            var oldPackage = _defaultPackageRepository.FirstOrDefaultAsync(_ => _.Id == id && _.Status == Enums.Status.Active);
            if (oldPackage is null)
                throw new EntityNotFoundException(typeof(DefaultPackage), id);
            else
            {
                var defaultPackage = ObjectMapper.Map<CreateUpdateDefaultPackageDto, DefaultPackage>(input);
                var newPacage = await _defaultPackageRepository.UpdateAsync(defaultPackage, autoSave: true);
                return ObjectMapper.Map<DefaultPackage, DefaultPackageDto>(newPacage);
            }
        }
    }
    
}
