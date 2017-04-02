using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace GenderPayGap.Database
{
    public class SicCodeInitialiser : CreateDatabaseIfNotExists<DbContext>
    {
        protected override void Seed(DbContext context)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = "GenderPayGap.Database.App_Data.SicSections.csv";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var csvReader = new CsvReader(reader);
                    csvReader.Configuration.WillThrowOnMissingField = false;
                    csvReader.Configuration.TrimFields = true;
                    csvReader.Configuration.IgnoreQuotes = true;
                    var sections = csvReader.GetRecords<SicSection>().ToArray();
                    context.SicSections.AddRange(sections);
                }
            }

            context.SaveChanges();

            resourceName = "GenderPayGap.Database.App_Data.SicCodes.csv";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var csvReader = new CsvReader(reader);
                    csvReader.Configuration.WillThrowOnMissingField = false;
                    csvReader.Configuration.TrimFields = true;
                    csvReader.Configuration.IgnoreQuotes = true;
                    var codes = csvReader.GetRecords<SicCode>().ToArray();
                    Parallel.ForEach(codes,(sicCode)=>sicCode.SicSection=null);
                    context.SicCodes.AddRange(codes);
                }
            }
            context.SaveChanges();

        }
    }
}
