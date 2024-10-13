using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace SmartParking.Repositories;

public interface IMyUserRepository : IRepository<IdentityUser, Guid>
{
    Task<List<IdentityUser>> FindByOrganizationAsync(int oraganizationId);
}
