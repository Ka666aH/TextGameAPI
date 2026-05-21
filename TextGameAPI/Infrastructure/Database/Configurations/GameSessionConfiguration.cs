using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.JSON;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
    {
        public void Configure(EntityTypeBuilder<GameSession> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.LastSavedAt).IsRequired();

            builder.Property(x => x.State)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => GameSessionStateSerializer.Serialize(v),
                    v => GameSessionStateSerializer.Deserialize(v)
                );
        }
    }
}