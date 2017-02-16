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
        public static DbContext Default = new DbContext();

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

            if (tables == null || tables.Length == 0)
            {
                target = GetTableList(DbContext.Default);
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
                        DbContext.Default.Database.ExecuteSqlCommand(string.Format("TRUNCATE TABLE [{0}]", table));
                        result++;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            DbContext.Default.Database.ExecuteSqlCommand(string.Format("DELETE [{0}]", table));
                            result++;

                            try
                            {
                                DbContext.Default.Database.ExecuteSqlCommand(string.Format("DBCC CHECKIdENT ([{0}], RESEED, 1)", table));
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
            DbContext.Default.SaveChanges();
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

        public static void RefreshAll()
        {
            Default = new DbContext();
        }

        public override int SaveChanges()
        {
            int c = 0;
            retry:
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                c++;
                Thread.Sleep(1000);
                if (c<10)goto retry;
                throw;
            }
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();

        }
    }
}
