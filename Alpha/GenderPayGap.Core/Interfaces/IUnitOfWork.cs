using System;

namespace GenderPayGap.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void CommitTransaction();
        void StartTransaction();
    }
}
