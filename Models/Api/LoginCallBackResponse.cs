using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCCompanyService.Models.Api
{
    /// <summary>
    /// Login CallBack Response
    /// </summary>
    public class LoginCallBackResponse
    {
        /// <summary>
        /// Token Type
        /// </summary>
        public string token_type { get; set; }

        /// <summary>
        /// Access Token
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// Expiration Date
        /// </summary>
        public string expires_at { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LoginCallBackResponse()
        {

        }

    }
}
