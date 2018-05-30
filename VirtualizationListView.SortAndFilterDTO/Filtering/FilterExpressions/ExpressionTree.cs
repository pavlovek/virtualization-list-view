using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions
{
    /// <summary>
    /// Describe filter conditions by binary expression tree
    /// </summary>
    [DataContract]
    [Serializable]
    public class ExpressionTree : IEquatable<ExpressionTree>, INotifyPropertyChanged
    {
        /// <summary>
        /// Expression tree root
        /// </summary>
        [DataMember]
        [XmlElement("Root")]
        public ExpressionTreeNode Root
        {
            get { return _root; }
            set
            {
                _root = value;
                FirePropertyChanged("Root");
            }
        }
        private ExpressionTreeNode _root;

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExpressionTree()
        {
            Root = null;
        }

        /// <summary>
        /// Constructor with logical expression node
        /// </summary>
        /// <param name="boolOp">Logical operator</param>
        /// <param name="left">Left operand (child)</param>
        /// <param name="right">Right operand (child)</param>
        public ExpressionTree(BooleanOperators boolOp, ExpressionTreeElement left, ExpressionTreeElement right)
            : this(boolOp)
        {
            Root.Left = left;
            Root.Right = right;
        }

        /// <summary>
        /// Constructor with logical expression node
        /// </summary>
        /// <param name="boolOp">Logical operator</param>
        public ExpressionTree(BooleanOperators boolOp)
        {
            Root = new BooleanOperatorNode(null, boolOp);
        }

        /// <summary>
        /// Constructor with comparison expression node
        /// </summary>
        /// <param name="compOp">Comparison operator</param>
        /// <param name="left">Left operand (child)</param>
        /// <param name="right">Right operand (child)</param>
        public ExpressionTree(ComparisonOperators compOp, ExpressionTreeElement left, ExpressionTreeElement right)
            : this(compOp)
        {
            Root.Left = left;
            Root.Right = right;
        }

        /// <summary>
        /// Constructor with comparison expression node
        /// </summary>
        /// <param name="compOp">Comparison operator</param>
        public ExpressionTree(ComparisonOperators compOp)
        {
            Root = new ComparisonOperatorNode(null, compOp);
        }

        #endregion

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

        public override string ToString()
        {
            return Root.ToString();
        }

        public bool Equals(ExpressionTree other)
        {
            if (ReferenceEquals(other, null)) throw new ArgumentNullException();
            if (ReferenceEquals(Root, null))
                if (ReferenceEquals(other.Root, null))
                    return true;
                else
                    return false;
            if (ReferenceEquals(other.Root, null))
                return false;
            else
                return Root.ToString() == other.Root.ToString();
        }

        /// <summary>
        /// Validate condition
        /// </summary>
        /// <returns>true - success validation, otherwise - false</returns>
        public ValidationResponce Validate()
        {
            if (Root != null)
                return Root.Validate();
            return new ValidationResponce();
        }

        /// <summary>
        /// Find element in expression tree
        /// </summary>
        /// <param name="element">Required element</param>
        /// <returns>Required element in expression tree</returns>
        public ExpressionTreeElement Find(ExpressionTreeElement element)
        {
            return Root.Find(element);
        }

        public bool FindAndReplace(ExpressionTreeElement findElement, ExpressionTreeElement replaceElement)
        {
            if (Root.Equals(findElement)
                && replaceElement is ExpressionTreeNode)
            {
                Root = replaceElement as ExpressionTreeNode;

                return true;
            }

            var expression = Root.Find(findElement.Parent) as BooleanOperatorNode;
            if (expression != null)
            {
                if (expression.Left != null
                    && expression.Left.Equals(findElement))
                    expression.Left = replaceElement;
                else if (expression.Right != null
                         && expression.Right.Equals(findElement))
                    expression.Right = replaceElement;
                else
                    return false;

                return true;
            }

            return false;
        }
    }
}
