using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using SmartParking.Entities;
using System.Reflection;

namespace SmartParking;

public  class CascadeSoftDeleteHandler<T> : ICascadeSoftDeleteHandler<T>
{
    private readonly IServiceProvider _serviceProvider;

    public  CascadeSoftDeleteHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public  async Task HandleEventAsync(EntityDeletedEto<T> eventData)
    {
        var entityType = eventData.Entity.GetType();

        // Resolve repositories dynamically
        var dependentProperties = entityType.GetProperties()
            .Where(p => typeof(IEnumerable<ISoftDelete>).IsAssignableFrom(p.PropertyType))
            .ToList();

        foreach (var property in dependentProperties)
        {
            var dependentEntities = property.GetValue(eventData.Entity) as IEnumerable<ISoftDelete>;
            if (dependentEntities == null) continue;
            foreach (var dependentEntity in dependentEntities)
            {
                var entityDependent = dependentEntity.GetType().BaseType ?? dependentEntity.GetType();
                // Resolve the repository dynamically
                // Resolve the repository for the specific type of the dependent entity
                var repositoryType = typeof(IRepository<>).MakeGenericType(entityDependent);
                var repository = _serviceProvider.GetService(repositoryType)  ;

                if (repository != null)
                {
                    var softDeleteMethod = repository.GetType().GetMethod("DeleteAsync", new[] { dependentEntity.GetType() });
                    //var softDeleteMethod = repositoryType.GetMethod("DeleteAsync");
                    if (softDeleteMethod != null)
                    {
                        await (Task)softDeleteMethod.Invoke(repository, new[] { dependentEntity });
                    }
                }

            }
        }
    }
    
}

