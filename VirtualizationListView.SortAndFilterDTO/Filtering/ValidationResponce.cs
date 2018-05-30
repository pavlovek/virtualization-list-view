using System;
using System.Runtime.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering
{
    /// <summary>
    /// Validation result and error message
    /// </summary>
    [DataContract]
    public class ValidationResponce
    {
        /// <summary>
        /// Validation result flag
        /// </summary>
        [DataMember]
        public bool ValidationResult;

        /// <summary>
        /// Validation error message if result false
        /// </summary>
        [DataMember]
        public string ValidationErrorMessage;


        /// <summary>
        /// Success validation constructor
        /// </summary>
        public ValidationResponce()
        {
            ValidationResult = true;
            ValidationErrorMessage = String.Empty;
        }

        /// <summary>
        /// Failure validation constructor
        /// </summary>
        /// <param name="validationErrorMessage">Error message</param>
        public ValidationResponce(string validationErrorMessage)
        {
            ValidationResult = false;
            ValidationErrorMessage = validationErrorMessage;
        }
    }
}
