using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace DailyClean
{
    public class Functions
    {
        //Remove any unverified users their addresses, UserOrgs, Org and addresses
        public static void PurgeUsers()
        {
            Program.InfoLog.WriteLine($"Executing {nameof(PurgeUsers)}");

        }

        //Remove any old manual registrations
        public static void PurgeManualRegistrations()
        {
            Program.InfoLog.WriteLine($"Executing {nameof(PurgeManualRegistrations)}");

        }

        //Remove retired copied of GPG data
        public static void PurgeGPGData()
        {
            Program.InfoLog.WriteLine($"Executing {nameof(PurgeGPGData)}");   
        }
    }
}
