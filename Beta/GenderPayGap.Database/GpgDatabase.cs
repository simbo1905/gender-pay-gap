using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Validation;
using Extensions;

namespace GenderPayGap.Database
{
    using GenderPayGap.Core.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    public partial class DbContext : System.Data.Entity.DbContext, IDbContext
    {
        static DbContext()
        {
            Database.SetInitializer(new SicCodeInitialiser());
        }
        public DbContext()
            : base("GpgDatabase")
        {
        }

        public static bool EncryptEmails = ConfigurationManager.AppSettings["EncryptEmails"].ToBoolean(true);

        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<OrganisationAddress> OrganisationAddress { get; set; }
        public virtual DbSet<AddressStatus> AddressStatus { get; set; }
        public virtual DbSet<OrganisationStatus> OrganisationStatus { get; set; }
        public virtual DbSet<Return> Return { get; set; }
        public virtual DbSet<ReturnStatus> ReturnStatus { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserStatus> UserStatuses { get; set; }
        public virtual DbSet<UserOrganisation> UserOrganisations { get; set; }
        public virtual DbSet<SicCode> SicCodes { get; set; }
        public virtual DbSet<SicSection> SicSections { get; set; }
        public virtual DbSet<OrganisationSicCode> OrganisationSicCodes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Turn off cascade on delete
            //modelBuilder.Entity<UserOrganisation>()
            //    .HasRequired(f => f.Organisation)
            //    .WithRequiredDependent()
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<OrganisationSicCode>()
            //    .HasRequired(f => f.SicCode)
            //    .WithRequiredDependent()
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<SicCode>()
           .HasRequired(d => d.SicSection)
           .WithMany(w => w.SicCodes)
           .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
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

        public Database GetDatabase()
        {
            return Database;
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();

        }

        public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (Database.Connection.State != ConnectionState.Open)Database.Connection.Open();
            return Database.Connection.BeginTransaction(isolationLevel);
        }

        #region TEST CODE ONLY
#if DEBUG || TEST

        public static int Truncate(params string[] tables)
        {
            var target = new List<string>();
            var context = new DbContext();
            if (tables == null || tables.Length == 0)
            {
                target = GetTableList(context);
            }
            else
            {
                target.AddRange(tables);
            }

            var targetCount = target.Count();
            var result = 0;
            for (var i = 0; i < 10; i++)
            {
                result = 0;
                foreach (var table in target)
                {
                    //Dont delete the sic codes/sections
                    if (table.EqualsI("SicCodes", "SicSections"))
                    {
                        targetCount--;
                        continue;
                    }
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
                if (result == targetCount) break;
                if (result == 10) throw new Exception("Unable to truncate all tables");
            }
            context.SaveChanges();
            return result;
        }

        public static List<string> GetTableList(System.Data.Entity.DbContext db)
        {
            var tableNames = db.Database.SqlQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%Migration%' AND TABLE_NAME NOT LIKE 'AspNet%'").ToList();
            return tableNames;
            var type = db.GetType();

            return db.GetType().GetProperties()
                .Where(x => x.PropertyType.Name == "DbSet`1")
                .Select(x => x.Name).ToList();
        }
#endif
        #endregion

        public static void Delete(long userId, bool deleteReturns, bool deleteOrg, bool deleteUser)
        {
            var context = new DbContext();
            var user = context.User.FirstOrDefault(u => u.UserId == userId);
            var orgUser = context.UserOrganisations.FirstOrDefault(uo => uo.UserId == userId);
            if (orgUser != null)
            {
                var org = context.Organisation.FirstOrDefault(o => o.OrganisationId == orgUser.OrganisationId);
                if (org != null)
                {
                    if (deleteOrg || deleteUser)
                    {
                        var addresses = context.OrganisationAddress.Where(a => a.OrganisationId == org.OrganisationId).ToList();
                        foreach (var address in addresses)
                            context.AddressStatus.RemoveRange(address.AddressStatuses);

                        context.OrganisationAddress.RemoveRange(addresses);
                    }
                    if (deleteOrg || deleteUser || deleteReturns)
                    {
                        var returns = context.Return.Where(a => a.OrganisationId == org.OrganisationId).ToList();

                        foreach (var @return in returns)
                            context.ReturnStatus.RemoveRange(@return.ReturnStatuses);

                        context.Return.RemoveRange(returns);
                    }

                    if (deleteOrg || deleteUser)
                    {
                        context.OrganisationStatus.RemoveRange(org.OrganisationStatuses);
                        context.Organisation.Remove(org);
                    }
                }
                if (deleteOrg || deleteUser)
                    context.UserOrganisations.Remove(orgUser);
            }
            if (user != null && deleteUser)
            {

                context.UserStatuses.RemoveRange(user.UserStatuses);
                context.User.Remove(user);
            }
            context.SaveChanges();
        }

        public static void DeleteReturns(long userId)
        {
            Delete(userId, true, false, false);
        }

        public static void DeleteOrganisations(long userId)
        {
            Delete(userId, true, true, false);
        }

        public static void DeleteAccount(long userId)
        {
            Delete(userId, true, true, true);
        }

    }
}
