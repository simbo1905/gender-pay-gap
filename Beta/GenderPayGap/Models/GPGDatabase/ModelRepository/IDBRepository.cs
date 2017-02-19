using System.Collections.Generic;
using GenderPayGap.Models.GpgDatabase;


namespace GenderPayGap.Models.GPGDatabase.ModelRepository
{
    //Generic version
    public interface IDBRepository<T>
    {
        void CreateNewObject(T objectToCreate);
        void DeleteObject(int id);
        T GetObjectByID(int id);
        IEnumerable<T> GetAllUsers();
        int SaveChanges();
    }

}