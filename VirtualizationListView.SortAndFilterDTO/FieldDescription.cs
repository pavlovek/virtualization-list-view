using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO
{
    /// <summary>
    /// Object's field description
    /// </summary>
    [DataContract]
    public struct FieldDescription
    {
        /// <summary>
        /// Assembly name where define type
        /// </summary>
        [DataMember]
        [XmlAttribute("Assembly")]
        public string Assembly { get; set; }

        /// <summary>
        /// Type name where declare this field
        /// </summary>
        [DataMember]
        [XmlAttribute("DeclaringType")]
        public string DeclaringType { get; set; }

        /// <summary>
        /// Field name
        /// </summary>
        [DataMember]
        [XmlAttribute("PropertyName")]
        public string FieldName { get; set; }

        /// <summary>
        /// Numeric data presentation
        /// </summary>
        [DataMember]
        [XmlAttribute("NumberStyle")]
        public NumberStyles NumberStyle { get; set; }

        /// <summary>
        /// Is FieldDescription structure Null
        /// </summary>
        [XmlIgnore]
        public bool IsNull
        {
            get
            {
                return String.IsNullOrWhiteSpace(Assembly)
                       && String.IsNullOrWhiteSpace(DeclaringType)
                       && String.IsNullOrWhiteSpace(FieldName);
            }
        }


        /// <summary>
        /// Constructor for basic parameters
        /// </summary>
        /// <param name="assembly">Assembly name where define type</param>
        /// <param name="declaringType">Type name where declare this field</param>
        /// <param name="fieldName">Field name</param>
        public FieldDescription(string assembly, string declaringType, string fieldName) : this()
        {
            Assembly = assembly;
            DeclaringType = declaringType;
            FieldName = fieldName;
            NumberStyle = NumberStyles.None;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((FieldDescription)obj);
        }

        private bool Equals(FieldDescription other)
        {
            return Assembly == other.Assembly 
                && DeclaringType == other.DeclaringType 
                && FieldName == other.FieldName;
        }
    }
}
