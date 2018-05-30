using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators
{
    /// <summary>
    /// Logical operators
    /// </summary>
    [DataContract]
    public enum BooleanOperators : byte
    {
        [XmlEnum("AND")]
        [Description("AND")]
        [EnumMember]
        And = 0,

        [XmlEnum("OR")]
        [Description("OR")]
        [EnumMember]
        Or = 1,

        [XmlEnum("NOT")]
        [Description("NOT")]
        [EnumMember]
        Not = 2
    }
}
