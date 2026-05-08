using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.Property(x => x.Token).HasMaxLength(512);
            builder.Property(x => x.HashedFingerprint).HasMaxLength(60);

            builder.HasIndex(x => x.Token).IsUnique();
        }
    }
}