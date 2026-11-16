using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);
            // Token = SHA-256 (64 hex-символа) — сырой токен в БД не хранится (notes/AGENTS.md §1b, п.8).
            builder.Property(x => x.Token).HasMaxLength(512);
            builder.Property(x => x.HashedFingerprint).HasMaxLength(60);

            builder.HasIndex(x => x.Token).IsUnique();
        }
    }
}