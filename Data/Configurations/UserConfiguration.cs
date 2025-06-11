using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username).IsRequired().HasMaxLength(20);
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");

        builder.Property(u => u.Email).IsRequired().HasMaxLength(30);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.Property(u => u.IsActive).HasDefaultValue(true);
    }
}