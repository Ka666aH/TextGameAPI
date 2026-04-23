using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities.GameObjects.Items.Other;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class ChestConfiguration : IEntityTypeConfiguration<Chest>
    {
        public void Configure(EntityTypeBuilder<Chest> builder)
        {
            builder.HasOne(x => x.Mimic)
                .WithOne()
                .HasForeignKey<Chest>(x => x.MimicId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Items)
                .WithOne()
                .HasForeignKey("ChestId")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}