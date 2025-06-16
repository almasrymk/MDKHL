using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Cameras.Watchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Configurations;

internal class DispenserConfiguration : IEntityTypeConfiguration<Dispenser>
{
    public void Configure(EntityTypeBuilder<Dispenser> builder)
    {
        builder.ToTable("Dispensers");
    }
}
