using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators
{
    /// <summary>
    /// Comparison operators
    /// </summary>
    [DataContract]
    public enum ComparisonOperators : byte
    {
        [XmlEnum("EQUAL")]
        [Description("EQUAL")]
        [EnumMember]
        Equal = 0,

        [XmlEnum("NOT EQUAL")]
        [Description("NOT EQUAL")]
        [EnumMember]
        NotEqual = 1,

        [XmlEnum("LESS")]
        [Description("LESS")]
        [EnumMember]
        Less = 2,

        [XmlEnum("LESS OR EQUAL")]
        [Description("LESS OR EQUAL")]
        [EnumMember]
        LessOrEqual = 3,

        [XmlEnum("GREATER")]
        [Description("GREATER")]
        [EnumMember]
        Greater = 4,

        [XmlEnum("GREATER OR EQUAL")]
        [Description("GREATER OR EQUAL")]
        [EnumMember]
        GreaterOrEqual = 5,

        [XmlEnum("START WITH")]
        [Description("START WITH")]
        [EnumMember]
        StartsWith = 6
    }
}
