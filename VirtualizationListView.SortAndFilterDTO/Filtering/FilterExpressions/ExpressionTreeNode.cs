using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions
{
    /// <summary>
    /// Expression tree node
    /// </summary>
    [DataContract]
    [KnownType(typeof(BooleanOperatorNode))]
    [KnownType(typeof(ComparisonOperatorNode))]
    [Serializable]
    [XmlInclude(typeof(BooleanOperatorNode))]
    [XmlInclude(typeof(ComparisonOperatorNode))]
    public abstract class ExpressionTreeNode : ExpressionTreeElement
    {
        [DataMember]
        private ExpressionTreeElement _left;

        [DataMember]
        private ExpressionTreeElement _right;
        
        /// <summary>
        /// Left child
        /// </summary>
        [XmlElement("Left")]
        public ExpressionTreeElement Left
        {
            get { return _left; }
            set
            {
                _left = value;
                if (_left != null)
                    _left.Parent = this;
                FirePropertyChanged("Left");
            }
        }

        /// <summary>
        /// Right child
        /// </summary>
        [XmlElement("Right")]
        public ExpressionTreeElement Right
        {
            get { return _right; }
            set
            {
                _right = value;
                if (_right != null)
                    _right.Parent = this;
                FirePropertyChanged("Right");
            }
        }


        protected ExpressionTreeNode() 
            : this(null, null, null) 
        { }

        protected ExpressionTreeNode(ExpressionTreeNode parent, ExpressionTreeElement left, ExpressionTreeElement right)
            : base(parent)
        {
            Left = left;
            Right = right;
        }
    }
}
