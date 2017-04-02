using System;
using System.Data.Entity;

namespace GenderPayGap.Core.Interfaces
{
    public interface IDbContext : IDisposable
    {
        IDbSet<TEntity> Set<TEntity>() where TEntity : class;
        int SaveChanges();

        System.Data.Entity.Database GetDatabase();
    }
}
