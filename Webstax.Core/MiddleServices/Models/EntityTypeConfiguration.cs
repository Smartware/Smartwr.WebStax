using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Smartwr.Webstax.Core.MiddleServices.Models.Auth
{
    public interface IEntityMappingConfiguration
    {
        void Map(ModelBuilder builder);
    }

    public interface IEntityMappingConfiguration<T> : IEntityMappingConfiguration where T : class
    {
        void Map(EntityTypeBuilder<T> builder);
    }

    public abstract class EntityTypeConfiguration<TEntity> : IEntityMappingConfiguration<TEntity>
         where TEntity : class
    {
        public EntityTypeConfiguration()
        {
        }

        public abstract void Map(EntityTypeBuilder<TEntity> builder);

        public void Map(ModelBuilder b)
        {
            Map(b.Entity<TEntity>());
        }
    }

    public static class ModelBuilderExtensions
    {
        private static IEnumerable<Type> GetMappingTypes(this Assembly assembly, Type mappingInterface)
        {
            return assembly.GetTypes().Where(x => !x.GetTypeInfo().IsAbstract && x.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType && y.GetGenericTypeDefinition() == mappingInterface));
        }

        public static void AddEntityConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var mappingTypes = assembly.GetMappingTypes(typeof(IEntityMappingConfiguration<>));
            foreach (var config in mappingTypes.Select(Activator.CreateInstance).Cast<IEntityMappingConfiguration>())
            {
                config.Map(modelBuilder);
            }
        }
    }
}
