using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenderPayGap.Models.GpgDatabase;
using GenderPayGap.Models.GPGDatabase.ModelRepository;


namespace GenderPayGap.Tests.DBRespository
{
    public class TestEF_DBRepository : IDBUserRepository
    {
        private List<User> _db = new List<User>();
        public Exception ExceptionToThrow { get; set; }
        //public List<User> Items { get; set; }

        public void SaveChanges(User userToUpdate)
        {
            foreach (User user in _db)
            {
                if (user.UserId == userToUpdate.UserId)
                {
                    _db.Remove(user);
                    _db.Add(userToUpdate);
                    break;
                }
            }
        }
        public void Add(User contactToAdd)
        {
            _db.Add(contactToAdd);
        } public User GetUserByID(int id)
        {
            return _db.FirstOrDefault(d => d.UserId == id);
        }
        public void CreateNewUser(User userToCreate)
        {
            if (ExceptionToThrow != null)
                throw ExceptionToThrow;
            _db.Add(userToCreate);
            // return contactToCreate; 
        }
        public int SaveChanges() { return 1; }
        public IEnumerable<User> GetAllUsers()
        {
            return _db.ToList();
        }
        public void DeleteUser(int id)
        {
            _db.Remove(GetUserByID(id));
        }
    }

    }