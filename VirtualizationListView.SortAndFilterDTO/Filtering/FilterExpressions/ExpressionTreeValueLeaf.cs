using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions
{
    /// <summary>
    /// Expression tree leaf with object's value
    /// </summary>
    [DataContract(IsReference = true)]
    [Serializable]
    public class ExpressionTreeValueLeaf : ExpressionTreeElement
    {
        /// <summary>
        /// Object's value
        /// </summary>
        [XmlElement("Value")]
        [DataMember]
        public object FieldValue
        {
            get { return _fieldValue; }
            set
            {
                _fieldValue = value;
                FirePropertyChanged("FieldValue");
            }
        }
        private object _fieldValue;

        /// <summary>
        /// Available values list for object
        /// </summary>
        [XmlIgnore]
        public List<object> AvailableValues
        {
            get { return _availableValues; }
            set
            {
                _availableValues = value;
                FirePropertyChanged("AvailableValues");
            }
        }
        private List<object> _availableValues;

        /// <summary>
        /// Error information at entered object value
        /// </summary>
        [XmlIgnore]
        public ValidationResponce ErrorResponce
        {
            get { return _errorResponce; }
            set
            {
                _errorResponce = value;
                FirePropertyChanged("ErrorResponce");
            }
        }
        private ValidationResponce _errorResponce;

        #region Constructors

        public ExpressionTreeValueLeaf()
            : this(null)
        { }

        public ExpressionTreeValueLeaf(ExpressionTreeNode parent) 
            : base(parent) 
        { }

        public ExpressionTreeValueLeaf(ExpressionTreeNode parent, object fieldValue)
            : base(parent)
        {
            FieldValue = fieldValue;
        }

        #endregion

        public override string ToString()
        {
            if (FieldValue == null)
                return String.Empty;

            if (FieldValue is string
                || FieldValue is DateTime
                || FieldValue is TimeSpan)
                return "\"" + FieldValue + "\"";

            return FieldValue.ToString();
        }

        public override object Clone()
        {
            return new ExpressionTreeValueLeaf(null, FieldValue);
        }

        /// <summary>
        /// Validate condition
        /// </summary>
        /// <returns>true - success validation, otherwise - false</returns>
        public override ValidationResponce Validate()
        {
            if (ErrorResponce == null)
                return new ValidationResponce();
            return ErrorResponce;
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
