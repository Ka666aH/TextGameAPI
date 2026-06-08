using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.JSON;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class SaveConfiguration : IEntityTypeConfiguration<Save>
    {
        public void Configure(EntityTypeBuilder<Save> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SessionId).IsRequired();
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.StateHash).HasMaxLength(64);

            builder.Property(x => x.State)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => StateSerializer.Serialize(v),
                    v => StateSerializer.Deserialize(v)
                );
        }
    }
}