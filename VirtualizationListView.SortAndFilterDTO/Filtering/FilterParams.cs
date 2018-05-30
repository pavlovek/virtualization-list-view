using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;

namespace VirtualizationListView.SortAndFilterDTO.Filtering
{
    /// <summary>
    /// Filtration parameters
    /// </summary>
    [DataContract]
    [Serializable]
    [XmlRoot("FilterParameters")]
    public class FilterParams : IEquatable<FilterParams>
    {
        /// <summary>
        /// Filter name
        /// </summary>
        [DataMember]
        [XmlAttribute("Title")]
        public string Name { get; set; }
        
        /// <summary>
        /// Filter conditions
        /// </summary>
        [DataMember]
        [XmlElement("Conditions")]
        public ExpressionTree Conditions { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public FilterParams() : this(null)
        { }

        /// <summary>
        /// Constructor with filter conditions
        /// </summary>
        /// <param name="conditions">Filter conditions</param>
        public FilterParams(ExpressionTree conditions)
            : this(String.Empty, conditions)
        { }

        /// <summary>
        /// Constructor for all parameters
        /// </summary>
        /// <param name="name">Filter name</param>
        /// <param name="conditions">Filter conditions</param>
        public FilterParams(string name, ExpressionTree conditions)
        {
            Name = name;
            Conditions = conditions;
        }


        public bool Equals(FilterParams other)
        {
            if (ReferenceEquals(Conditions, null))
                if (ReferenceEquals(other.Conditions, null))
                    return true;
                else
                    return false;
            return Name == other.Name && Conditions.Equals(other.Conditions);
        }

        /// <summary>
        /// Validate desired filter conditions
        /// </summary>
        /// <returns>true - success validation, otherwise - false</returns>
        public ValidationResponce ValidateConditions()
        {
            var validResult = Conditions.Validate();
            if (!validResult.ValidationResult)
            {
                if (String.IsNullOrWhiteSpace(validResult.ValidationErrorMessage))
                    validResult.ValidationErrorMessage = "Filter not validation desired conditions";//"Фильтр не прошел проверку правильности заданного условия";
                validResult.ValidationErrorMessage = validResult.ValidationErrorMessage.Insert(0,
                                                                                               "[" + Name + "] - ");
            }

            return validResult;
        }
    }
}
