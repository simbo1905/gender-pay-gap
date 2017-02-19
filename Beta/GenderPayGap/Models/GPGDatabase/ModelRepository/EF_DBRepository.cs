using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GenderPayGap.Models.GpgDatabase;



namespace GenderPayGap.Models.GPGDatabase.ModelRepository
{
    public class EF_DBRepository :  IDBUserRepository
    {
        private static GpgDatabase.GpgDatabase _dbContext = new GpgDatabase.GpgDatabase();
        User currUser = GpgDatabase.GpgDatabase.Default.User.FirstOrDefault();

        public void CreateNewUser(User userToCreate)
        {
            _dbContext.User.Add(userToCreate);
            _dbContext.SaveChanges();
            // return contactToCreate;
        }

        public void DeleteUser(int id)
        {
            var userToDel = GetUserByID(id);
            _dbContext.User.Remove(userToDel);
            _dbContext.SaveChanges();
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _dbContext.User.ToList();
        }

        public User GetUserByID(int id)
        {
             return _dbContext.User.FirstOrDefault(d => d.UserId == id);
        }

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }
    }
}