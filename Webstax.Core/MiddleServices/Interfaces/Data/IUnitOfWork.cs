using Smartwr.Webstax.Core.MiddleServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smartwr.Webstax.Core.MiddleServices.Interfaces.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IEnumerable<TEntity> FromSql<TEntity>(String sql, params object[] parameters) 
            where TEntity : BaseEntity, new();
        int SaveChanges();
        void Dispose(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        void BeginTransaction();
        int Commit();
        void Rollback();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> CommitAsync();
    }
}
