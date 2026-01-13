using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySecrets.Domain.Entities;

namespace MySecrets.Infrastructure.Persistence.Configurations;

public class SecretConfiguration : IEntityTypeConfiguration<Secret>
{
    public void Configure(EntityTypeBuilder<Secret> builder)
    {
        builder.ToTable("Secrets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.WebsiteUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(s => s.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.EncryptedPassword)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(s => s.Notes)
            .HasMaxLength(4000);

        builder.Property(s => s.Category)
            .HasMaxLength(100);

        builder.HasIndex(s => s.UserId);

        builder.HasIndex(s => new { s.UserId, s.Category });

        builder.HasIndex(s => new { s.UserId, s.IsFavorite });
    }
}
