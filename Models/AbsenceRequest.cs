using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCCompanyService.Models
{
    /// <summary>
    /// 
    /// </summary>
    public enum AbsenceRequestStatus
    {
        Pending,
        Aproved,
        Declined
    }

    public class AbsenceRequest
    {
        public int AbsenceRequestId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime StartEventDateTime { get; set; }
        public DateTime EndEventDateTime { get; set; }
        public string Description { get; set; }
        public AbsenceRequestStatus Status { get; set; }
        public string StatusChangedBy { get; set; }
        public DateTime StatusDateTime { get; set; }
        public string GoogleEventId { get; set; }

        public AbsenceRequest()
        {

        }
    }
}
