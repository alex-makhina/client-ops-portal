using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AddressValidator.Domain.Entities;

namespace AddressValidator.Infrastructure.Persistence.Configurations;

public sealed class AddressObjectConfiguration : IEntityTypeConfiguration<AddressObject>
{
    public void Configure(EntityTypeBuilder<AddressObject> builder)
    {
        builder.ToTable("address_objects");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();  

        builder.Property(a => a.ParentId)
               .HasColumnName("parent_id")
               .HasColumnType("uuid");

        builder.HasOne(a => a.Parent)
               .WithMany(a => a.Children)
               .HasForeignKey(a => a.ParentId)
               .OnDelete(DeleteBehavior.Restrict);  

        builder.HasIndex(a => a.ParentId)
               .HasDatabaseName("idx_address_objects_parent");

        builder.Property(a => a.OsmId)
               .HasColumnName("osm_id");
        builder.Property(a => a.OsmType)
               .HasColumnName("osm_type")
               .HasMaxLength(16)
               .IsRequired();
        builder.HasIndex(a => new { a.OsmId, a.OsmType })
               .IsUnique()
               .HasDatabaseName("idx_address_objects_osm_unique");

        builder.Property(a => a.Name)
               .HasColumnName("name")
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(a => a.Type)
               .HasColumnName("type")
               .HasConversion<string>()
               .HasMaxLength(32)
               .IsRequired();
        builder.HasIndex(a => a.Type)
               .HasDatabaseName("idx_address_objects_type");

        builder.Property(a => a.FullPath)
               .HasColumnName("full_path")
               .HasMaxLength(1024)
               .IsRequired();

        builder.HasIndex(a => a.FullPath)
               .HasDatabaseName("idx_address_objects_full_path_trgm")
               .HasMethod("gin")
               .HasOperators("gin_trgm_ops");

        builder.Property(a => a.Geom)
               .HasColumnName("geom")
               .HasColumnType("geometry(Point, 4326)");


        builder.Property(a => a.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");
        builder.Property(a => a.UpdatedAt)
               .HasColumnName("updated_at")
               .HasDefaultValueSql("now()");
    }
}
