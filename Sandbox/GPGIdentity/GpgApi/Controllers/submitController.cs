using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Api
{
    /// <summary>
    /// This is a secure API for organisations to list, retrieve, save and submit their GPG returns using any client software
    /// </summary>
    [Route("submit")]
    [Authorize]
    public class SubmitController : ApiController
    {
        /// <summary>
        /// Return all GPG returns filtered using the specified criteria
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns>Array of GPG Returns for the current organisation</returns>
        [HttpGet]
        public IEnumerable<string> List(int? startYear, int? endYear)
        {
            return null;
        }

        /// <summary>
        /// Returns a specific GPG return 
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns>Array of GPG Returns for the current organisation</returns>
        [HttpGet]
        public string List(long returnId)
        {
            return null;
        }

        /// <summary>
        /// Deletes a specific GPG Return for the current organisation for the specified year
        /// </summary>
        /// <param name="returnId">The unique identifier of the specific GPG Return to delete</param>
        /// <param name="year">The specific year of the GPG Return to delete</param>
        /// <returns>Confirmation or an error code</returns>
        [HttpDelete]
        public IHttpActionResult Delete(long returnId, int year)
        {
            return null;
        }

        /// <summary>
        /// Saves a temporary copy of the GPG return data for the current users organisation
        /// </summary>
        /// <param name="year">The year to which the GPG Return data applies</param>
        /// <param name="gpgData">The actual GPG data</param>
        /// <param name="returnId">The specific GPG Return identifier for an existing return or empty if saving a new GPG return</param>
        /// <returns>The ReturnId or an error code</returns>
        [HttpPut]
        public IHttpActionResult Save(int year, [FromBody]string gpgData, long returnId = 0)
        {
            return null;
        }

        /// <summary>
        /// Submits a new GPG Return or an existing temporary copy for the current users organisation for the specified year
        /// </summary>
        /// <param name="year">The year to which the GPG Return data applies</param>
        /// <param name="gpgData">The actual GPG data. Can be empty if returnID is specified for an existing copy</param>
        /// <param name="returnId">The specific GPG Return identifier for an existing GPG Return or empty if saving a new GPG return</param>
        /// <returns>The ReturnId or an error code</returns>
        [HttpPost]
        public IHttpActionResult Submit(int year, [FromBody]string gpgData = null, long returnId = 0)
        {
            return null;
        }
    }
}