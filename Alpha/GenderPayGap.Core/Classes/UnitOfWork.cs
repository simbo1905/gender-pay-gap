using GenderPayGap.Core.Interfaces;
using System;
using System.Transactions;

namespace GenderPayGap.Core.Classes
{
    public class UnitOfWork : IDisposable
    {
        private TransactionScope transaction;

        public void StartTransaction()
        {
            this.transaction = new TransactionScope();
        }

        public void CommitTransaction()
        {
            this.transaction.Complete();
        }

        public void Dispose()
        {
            this.transaction.Dispose();
        }
    }
}
