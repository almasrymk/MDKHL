using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Payments;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations;

public class PosTerminalTransactionDetailConfiguration : IEntityTypeConfiguration<PosTerminalTransactionDetail>
{
    public void Configure(EntityTypeBuilder<PosTerminalTransactionDetail> builder)
    {
        builder.ConfigureByConvention();
        
        builder.ToTable("PosTerminalTransactionDetails");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Payment)
            .WithOne(p => p.PosTerminalDetail)
            .HasForeignKey<PosTerminalTransactionDetail>(x => x.PaymentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PaymentId).IsUnique();

        builder.Property(x => x.SamaResponseCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Outcome).IsRequired();
        builder.Property(x => x.RawJsonResult).IsRequired();
        builder.Property(x => x.TerminalId).HasMaxLength(50);
        builder.Property(x => x.TransactionStan).HasMaxLength(6);
        builder.Property(x => x.RetrievalReferenceNumber).HasMaxLength(12);
        builder.Property(x => x.AuthCode).HasMaxLength(20);
        builder.Property(x => x.MaskedPan).HasMaxLength(20);
        builder.Property(x => x.CardScheme).HasMaxLength(50);
    }
}
