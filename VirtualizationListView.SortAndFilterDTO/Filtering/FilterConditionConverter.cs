using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;
using VirtualizationListView.SortAndFilterDTO.Helpers;

namespace VirtualizationListView.SortAndFilterDTO.Filtering
{
    /// <summary>
    /// Converter filter condition to other formats
    /// </summary>
    public class FilterConditionConverter
    {
        /// <summary>
        /// Get LINQ string from filter expression (IQuaryable)
        /// </summary>
        /// <param name="filterElement">Filter expression</param>
        /// <param name="classType">Object Type for filtering (it contain filtering fields)</param>
        /// <param name="knownTypes">Known Types if use polymorphism</param>
        /// <param name="preambule">Preambule for LINQ string</param>
        /// <returns>Filter expression for LINQ</returns>
        public static string GetLinqFilteringConditions(ExpressionTreeElement filterElement, Type classType, Dictionary<Type, Type[]> knownTypes = null, string preambule = null)
        {
            var fieldLeaf = filterElement as ExpressionTreeFieldLeaf;
            if (fieldLeaf != null)
            {
                bool isFindProperty;
                var filterProp = ReflectionHelper.GetPropertyPathFromClass(fieldLeaf.PropertyDescription.FieldName,
                                                                           classType,
                                                                           knownTypes,
                                                                           preambule,
                                                                           out isFindProperty);
                if (isFindProperty)
                {
                    if (fieldLeaf.Property.PropertyType == typeof(string))
                        return filterProp;
                    if (fieldLeaf.Property.PropertyType == typeof(DateTime))
                        filterProp += ".Ticks";
                    else if (fieldLeaf.Property.PropertyType.IsClass
                             && fieldLeaf.Property.PropertyType != typeof(Enum))
                        filterProp += ".HashCode";
                    else if (fieldLeaf.PropertyDescription.NumberStyle == NumberStyles.HexNumber)
                        filterProp = "(Int64(" + filterProp + ")).ToString(\"X\")";
                    return filterProp;
                }
            }

            var valueLeaf = filterElement as ExpressionTreeValueLeaf;
            if (valueLeaf != null)
            {
                var fieldByValueLeaf = valueLeaf.Parent.Left as ExpressionTreeFieldLeaf;
                if (fieldByValueLeaf != null)
                {
                    var fieldType = ReflectionHelper.GetRealType(fieldByValueLeaf.Property.PropertyType);

                    if (fieldType == typeof(string)
                        || fieldType == typeof(Enum)
                        || fieldType.BaseType == typeof(Enum))
                    {
                        return "\"" + valueLeaf.FieldValue + "\"";
                    }
                    if (fieldType == typeof(DateTime))
                    {
                        return ((DateTime) valueLeaf.FieldValue).Ticks.ToString();
                    }
                    if (fieldType.IsClass)
                    {
                        return valueLeaf.FieldValue.GetType()
                            .GetProperty("HashCode")
                            .GetValue(valueLeaf.FieldValue)
                            .ToString();
                    }
                    if (fieldByValueLeaf.PropertyDescription.NumberStyle == NumberStyles.HexNumber)
                    {
                        long value;
                        if (long.TryParse(valueLeaf.ToString(), out value))
                            return value.ToString("X");
                    }
                }
                return valueLeaf.FieldValue.ToString();
            }

            var comparisonNode = filterElement as ComparisonOperatorNode;
            if (comparisonNode != null)
            {
                var comparisonNodeValueLeaf = comparisonNode.Right as ExpressionTreeValueLeaf;
                if (comparisonNodeValueLeaf != null)
                {
                    var comparisonNodeFieldLeaf = comparisonNode.Left as ExpressionTreeFieldLeaf;
                    string expressionFieldProp;
                    bool isFindProperty = false;
                    if (comparisonNodeFieldLeaf != null
                        && ReflectionHelper.IsNullable(comparisonNodeFieldLeaf.Property.PropertyType))
                    {
                        expressionFieldProp = ReflectionHelper.GetPropertyPathFromClass(comparisonNodeFieldLeaf.PropertyDescription.FieldName,
                                                                                        classType,
                                                                                        knownTypes,
                                                                                        preambule,
                                                                                        out isFindProperty);
                    }
                    else
                    {
                        expressionFieldProp = GetLinqFilteringConditions(comparisonNodeFieldLeaf, classType, knownTypes, preambule);
                    }

                    if (comparisonNodeValueLeaf.FieldValue == null
                        || (comparisonNodeValueLeaf.FieldValue is String
                            && String.IsNullOrWhiteSpace(comparisonNodeValueLeaf.FieldValue as String)))
                        return expressionFieldProp +
                               ComparisonFilterOperatorToLinq(comparisonNode.Operator, false, true)
                               + "null";

                    var valueStr = GetLinqFilteringConditions(comparisonNode.Right, classType, knownTypes, preambule);
                    if (comparisonNode.Operator == ComparisonOperators.StartsWith)
                        valueStr = "(\"" + valueStr + "\")";

                    var preExpression = String.Empty;

                    if (comparisonNodeFieldLeaf != null
                        && ReflectionHelper.IsNullable(comparisonNodeFieldLeaf.Property.PropertyType)
                        && isFindProperty)
                    {
                        preExpression = expressionFieldProp +
                                        ComparisonFilterOperatorToLinq(ComparisonOperators.NotEqual, false, true)
                                        + "null" +
                                        BooleanFilterOperatorToLinq(BooleanOperators.And);
                    }

                    return preExpression +
                           GetLinqFilteringConditions(comparisonNodeFieldLeaf, classType, knownTypes, preambule) +
                           ComparisonFilterOperatorToLinq(comparisonNode.Operator, comparisonNodeValueLeaf.FieldValue is Enum) +
                           valueStr;
                }
            }

            var booleanNode = filterElement as BooleanOperatorNode;
            if (booleanNode != null)
            {
                return GetLinqFilteringConditions(booleanNode.Left, classType, knownTypes, preambule) +
                       BooleanFilterOperatorToLinq(booleanNode.Operator) +
                       GetLinqFilteringConditions(booleanNode.Right, classType, knownTypes, preambule);
            }

            return String.Empty;
        }

        /// <summary>
        /// Get LINQ expression for field values from objects list
        /// </summary>
        /// <param name="field">Field description for expression</param>
        /// <param name="classType">Object Type for expression (it contain expression field)</param>
        /// <param name="list">Objects list</param>
        /// <returns>LINQ expression for field values</returns>
        public static IQueryable<object> GetFieldValuesLinq(FieldDescription field, Type classType, IQueryable list)
        {
            var property = Assembly.Load(field.Assembly).GetType(field.DeclaringType).GetProperty(field.FieldName);

            var param = Expression.Parameter(classType, "p");
            var selector = Expression.Property(param, property);
            var pred = Expression.Lambda(selector, param);
            var expr = Expression.Call(typeof(Queryable), "Select",
                new[] { classType, property.PropertyType },
                Expression.Constant(list), pred);
            IQueryable<object> query = list.Provider.CreateQuery<object>(expr);

            return query;
        }

        /// <summary>
        /// Get LINQ comparison operator representation
        /// </summary>
        /// <param name="operators">Comparison operator</param>
        /// <param name="isEnumValue">Indicate when comparison value is enum</param>
        /// <param name="isNullValue">Indicate when comparison value is null</param>
        /// <returns>LINQ comparison operator representation</returns>
        protected static string ComparisonFilterOperatorToLinq(ComparisonOperators operators, bool isEnumValue, bool isNullValue = false)
        {
            switch (operators)
            {
                case ComparisonOperators.Equal:
                    return isEnumValue ? " = " : " == ";
                case ComparisonOperators.NotEqual:
                    return " != ";
                case ComparisonOperators.Less:
                    return " < ";
                case ComparisonOperators.LessOrEqual:
                    return " <= ";
                case ComparisonOperators.Greater:
                    return " > ";
                case ComparisonOperators.GreaterOrEqual:
                    return " >= ";
                case ComparisonOperators.StartsWith:
                    return isNullValue ? (isEnumValue ? " = " : " == ") : ".StartsWith";
            }
            return String.Empty;
        }

        /// <summary>
        /// Get LINQ boolean operator representation
        /// </summary>
        /// <param name="operators">Boolean operator</param>
        /// <returns>LINQ boolean operator representation</returns>
        protected static string BooleanFilterOperatorToLinq(BooleanOperators operators)
        {
            switch (operators)
            {
                //case BooleanOperators.Not:
                //    return " NOT ";
                case BooleanOperators.And:
                    return " AND ";
                case BooleanOperators.Or:
                    return " OR ";
            }
            return String.Empty;
        }
    }
}
