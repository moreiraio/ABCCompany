using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCCompanyService.Models
{
    /// <summary>
    /// Absence Request Status Enum
    /// </summary>
    public enum AbsenceRequestStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending,

        /// <summary>
        /// Aproved
        /// </summary>
        Aproved,

        /// <summary>
        /// Declined
        /// </summary>
        Declined
    }


    /// <summary>
    /// Absence Request Model 
    /// </summary>
    public class AbsenceRequest
    {
        /// <summary>
        /// Absence Request Id
        /// </summary>
        public int AbsenceRequestId { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Created DateTime
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Start EventDate Time
        /// </summary>
        public DateTime StartEventDateTime { get; set; }

        /// <summary>
        /// End Event DateTime
        /// </summary>
        public DateTime EndEventDateTime { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public AbsenceRequestStatus Status { get; set; }

        /// <summary>
        /// Status ChangedBy
        /// </summary>
        public string StatusChangedBy { get; set; }

        /// <summary>
        /// Status Changed DateTime
        /// </summary>
        public DateTime StatusDateTime { get; set; }

        /// <summary>
        /// GoogleEventId
        /// </summary>
        public string GoogleEventId { get; set; }


        /// <summary>
        /// Default Constructor
        /// </summary>
        public AbsenceRequest()
        {

        }
    }
}
