using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
    {
        public void Configure(EntityTypeBuilder<GameSession> builder)
        {
            builder.HasMany(x => x.Rooms)
                .WithOne()
                .HasForeignKey("GameSessionId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Inventory)
                .WithOne()
                .HasForeignKey("GameSessionId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CurrentRoom)
               .WithOne()
               .HasForeignKey<GameSession>(x => x.CurrentRoomId)
               .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Weapon)
               .WithOne()
               .HasForeignKey<GameSession>(x => x.WeaponId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Helm)
               .WithOne()
               .HasForeignKey<GameSession>(x => x.HelmId)
               .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Chestplate)
               .WithOne()
               .HasForeignKey<GameSession>(x => x.ChestplateId)
               .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.CurrentMimicChest)
               .WithOne()
               .HasForeignKey<GameSession>(x => x.CurrentMimicChestId)
               .OnDelete(DeleteBehavior.SetNull);
        }
    }
}