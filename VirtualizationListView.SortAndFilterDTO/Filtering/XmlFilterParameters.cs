using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering
{
    /// <summary>
    /// XML presentation filtration parameters
    /// </summary>
    [XmlRoot("FiltersParameters")]
    [DataContract]
    public class XmlFilterParameters
    {
        /// <summary>
        /// Current filter name
        /// </summary>
        [XmlAttribute("CurrentFilterName")]
        [DataMember]
        public string CurrentFilter { get; set; }

        /// <summary>
        /// All filters list
        /// </summary>
        [XmlArray("Filters")]
        [XmlArrayItem("FilterParameters")]
        [DataMember]
        public FilterParams[] Filters { get; set; }
    }
}
