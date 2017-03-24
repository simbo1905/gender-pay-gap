using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Extensions;
using GenderPayGap;
using IdentityServer3.Core.Logging;
using Logger = IdentityServer3.Core.Logging.Logger;

namespace GenderPayGap.IdentityServer
{
    public class CustomLogProvider : ILogProvider
    {

        private static bool _Debug = ConfigurationManager.AppSettings["LogDebug"].ToBoolean();

        public Logger GetLogger(string name)
        {
            return Log;
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }

        private bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                case LogLevel.Warn:
                    return false;
                case LogLevel.Debug:
                    if (!_Debug) return false;
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                case LogLevel.Trace:
                    break;
            }
            var result = messageFunc?.Invoke();

            if (!string.IsNullOrWhiteSpace(result))
                result = string.Format(result, formatParameters);
            else if (exception != null)
            {
                result = exception.ToString();
                //Track the exception with Application Insights if it is available
                MvcApplication.AppInsightsClient?.TrackException(exception);

            }

            if (string.IsNullOrWhiteSpace(result)) return false;

            Global.Log.WriteLine(result);
            return true;
        }
    }
}