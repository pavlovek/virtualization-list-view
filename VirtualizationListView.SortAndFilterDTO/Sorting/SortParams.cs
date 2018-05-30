using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Sorting
{
    /// <summary>
    /// Sorting parameters
    /// </summary>
    [DataContract]
    [XmlRoot("SortingParameters")]
    public class SortParams
    {
        /// <summary>
        /// Property description for sorting
        /// </summary>
        [DataMember]
        [XmlElement("Property")]
        public FieldDescription PropertyDescr { get; set; }

        /// <summary>
        /// Property info for which carry sorting
        /// </summary>
        [XmlIgnore]
        public PropertyInfo Property
        {
            get { return DeclaringPropertyType.GetProperty(PropertyDescr.FieldName); } 
        }

        /// <summary>
        /// Type's object contain property for which carry sorting
        /// </summary>
        [XmlIgnore]
        public Type DeclaringPropertyType
        {
            get { return Assembly.Load(PropertyDescr.Assembly).GetType(PropertyDescr.DeclaringType); }
        }

        /// <summary>
        /// Sort order
        /// </summary>
        [DataMember]
        [XmlAttribute("IsAsc")]
        public bool IsAsc { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public SortParams()
        {
            IsAsc = true;
        }

        /// <summary>
        /// Constructor for all parameters
        /// </summary>
        /// <param name="propertyDescr">Property description for sorting</param>
        /// <param name="isAsc">Sort order</param>
        public SortParams(FieldDescription propertyDescr, bool isAsc)
        {
            PropertyDescr = propertyDescr;
            IsAsc = isAsc;
        }
    }
}
