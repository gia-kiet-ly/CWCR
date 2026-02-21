using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        Task SaveAsync();
        void Save();
        void BeginTransaction();
        Task BeginTransactionAsync();
        void CommitTransaction();
        Task CommitTransactionAsync();
        void RollBack();
        Task<bool> IsValidAsync<T>(Guid id) where T : BaseEntity;
        Task RollbackTransactionAsync();

        /// <summary>
        /// Executes raw SQL command and returns the number of rows affected.
        /// Useful for atomic operations like UPDATE with WHERE conditions.
        /// </summary>
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    }
}
