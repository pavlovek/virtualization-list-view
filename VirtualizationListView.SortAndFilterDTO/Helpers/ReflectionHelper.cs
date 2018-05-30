using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtualizationListView.SortAndFilterDTO.Helpers
{
    public class ReflectionHelper
    {
        /// <summary>
        /// Get property path at specify class
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="classType">Class that contain property</param>
        /// <param name="knownTypes">Known Types if use polymorphism</param>
        /// <param name="preambule">Property path preambule</param>
        /// <param name="findSuccess">Indicate when property path found success</param>
        /// <returns>Property path</returns>
        public static string GetPropertyPathFromClass(string propertyName, Type classType,
            Dictionary<Type, Type[]> knownTypes, string preambule, out bool findSuccess)
        {
            findSuccess = false;

            var specClassTypes = new [] { classType };
            if (knownTypes != null
                && knownTypes.ContainsKey(classType))
                specClassTypes = knownTypes[classType];

            foreach (var property in specClassTypes.SelectMany(specClassType => specClassType.GetProperties()))
            {
                string propertyPath;
                if (property.Name == propertyName)
                {
                    propertyPath = propertyName;
                    if (!String.IsNullOrEmpty(preambule))
                        propertyPath = preambule + "." + propertyName;
                    findSuccess = true;
                    return propertyPath;
                }
                if (property.PropertyType.FullName != "System.String"
                    && property.PropertyType.FullName != "System.DateTime"
                    && !property.PropertyType.IsPrimitive
                    && !(property.PropertyType.IsGenericType
                         && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    var newPreambule = property.Name;
                    if (!String.IsNullOrEmpty(preambule))
                        newPreambule = preambule + "." + property.Name;
                    propertyPath = GetPropertyPathFromClass(propertyName, property.PropertyType, knownTypes, newPreambule,
                        out findSuccess);
                    if (findSuccess)
                        return propertyPath;
                }
            }
            
            return String.Empty;
        }

        /// <summary>
        /// Get property path at specify class
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="classType">Class that contain property</param>
        /// <param name="preambule">Property path preambule</param>
        /// <param name="findSuccess">Indicate when property path found success</param>
        /// <returns>Property path</returns>
        public static string GetPropertyPathFromClass(string propertyName, Type classType, string preambule,
            out bool findSuccess)
        {
            return GetPropertyPathFromClass(propertyName, classType, null, preambule, out findSuccess);
        }

        /// <summary>
        /// Get property value at specify object
        /// </summary>
        /// <param name="obj">Object that contain property</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value</returns>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj != null)
            {
                var propertiesAgr = propertyName.Split('.');
                for (int i = 0; i < propertiesAgr.Length; i++)
                {
                    var prop = obj.GetType().GetProperty(propertiesAgr[i]);
                    if (prop != null)
                        obj = prop.GetValue(obj, null);
                    else if (i == propertiesAgr.Length - 1)
                        obj = null;
                }
            }
            return obj;
        }

        /// <summary>
        /// Define Nullable type
        /// </summary>
        /// <param name="popertyType">Type of object</param>
        /// <returns>Indicate when object type is Nullable</returns>
        public static bool IsNullable(Type popertyType)
        {
            if (popertyType == null)
                return true;    //obvious
            if (!popertyType.IsValueType)
                return true;    //ref-type
            if (Nullable.GetUnderlyingType(popertyType) != null)
                return true;    //Nullable<T>
            return false;       //value-type
        }

        /// <summary>
        /// Get type T inside Nullable construction
        /// </summary>
        /// <param name="properType">Type of object</param>
        /// <returns></returns>
        public static Type GetRealType(Type properType)
        {
            if (properType == null)
                return null;        //obvious
            if (!properType.IsValueType)
                return properType;  //ref-type
            if (properType.IsGenericType
                && properType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(properType);  //real type in Nulable
            return properType;      //value-type
        }
    }
}
