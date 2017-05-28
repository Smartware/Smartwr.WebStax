

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Smartwr.Webstax.lib.MiddleServices.Models.Auth
{
    public abstract class EntityTypeConfiguration<TEntity>
          where TEntity : class
    {
        public EntityTypeConfiguration()
        {
        }
        public abstract void Map(EntityTypeBuilder<TEntity> builder);
    }

    public static class ModelBuilderExtensions
    {
        public static void AddConfiguration<TEntity>(this ModelBuilder modelBuilder, EntityTypeConfiguration<TEntity> configuration)
            where TEntity : class
        {

            //configuration.Map(modelBuilder.Entity<TEntity>(builder => 
            //{
            //    builder.HasOne(r => r.CreatedBy)
            //    .WithMany()
            //    .HasForeignKey(r => r.CreatedBy_Id)
            //    .WillCascadeOnDelete(false);

            //    builder.HasOne(r => r.ModifiedBy)
            //    .WithMany()
            //    .HasForeignKey(r => r.ModifiedBy_Id)
            //    .WillCascadeOnDelete(false);
            //}));

            configuration.Map(modelBuilder.Entity<TEntity>());
        }
    }
}
