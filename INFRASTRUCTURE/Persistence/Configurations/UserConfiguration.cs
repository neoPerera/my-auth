using CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INFRASTRUCTURE.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("tbl_users");
        
        // Primary key configuration
        builder.HasKey(u => u.Id);

        // Map properties to database columns with required constraints
        builder.Property(u => u.Username)
            .HasColumnName("str_user_name")
            .IsRequired();

        builder.Property(u => u.Password)
            .HasColumnName("str_password")
            .IsRequired();

        // Configure a unique index on the Username column
        builder.HasIndex(u => u.Username)
            .IsUnique();
    }
}

