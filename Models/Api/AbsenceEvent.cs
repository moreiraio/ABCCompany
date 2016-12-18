using ABCCompanyService.Models.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ABCCompanyService.Models.Api
{



    /// <summary>
    /// Abcence Event
    /// </summary>
    public class AbsenceEvent
    {
        /// <summary>
        /// Start Day and hour off the event
        /// </summary>
        public DateTime startEventDateTime { get; set; }

        /// <summary>
        /// End Day and hour off the event
        /// </summary>
        public DateTime endEventDateTime { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Avaible Status (Pending,Aproved,Declined)
        /// </summary>
        [EnumDataType(typeof(AbsenceRequestStatus))]
        [JsonConverter(typeof(StringEnumConverter))]
        public AbsenceRequestStatus status { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public AbsenceEvent()
        {

        }
    }
}
