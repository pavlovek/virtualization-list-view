using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions
{
    /// <summary>
    /// Expression tree element
    /// </summary>
    [DataContract(IsReference = true)]
    [KnownType(typeof(ExpressionTreeNode))]
    [KnownType(typeof(BooleanOperatorNode))]
    [KnownType(typeof(ComparisonOperatorNode))]
    [KnownType(typeof(ExpressionTreeFieldLeaf))]
    [KnownType(typeof(ExpressionTreeValueLeaf))]
    [Serializable]
    [XmlInclude(typeof(ExpressionTreeNode))]
    [XmlInclude(typeof(BooleanOperatorNode))]
    [XmlInclude(typeof(ComparisonOperatorNode))]
    [XmlInclude(typeof(ExpressionTreeFieldLeaf))]
    [XmlInclude(typeof(ExpressionTreeValueLeaf))]
    public abstract class ExpressionTreeElement : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// Parent element
        /// </summary>
        [XmlIgnore]
        [DataMember]
        public ExpressionTreeNode Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                FirePropertyChanged("Parent");
            }
        }
        private ExpressionTreeNode _parent;


        protected ExpressionTreeElement() : this(null) { }

        protected ExpressionTreeElement(ExpressionTreeNode parent)
        {
            Parent = parent;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        protected void FirePropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
        }

        #endregion

        public abstract object Clone();

        /// <summary>
        /// Validate condition
        /// </summary>
        /// <returns>true - success validation, otherwise - false</returns>
        public abstract ValidationResponce Validate();

        /// <summary>
        /// Find element in expression tree
        /// </summary>
        /// <param name="element">Required element</param>
        /// <returns>Required element in expression tree</returns>
        public abstract ExpressionTreeElement Find(ExpressionTreeElement element);
    }
}
