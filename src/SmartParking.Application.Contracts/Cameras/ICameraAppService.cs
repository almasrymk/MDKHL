using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Cameras;

public interface ICameraAppService:
     ICrudAppService<
        CameraDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCameraDto
        >
{
        Task<PagedResultDto<CameraDto>> GetListAsync(PagedAndSortedResultRequestDto input);
        Task<CameraDto> UpdateAsync(int id, CreateUpdateCameraDto input);

        Task<CameraDto> GetAsync(int id);
        Task DeleteAsync(int id);

}
