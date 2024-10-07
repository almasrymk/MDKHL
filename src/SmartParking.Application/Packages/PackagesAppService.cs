
using AutoMapper.Internal.Mappers;
using Microsoft.EntityFrameworkCore;
using SmartParking.Bases;
using SmartParking.Entities.Organizations;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.DefaultPackages
{
	public class PackagesAppService : BaseApplicationService, IPackagesAppService
	{
		private readonly IRepository<Package, int> _packageRepository;
		public PackagesAppService(IRepository<Package, int> packageRepository)
		{
			_packageRepository = packageRepository;
		}


		public async Task<PackageDto> CreateAsync(CreateUpdatePackageDto input)
		{
			var validationResult = new CreateUpdatePackageDtoValidator().Validate(input);
			if (!validationResult.IsValid)
			{
				var exception = GetValidationException(validationResult);
				throw exception;
			}
			var package = ObjectMapper.Map<CreateUpdatePackageDto, Package>(input);
			var inserted = await _packageRepository.InsertAsync(package, autoSave: true);
			return ObjectMapper.Map<Package, PackageDto>(inserted);
		}

		public Task<PackageDto> GetAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<PagedResultDto<PackageDto>> GetbyOrganizationAsync(PackageListDto input, int OrgnizationId)
		{
			throw new NotImplementedException();
		}

		public async Task<PagedResultDto<PackageDto>> GetListAsync(PackageListDto input)
		{
			if (input.Sorting.IsNullOrWhiteSpace())
				input.Sorting = nameof(Package.Id);
			var packages = await _packageRepository.WithDetailsAsync(_ => _.Organization)
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
				? await _packageRepository.CountAsync()
				: await _packageRepository.CountAsync(_ => _.Organization.ArabicName.Contains(input.Filter)
															   || _.Organization.EnglishName.Contains(input.Filter));

			return new PagedResultDto<PackageDto>(
				totalCount,
				ObjectMapper.Map<List<Package>, List<PackageDto>>(packages));
		}
		///<inheritdoc>/>
        [RemoteService(IsEnabled = false)]
        public async Task<List<PackageDto>> GetListPackageByOrganizationId(int organizationId)
        {
			var query = await _packageRepository.GetQueryableAsync();
			var pakages =await query.Where(_ => _.OrgnizationId == organizationId).ToListAsync();
			var pakagesMap = ObjectMapper.Map<List<Package>, List<PackageDto>>(pakages);
			return pakagesMap;
        }

        public Task<PackageDto> UpdateAsync(int id, CreateUpdatePackageDto input)
		{
			throw new NotImplementedException();
		}
	}

}
