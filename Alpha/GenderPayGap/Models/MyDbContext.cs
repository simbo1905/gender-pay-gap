using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GenderPayGap.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() : base("gpgsql.GPGDB.dbo")
        {
            
        }

        public DbSet<MyReturn> MyReturns { get; set; }
    }
}