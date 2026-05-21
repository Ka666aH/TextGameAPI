using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System.Text.Json;
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
                .HasConversion(new GameSessionStateConverter());
            //.HasConversion(
            //v => JsonSerializer.SerializeToUtf8Bytes<GameSessionState>(v, JSON.Options.GameObjectsSerializeOptions),
            //v => JsonSerializer.Deserialize<GameSessionState>(v, JSON.Options.GameObjectsSerializeOptions) ?? new GameSessionState());

        }
    }
}