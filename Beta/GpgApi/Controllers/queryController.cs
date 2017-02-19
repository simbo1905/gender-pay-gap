using GenderPayGap.Models.GpgDatabase;
using System;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Api
{
    /// <summary>
    /// This is a unsecured API where the public or influencers and retrieve GPG returns data for use with any client software
    /// </summary>
    [Route("query")]
    public class QueryController : ApiController
    {
        /// <summary>
        /// Return all GPG returns filtered using the specified criteria
        /// </summary>
        /// <param name="startYear">The start year of the GPG returns</param>
        /// <param name="endYear">The end year of the GPG returns</param>
        /// <param name="organisation">The name of the organisation</param>
        /// <returns>Array of GPG Returns for the current organisation</returns>
        [HttpGet]
        public IEnumerable<ReturnModel> List(int startYear=0, int endYear=0, string organisation=null)
        {
            foreach (var ret in GpgDatabase.Default.Return.ToList())
            {
                var result = new ReturnModel();
                var org = GpgDatabase.Default.Organisation.Find(ret.OrganisationId);
                result.OrganisationName = org==null ? "UNKNOWN" : org.OrganisationName;
                ret.CopyProperties(result);
                yield return result;
            }
        }
   
    }
}