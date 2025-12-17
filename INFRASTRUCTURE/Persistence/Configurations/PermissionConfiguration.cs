using CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INFRASTRUCTURE.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("tbl_permissions");
        
        // Primary key configuration
        builder.HasKey(p => p.Id);

        // Map properties to database columns
        builder.Property(p => p.PermissionName)
            .HasColumnName("str_permission")
            .IsRequired();

        builder.Property(p => p.PermissionDescription)
            .HasColumnName("str_permission_description");

        // Configure unique index on PermissionName
        builder.HasIndex(p => p.PermissionName)
            .IsUnique()
            .HasDatabaseName("IX_tbl_permissions_str_permission");
    }
}

