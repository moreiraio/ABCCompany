using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCCompanyService.Models.Api
{

  
    /// <summary>
    /// Api Result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// SuccessMessage
        /// </summary>
        public static  string SuccessMessage = "Operation successfully executed";
        /// <summary>
        /// ErrorMessage
        /// </summary>
        public static  string ErrorMessage = "Operation error";
        /// <summary>
        /// Generic Type for result
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Message with information about te result of the execution
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ApiResult()
        {

        }

        /// <summary>
        /// Constructor with generic input
        /// </summary>
        /// <param name="result"></param>
        /// <param name="status"></param>
        public ApiResult(T result, bool status)
        {
            if (status)
                this.Message = SuccessMessage;
            else
                this.Message = ErrorMessage;

            this.Result = result;
        }
    }
}
