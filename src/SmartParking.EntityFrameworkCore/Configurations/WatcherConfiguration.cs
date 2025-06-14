using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Cameras.Watchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Configurations;

internal class WatcherConfiguration : IEntityTypeConfiguration<Watcher>
{
    public void Configure(EntityTypeBuilder<Watcher> builder)
    {
       builder.UseTpcMappingStrategy();
    }
}
