using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using Microsoft.Azure.WebJobs;

namespace DailyClean
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        #region Properties
        public static IContainer ContainerIOC;
        public static IFileRepository FileRepository;

        private static Logger _InfoLog;
        public static Logger InfoLog
        {
            get
            {
                if (_InfoLog == null) _InfoLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer", "InfoLog.txt"));
                return _InfoLog;
            }
        }

        private static Logger _WarningLog;
        public static Logger WarningLog
        {
            get
            {
                if (_WarningLog == null) _WarningLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer", "WarningLog.txt"));
                return _WarningLog;
            }
        }

        private static Logger _ErrorLog;
        public static Logger ErrorLog
        {
            get
            {
                if (_ErrorLog == null) _ErrorLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer", "ErrorLog.txt"));
                return _ErrorLog;
            }
        }
        #endregion

        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            //Create Inversion of Control container
            ContainerIOC = BuildContainerIoC();
            FileRepository = ContainerIOC.Resolve<IFileRepository>();

            Functions.PurgeGPGData();
            Functions.PurgeManualRegistrations();
            Functions.PurgeUsers();
        }

        public static IContainer BuildContainerIoC()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => new SqlRepository(new DbContext())).As<IRepository>();

            var azureStorageConnectionString = ConfigurationManager.AppSettings["AzureStorageConnectionString"];
            var azureStorageShareName = ConfigurationManager.AppSettings["AzureStorageShareName"];
            var localStorageRoot = ConfigurationManager.AppSettings["LocalStorageRoot"];

            if (!string.IsNullOrWhiteSpace(azureStorageConnectionString) && !string.IsNullOrWhiteSpace(azureStorageShareName))
                builder.Register(c => new AzureFileRepository(azureStorageConnectionString, azureStorageShareName)).As<IFileRepository>();
            else
                builder.Register(c => new SystemFileRepository(localStorageRoot)).As<IFileRepository>();

            return builder.Build();
        }

    }
}
