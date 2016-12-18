using ABCCompanyService.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCCompanyService.Models.Api
{

    /// <summary>
    /// Absence Request Model 
    /// </summary>
    public class AbsenceRequestApi
    {
        /// <summary>
        /// Absence Request Id
        /// </summary>
        public int absenceRequestId { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// Created DateTime
        /// </summary>
        public DateTime createdDateTime { get; set; }

        /// <summary>
        /// Start EventDate Time
        /// </summary>
        public DateTime startEventDateTime { get; set; }

        /// <summary>
        /// End Event DateTime
        /// </summary>
        public DateTime endEventDateTime { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public AbsenceRequestStatus status { get; set; }

        /// <summary>
        /// Status ChangedBy
        /// </summary>
        public string statusChangedBy { get; set; }

        /// <summary>
        /// Status Changed DateTime
        /// </summary>
        public DateTime statusDateTime { get; set; }

        /// <summary>
        /// GoogleEventId
        /// </summary>
        public string googleEventId { get; set; }


        /// <summary>
        /// Default Constructor
        /// </summary>
        public AbsenceRequestApi()
        {

        }
    }
}
