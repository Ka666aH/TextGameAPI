using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");

            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(250);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Room)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasMany(x => x.Enemies)
                .WithOne(x => x.Room)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasDiscriminator<string>("RoomType")
                .HasValue<StartRoom>("StartRoom")
                .HasValue<EndRoom>("EndRoom")
                .HasValue<EmptyRoom>("EmptyRoom")
                .HasValue<SmallRoom>("SmallRoom")
                .HasValue<BigRoom>("BigRoom")
                .HasValue<Shop>("Shop");
        }
    }
}