using System.Data.Entity.Validation;

namespace GenderPayGap.Models.SqlDatabase
{
    using GenderPayGap.Core.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading;

    public partial class DbContext : System.Data.Entity.DbContext, IDbContext
    {
        public DbContext()
            : base("GpgDatabase")
        {
        }

        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<OrganisationAddress> OrganisationAddress { get; set; }
        public virtual DbSet<OrganisationStatus> OrganisationStatus { get; set; }
        public virtual DbSet<Return> Return { get; set; }
        public virtual DbSet<ReturnStatus> ReturnStatus { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserStatus> UserStatuses { get; set; }
        public virtual DbSet<UserOrganisation> UserOrganisations { get; set; }

        public static int Truncate(params string[] tables)
        {
            List<string> target = new List<string>();
            var context=new DbContext();
            if (tables == null || tables.Length == 0)
            {
                target = GetTableList(context);
            }
            else
            {
                target.AddRange(tables);
            }

            int result = 0;
            for (int i = 0; i < 10; i++)
            {
                result = 0;
                foreach (var table in target)
                {
                    try
                    {
                        context.Database.ExecuteSqlCommand(string.Format("TRUNCATE TABLE [{0}]", table));
                        result++;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            context.Database.ExecuteSqlCommand(string.Format("DELETE [{0}]", table));
                            result++;

                            try
                            {
                                context.Database.ExecuteSqlCommand(string.Format("DBCC CHECKIdENT ([{0}], RESEED, 1)", table));
                            }
                            catch (Exception ex1)
                            {

                            }
                        }
                        catch (Exception ex2)
                        {

                        }
                    }
                }
                if (result == target.Count()) break;
                if (result == 10) throw new Exception("Unable to truncate all tables");
            }
            context.SaveChanges();
            return result;
        }

        public static List<string> GetTableList(System.Data.Entity.DbContext db)
        {
            List<string> tableNames = db.Database.SqlQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%Migration%' AND TABLE_NAME NOT LIKE 'AspNet%'").ToList();
            return tableNames;
            var type = db.GetType();

            return db.GetType().GetProperties()
                .Where(x => x.PropertyType.Name == "DbSet`1")
                .Select(x => x.Name).ToList();
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException vex)
            {
                var innerExceptions=new List<ArgumentException>();
                
                foreach (var err in vex.EntityValidationErrors)
                {
                    foreach (var err1 in err.ValidationErrors)
                    {
                        innerExceptions.Add(new ArgumentException(err1.ErrorMessage,err1.PropertyName));
                    }
                }
                throw new AggregateException(innerExceptions);
            }
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();

        }
    }
}
