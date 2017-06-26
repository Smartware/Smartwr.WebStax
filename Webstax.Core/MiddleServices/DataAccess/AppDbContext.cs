using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Smartwr.Webstax.Core.MiddleServices.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Smartwr.Webstax.Core.MiddleServices.Extensions;
using Webstax.Core.MiddleServices.Models;

namespace Smartwr.Webstax.Core.MiddleServices.DataAccess
{
    public abstract class AppDbContext : DbContext, IEntitiesContext, IDbContextFactory<DbContext>
    {
        private DbTransaction _transaction;
        public static IConfigurationRoot Configuration { get; set; }
        public String ConnectionName { get; set; }

        static AppDbContext()
        {
            var builder = new ConfigurationBuilder();
#if DEBUG
            builder.AddJsonFile($"appsettings.development.json");
#else
             builder.AddJsonFile("appsettings.json");
#endif
            Configuration = builder.Build();
        }

        public AppDbContext(String connectionName)
        {
            ConnectionName = connectionName;
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }


        public AppDbContext(DbContextOptions<DbContext> options, String connectionName)
            : base(options)
        {
            ConnectionName = connectionName;
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>()
                .UseSqlServer(Configuration.GetConnectionString(ConnectionName));

            return new DbContext(optionsBuilder.Options);
        }

        private static void ApplyStateUsingIsKeySet(EntityEntry entry)
        {
            if (entry.IsKeySet)
            {
                if (((ClientChangeTracker)entry.Entity).IsDirty)
                {
                    entry.State = EntityState.Modified;
                }
                else
                {
                    entry.State = EntityState.Unchanged;
                }
            }
            else
            {
                entry.State = EntityState.Added;
            }
        }

        public IEnumerable<TElement> FromSql<TElement>(string sql, params object[] parameters) where TElement : BaseEntity, new()
        {
            return this.Database.GetModelFromQuery<TElement>(sql, parameters);
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public void SetAsAdded<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            UpdateEntityState(entity, EntityState.Added);
        }

        public void SetAsModified<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            UpdateEntityState(entity, EntityState.Modified);
        }

        public void SetAsDeleted<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            UpdateEntityState(entity, EntityState.Deleted);
        }

        public void BeginTransaction()
        {
            if (this.Database.GetDbConnection().State == ConnectionState.Open)
            {
                return;
            }
            this.Database.GetDbConnection().Open();
            _transaction = this.Database.GetDbConnection().BeginTransaction();
        }

        public int Commit()
        {
            var saveChanges = SaveChanges();
            _transaction.Commit();
            return saveChanges;
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public Task<int> CommitAsync()
        {
            var saveChangesAsync = SaveChangesAsync();
            _transaction.Commit();
            return saveChangesAsync;
        }

        private void UpdateEntityState<TEntity>(TEntity entity, EntityState entityState) where TEntity : BaseEntity
        {
            //this.ChangeTracker.TrackGraph(entity, e => ApplyStateUsingIsKeySet(e.Entry));
            var dbEntityEntry = GetDbEntityEntrySafely(entity);
            dbEntityEntry.State = entityState;
        }

        private EntityEntry GetDbEntityEntrySafely<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var dbEntityEntry = Entry<TEntity>(entity);
            if (dbEntityEntry.State == EntityState.Detached)
            {
                Set<TEntity>().Attach(entity);
            }
            return dbEntityEntry;
        }

        public override void Dispose()
        {
            //if (this.Database.GetDbConnection() != null && this.Database.GetDbConnection().State == ConnectionState.Open)
            //{
            //    this.Database.GetDbConnection().Close();
            //}

            base.Dispose();
        }

        /// <summary>
        /// Create database script
        /// </summary>
        /// <returns>SQL to generate database</returns>
        public string CreateDatabaseScript()
        {
            return this.CreateDatabaseScript();
        }

        /// <summary>
        /// Attach an entity to the context or return an already attached entity (if it was already attached)
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Attached entity</returns>
        protected virtual TEntity AttachEntityToContext<TEntity>(TEntity entity) where TEntity : BaseEntity, new()
        {
            //little hack here until Entity Framework really supports stored procedures
            //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
            var alreadyAttached = Set<TEntity>().Local.FirstOrDefault(x => x.Id == entity.Id);
            if (alreadyAttached == null)
            {
                //attach new entity
                Set<TEntity>().Attach(entity);
                return entity;
            }
            else
            {
                //entity is already loaded.
                return alreadyAttached;
            }
        }

        /// <summary>
        /// Execute stores procedure and load a list of entities at the end
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="commandText">Command text</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Entities</returns>
        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            //add parameters to command
            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i <= parameters.Length - 1; i++)
                {
                    var p = parameters[i] as DbParameter;
                    if (p == null)
                        throw new Exception("Not support parameter type");

                    commandText += i == 0 ? " " : ", ";

                    commandText += "@" + p.ParameterName;
                    if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output)
                    {
                        //output parameter
                        commandText += " output";
                    }
                }
            }

            var result = this.SqlQuery<TEntity>(commandText, parameters).ToList();

            for (int i = 0; i < result.Count; i++)
                result[i] = AttachEntityToContext(result[i]);

            return result;
        }

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given generic type.  The type can be any type that has properties that match the names of the columns returned from the query, or can be a simple primitive type. The type does not have to be an entity type. The results of this query are never tracked by the context even if the type of object returned is an entity type.
        /// </summary>
        /// <typeparam name="TElement">The type of object returned by the query.</typeparam>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="parameters">The parameters to apply to the SQL query string.</param>
        /// <returns>Result</returns>
        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters) where TElement : BaseEntity
        {
            return base.Set<TElement>().FromSql<TElement>(sql, parameters).AsEnumerable();
        }

        /// <summary>
        /// Executes the given DDL/DML command against the database.
        /// </summary>
        /// <param name="sql">The command string</param>
        /// <param name="doNotEnsureTransaction">false - the transaction creation is not ensured; true - the transaction creation is ensured.</param>
        /// <param name="timeout">Timeout value, in seconds. A null value indicates that the default value of the underlying provider will be used</param>
        /// <param name="parameters">The parameters to apply to the command string.</param>
        /// <returns>The result returned by the database after executing the command.</returns>
        public int ExecuteSqlCommand(string sql, int? timeout = null, params object[] parameters)
        {
            var result = this.Database.ExecuteSqlCommand(sql, parameters);
            return result;
        }


    }
}
