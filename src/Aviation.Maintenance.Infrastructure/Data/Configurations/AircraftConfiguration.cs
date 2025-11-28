using Aviation.Maintenance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aviation.Maintenance.Infrastructure.Data.Configurations;

public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.ToTable("Aircraft");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TailNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(a => a.Model)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.HasMany(a => a.WorkOrders)
            .WithOne(w => w.Aircraft)
            .HasForeignKey(w => w.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
