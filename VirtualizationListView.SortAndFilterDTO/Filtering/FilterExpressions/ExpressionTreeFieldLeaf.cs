using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions
{
    /// <summary>
    /// Expression tree leaf contain field name of object for comparison
    /// </summary>
    [DataContract(IsReference = true)]
    [Serializable]
    public class ExpressionTreeFieldLeaf : ExpressionTreeElement
    {
        /// <summary>
        /// Object's property info
        /// </summary>
        [XmlIgnore]
        public PropertyInfo Property
        {
            get { return Assembly.Load(PropertyDescription.Assembly).GetType(PropertyDescription.DeclaringType).GetProperty(PropertyDescription.FieldName); }
        }

        /// <summary>
        /// Property description for serializing data
        /// </summary>
        [DataMember]
        [XmlElement("Property")]
        public FieldDescription PropertyDescription
        {
            get { return _propertyDescription; }
            set
            {
                _propertyDescription = value;
                FirePropertyChanged("PropertyDescription");
                FirePropertyChanged("Property");
            }
        }
        private FieldDescription _propertyDescription;

        #region Constructors

        public ExpressionTreeFieldLeaf() 
            : this(null) 
        { }

        public ExpressionTreeFieldLeaf(ExpressionTreeNode parent) 
            : base(parent) 
        { }

        public ExpressionTreeFieldLeaf(ExpressionTreeNode parent, FieldDescription property)
            : base(parent)
        {
            PropertyDescription = property;
        }

        #endregion

        public override string ToString()
        {
            return Property.Name;
        }

        public override object Clone()
        {
            return new ExpressionTreeFieldLeaf(null, 
                PropertyDescription);
        }

        /// <summary>
        /// Validate condition
        /// </summary>
        /// <returns>true - success validation, otherwise - false</returns>
        public override ValidationResponce Validate()
        {
            return new ValidationResponce();
        }

        /// <summary>
        /// Find element in expression tree
        /// </summary>
        /// <param name="element">Required element</param>
        /// <returns>Required element in expression tree</returns>
        public override ExpressionTreeElement Find(ExpressionTreeElement element)
        {
            if (element != null
                && element.ToString() == ToString())
                return this;
            return null;
        }
    }
}
