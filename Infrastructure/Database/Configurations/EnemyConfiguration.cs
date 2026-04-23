using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities.GameObjects.Enemies;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class EnemyConfiguration : IEntityTypeConfiguration<Enemy>
    {
        public void Configure(EntityTypeBuilder<Enemy> builder)
        {
            builder.ToTable("Enemies");

            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(250);

            builder.HasDiscriminator<string>("EnemyType")
                .HasValue<Skeletor>("Skeletor")
                .HasValue<SkeletorArcher>("SkeletorArcher")
                .HasValue<Deadman>("Deadman")
                .HasValue<Ghost>("Ghost")
                .HasValue<Lich>("Lich")
                .HasValue<Mimic>("Mimic");
        }
    }
}