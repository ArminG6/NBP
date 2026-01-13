using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySecrets.Domain.Entities;

namespace MySecrets.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.RevokedReason)
            .HasMaxLength(256);

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(256);

        builder.HasIndex(rt => rt.UserId);

        builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiresAt });
    }
}
