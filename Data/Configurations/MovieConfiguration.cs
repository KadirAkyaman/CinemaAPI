using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(50);

        builder.Property(m => m.Description).HasMaxLength(250);

        builder.Property(m => m.Genre).HasMaxLength(25);

        
        builder.HasOne(m => m.Director).WithMany(d => d.Movies).HasForeignKey(m => m.DirectorId).OnDelete(DeleteBehavior.SetNull);
    }
}