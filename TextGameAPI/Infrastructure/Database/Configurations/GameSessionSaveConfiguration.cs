using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.JSON;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class GameSessionSaveConfiguration : IEntityTypeConfiguration<GameSessionSave>
    {
        public void Configure(EntityTypeBuilder<GameSessionSave> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.GameSessionId).IsRequired();
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.StateHash).HasMaxLength(64);

            builder.Property(x => x.State)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => GameSessionStateSerializer.Serialize(v),
                    v => GameSessionStateSerializer.Deserialize(v)
                );
        }
    }
}