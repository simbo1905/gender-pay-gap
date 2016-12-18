using System;
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
        public IEnumerable<string> List(int startYear=0, int endYear=0, string organisation=null)
        {
            return null;
        }
   
    }
}