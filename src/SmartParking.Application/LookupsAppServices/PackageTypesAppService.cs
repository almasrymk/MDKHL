using SmartParking.Entities;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.MultiTenancy;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Filters;

namespace SmartParking.LookupsAppServices
{
    [Authorize]
    public class PackageTypesAppService : CrudAppService<PackageType, PackageTypeDto,
                                                    int, PagedAndSortedResultRequestDto,
                                                    CreateUpdatePackageTypeDto>,
        IPackageTypesAppService
    {
        #region Private Fields

        private readonly IRepository<PackageType, int> _packageTypeRepository;

        #endregion

        #region Constructor

        public PackageTypesAppService(IRepository<PackageType, int> packageTypeRepository)
            : base(packageTypeRepository)
        {
            _packageTypeRepository = packageTypeRepository;
        }

        #endregion

        #region Public Methods

        ///<inheritdoc/>
        public override async Task<PackageTypeDto> CreateAsync(CreateUpdatePackageTypeDto input)
        {
            await ValidatePackageTypeUniqunessAsync(input);
            return await base.CreateAsync(input);
        }

        ///<inheritdoc/>
        public override async Task<PackageTypeDto> UpdateAsync(int id, CreateUpdatePackageTypeDto input)
        {
            var existingPackage = await _packageTypeRepository.GetAsync(id);

            if (existingPackage is null)
            {
                throw new InvalidOperationException("Package Type not found");
            }

            await ValidatePackageTypeUniqunessAsync(input);
            return await base.UpdateAsync(id, input);
        }

        ///<inheritdoc/>
        public override async Task DeleteAsync(int id)
        {
            var packageType = await _packageTypeRepository.FirstOrDefaultAsync(p => p.Id == id);
            if (packageType == null)
                throw new EntityNotFoundException(typeof(PackageType), id);

            await _packageTypeRepository.HardDeleteAsync(packageType);
        }

        ///<inheritdoc/>
        public override Task<PagedResultDto<PackageTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                return base.GetListAsync(input);
            }
        }

        /// <summary>
        /// Method used to get list of package Types with filtering and pagination
        /// </summary>
        /// <param name="input">specifies max number of package Types and if it should be filtered or not</param>
        /// <returns>PackageTypeDto List</returns>
        public async Task<PagedResultDto<PackageTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
            var queryable = await _packageTypeRepository.GetQueryableAsync();
            var filterExpression = BuildExpression<PackageType>.BuildFilterExpression(input.filters);
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

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            var packageTypesDto = await AsyncExecuter.ToListAsync(queryable);
            var packageTypeDto = ObjectMapper.Map<List<PackageType>, List<PackageTypeDto>>(packageTypesDto);
            return new PagedResultDto<PackageTypeDto>(totalCount, packageTypeDto);
        }

        /// <summary>
        /// Method used to search package types
        /// </summary>
        /// <param name="input">specifies search term and pagination options</param>
        /// <returns>PackageTypeDto List</returns>
        public async Task<PagedResultDto<PackageTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            var queryable = await _packageTypeRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {
                queryable = queryable.Where(c =>
                    c.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    c.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())
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

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            var packageTypes = await AsyncExecuter.ToListAsync(queryable);
            var packageTypeDto = ObjectMapper.Map<List<PackageType>, List<PackageTypeDto>>(packageTypes);
            return new PagedResultDto<PackageTypeDto>(totalCount, packageTypeDto);
        }

        /// <summary>
        /// Gets all package types
        /// </summary>
        /// <returns>List of PackageTypeDto</returns>
        public async Task<List<PackageTypeDto>> GetAllListAsync()
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                var packageType = await _packageTypeRepository.GetListAsync();
                var packageTypeMap = ObjectMapper.Map<List<PackageType>, List<PackageTypeDto>>(packageType);
                return packageTypeMap;
            }
        }

        #endregion

        #region Private Methods

        private async Task ValidatePackageTypeUniqunessAsync(CreateUpdatePackageTypeDto input)
        {
            var packageType = await _packageTypeRepository.FirstOrDefaultAsync(
                type =>
                    type.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                    type.ArabicName.ToLower().Contains(input.ArabicName.ToLower())
            );

            if (packageType != null)
            {
                if (packageType.EnglishName == input.EnglishName.Trim(' '))
                {
                    throw new InvalidOperationException("Package English name already exists");
                }
                else if (packageType.ArabicName == input.ArabicName.Trim(' '))
                {
                    throw new InvalidOperationException("Package Arabic name already exists");
                }
            }
        }

        #endregion
    }
}
