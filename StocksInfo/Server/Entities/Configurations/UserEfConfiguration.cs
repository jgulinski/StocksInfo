using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models;

namespace Server.Entities.Configurations;

public class UserEfConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.IdUser).HasName("User_pk");
        builder.Property(e => e.IdUser).UseIdentityColumn();

        builder.Property(e => e.EmailAddress).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Password).IsRequired().HasMaxLength(256);
        builder.Property(e => e.RefreshToken).IsRequired(false);
        builder.Property(e => e.RefreshTokenExpirationDateTime).IsRequired(false);

        builder.ToTable("User");
    }
}