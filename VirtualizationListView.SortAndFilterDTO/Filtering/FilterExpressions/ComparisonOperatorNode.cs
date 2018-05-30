using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions
{
    /// <summary>
    /// Expression tree node with comparison operator
    /// </summary>
    [DataContract]
    [Serializable]
    public class ComparisonOperatorNode : ExpressionTreeNode
    {
        /// <summary>
        /// Comparison operator merge childs
        /// </summary>
        [DataMember]
        [XmlAttribute("Operator")]
        public ComparisonOperators Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
                FirePropertyChanged("Operator");
            }
        }
        private ComparisonOperators _operator;


        public ComparisonOperatorNode() 
            : this(null, null, null, ComparisonOperators.Equal) 
        { }

        public ComparisonOperatorNode(ExpressionTreeNode parent, ComparisonOperators compOp)
            : this(parent, null, null, compOp) 
        { }

        public ComparisonOperatorNode(ExpressionTreeNode parent, ExpressionTreeElement left, ExpressionTreeElement right, ComparisonOperators compOp)
            : base(parent, left, right)
        {
            Operator = compOp;
        }


        public static string OperatorToString(ComparisonOperators comparisonOperator)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperators.Equal:
                    return "=";
                case ComparisonOperators.NotEqual:
                    return "!=";
                case ComparisonOperators.Less:
                    return "<";
                case ComparisonOperators.LessOrEqual:
                    return "<=";
                case ComparisonOperators.Greater:
                    return ">";
                case ComparisonOperators.GreaterOrEqual:
                    return ">=";
                case ComparisonOperators.StartsWith:
                    return "[начинается с]";
            }
            return String.Empty;
        }

        public override string ToString()
        {
            return ((Left != null) ? Left.ToString() : "")
                + OperatorToString(Operator)
                + ((Right != null) ? Right.ToString() : "");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ToString() == obj.ToString())
                return true;
            return false;
        }

        public override object Clone()
        {
            return new ComparisonOperatorNode(null,
                Left,
                Right,
                Operator);
        }

        /// <summary>
        /// Validate condition
        /// </summary>
        /// <returns>true - success validation, otherwise - false</returns>
        public override ValidationResponce Validate()
        {
            var leftValid = Left.Validate();
            if (!leftValid.ValidationResult)
                return leftValid;

            if (Right != null)
            {
                var rightValid = Right.Validate();
                if (!rightValid.ValidationResult)
                    return rightValid;
            }

            return new ValidationResponce();
        }

        /// <summary>
        /// Find element in expression tree
        /// </summary>
        /// <param name="element">Required element</param>
        /// <returns>Required element in expression tree</returns>
        public override ExpressionTreeElement Find(ExpressionTreeElement element)
        {
            if (element == null)
                return null;
            if (element.ToString() == ToString())
                return this;
            var leftFind = Left.Find(element);
            if (leftFind != null)
                return leftFind;
            var rightFind = Right.Find(element);
            return rightFind;
        }
    }
}
