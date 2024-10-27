using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Events.Distributed;

namespace SmartParking;

public interface ICascadeSoftDeleteHandler<T>
{
    public Task HandleEventAsync(EntityDeletedEto<T> eventData);
}
