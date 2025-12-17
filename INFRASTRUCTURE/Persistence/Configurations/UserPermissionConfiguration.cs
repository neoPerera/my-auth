using CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace INFRASTRUCTURE.Persistence.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("tbl_user_permissions");
        
        // Primary key configuration
        builder.HasKey(up => up.Id);

        // Map properties to database columns
        builder.Property(up => up.UserId)
            .HasColumnName("int_user_id")
            .IsRequired();

        builder.Property(up => up.PermissionId)
            .HasColumnName("int_permission_id")
            .IsRequired();

        // Configure foreign keys
        builder.HasOne(up => up.User)
            .WithMany(u => u.UserPermissions)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_tbl_user_permissions_user");

        builder.HasOne(up => up.Permission)
            .WithMany(p => p.UserPermissions)
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_tbl_user_permissions_permission");

        // Configure unique index on UserId and PermissionId combination
        builder.HasIndex(up => new { up.UserId, up.PermissionId })
            .IsUnique()
            .HasDatabaseName("UX_tbl_user_permissions_user_permission");
    }
}

