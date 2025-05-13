using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;


namespace SmartParking.Lookups.PlateColor;

    public interface IPlateColorsAppService : ICrudAppService<PlateColorDto, int, PagedAndSortedResultRequestDto, CreateUpdatePlateColorDto>
    {
    /// <summary>
    /// Abstract method to create a plate color.
    /// </summary>
    /// <param name="input">The plate color create DTO containing the details for the new plate color.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the created plate color DTO.</returns>
    public Task<PlateColorDto> CreateAsync(CreateUpdatePlateColorDto input);

    /// <summary>
    /// Abstract method to update a specific plate color.
    /// </summary>
    /// <param name="input">The plate color create DTO containing the updated details.</param>
    /// <param name="id">The ID of the specific plate color to be updated.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the updated plate color DTO.</returns>
    public Task<PlateColorDto> UpdateAsync(int id, CreateUpdatePlateColorDto input);

    /// <summary>
    /// Abstract method to search for plate colors.
    /// </summary>
    /// <param name="input">The search input DTO specifying search criteria, sorting, and maximum results.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a paged list of plate color DTOs.</returns>
    public Task<PagedResultDto<PlateColorDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter plate colors.
    /// </summary>
    /// <param name="input">The filter input DTO specifying filter criteria, sorting, and maximum results.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a paged list of plate color DTOs.</returns>
    public Task<PagedResultDto<PlateColorDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to delete a specific plate color.
    /// </summary>
    /// <param name="id">The ID of the plate color to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(int id);

    /// <summary>
    /// Abstract method to get a list of all plate colors.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a result of a list of plate color DTOs.</returns>
    public Task<List<PlateColorDto>> GetAllPlateColorsAsync();

}
