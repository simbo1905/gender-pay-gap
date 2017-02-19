using System.Collections.Generic;
using GenderPayGap.Models.GpgDatabase;


namespace GenderPayGap.Models.GPGDatabase.ModelRepository
{
    public interface IDBUserRepository 
    {
        void CreateNewUser(User userToCreate);
        void DeleteUser(int id);
        User GetUserByID(int id);
        IEnumerable<User> GetAllUsers();
        int SaveChanges();
    }

    
}