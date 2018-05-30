using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions
{
    /// <summary>
    /// Expression tree node with logical operator
    /// </summary>
    [DataContract]
    [Serializable]
    public class BooleanOperatorNode : ExpressionTreeNode
    {
        /// <summary>
        /// Logical operator merge childs
        /// </summary>
        [DataMember]
        [XmlAttribute("Operator")]
        public BooleanOperators Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
                FirePropertyChanged("Operator");
            }
        }
        private BooleanOperators _operator;


        public BooleanOperatorNode() 
            : this(null, null, null, BooleanOperators.And) 
        { }

        public BooleanOperatorNode(ExpressionTreeNode parent, BooleanOperators boolOp)
            : this(parent, null, null, boolOp) 
        { }

        public BooleanOperatorNode(ExpressionTreeNode parent, ExpressionTreeElement left, ExpressionTreeElement right, BooleanOperators boolOp)
            : base(parent, left, right)
        {
            Operator = boolOp;
        }


        public override string ToString()
        {
            if (Operator == BooleanOperators.Not)
                if (Left is BooleanOperatorNode)
                    return "!" + "(" + Left + ")";
                else
                    return "!" + Left;

            var op = Operator == BooleanOperators.And ? " && " : " || ";

            return ((Left != null) ? Left.ToString() : "")
                + op
                + ((Right != null) ? Right.ToString() : "");
        }

        public override object Clone()
        {
            return new BooleanOperatorNode(null,
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
            if (Left == null)
                return new ValidationResponce("Not set second expression for operation "/*"Не задано второе выражение для операции "*/ + Operator);
            
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
            if (Left != null)
            {
                var leftFind = Left.Find(element);
                if (leftFind != null)
                    return leftFind;
            }
            if (Right == null)
                return null;
            var rightFind = Right.Find(element);
            return rightFind;
        }
    }
}
