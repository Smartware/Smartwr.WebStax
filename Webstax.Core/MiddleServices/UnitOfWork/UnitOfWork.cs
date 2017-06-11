using Smartwr.Webstax.Core.MiddleServices.DataAccess;
using Smartwr.Webstax.Core.MiddleServices.Interfaces.Data;
using Smartwr.Webstax.Core.MiddleServices.Models;
using Smartwr.Webstax.Core.MiddleServices.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smartwr.Webstax.Core.MiddleServices.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IEntitiesContext _context;
        private bool _disposed;
        private Dictionary<String, object> _repositories;

        public UnitOfWork(IEntitiesContext context)
        {
            _context = context;
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public virtual IEnumerable<TEntity> FromSql<TEntity>(String sql, params object[] parameters) where TEntity: BaseEntity, new()
        {
            return _context.FromSql<TEntity>(sql, parameters);
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            
            if (_repositories == null)
            {
                _repositories = new Dictionary<String, object>();
            }

            var type = typeof(TEntity).Name;
            if (_repositories.ContainsKey(type))
            {
                return (IRepository<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(EntityRepository<>);

            _repositories.Add(type, Activator.CreateInstance(typeof(EntityRepository<>).MakeGenericType(typeof(TEntity)), _context));

            return (IRepository<TEntity>)_repositories[type];
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public void BeginTransaction()
        {
            _context.BeginTransaction();
        }

        public int Commit()
        {
            return _context.Commit();
        }

        public void Rollback()
        {
            _context.Rollback();
        }

        public Task<int> CommitAsync()
        {
            return _context.CommitAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                foreach (IDisposable repository in _repositories.Values)
                {
                    repository.Dispose();// dispose all repositries
                }
            }
            _disposed = true;
        }
    }
}
