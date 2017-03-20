using GenderPayGap.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenderPayGap.Core.Classes
{
    public class SqlRepository : IRepository
    {
        private readonly IDbContext context;

        public SqlRepository(IDbContext context)
        {
            this.context = context;
        }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return GetEntities<TEntity>().AsQueryable();
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Add(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Remove(entity);
        }

        private IDbSet<TEntity> GetEntities<TEntity>() where TEntity : class
        {
            return this.context.Set<TEntity>();
        }

        public void SaveChanges()
        {
            this.context.SaveChanges();
        }

        public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var database = context.GetDatabase();
            if (database.Connection.State != ConnectionState.Open)database.Connection.Open();
            return database.Connection.BeginTransaction(isolationLevel);
        }

        public void Dispose()
        {
            if (this.context != null)
            {
                this.context.Dispose();
            }
        }
    }

}
