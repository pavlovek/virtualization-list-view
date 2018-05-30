using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using VirtualizationListViewControl.Localization;

namespace VirtualizationListViewControl.Helpers
{
    internal class LocalizableEnumItemsSource : ObservableCollection<String>, IValueConverter
    {
        Type _type;

        IDictionary<Object, Object> _valueToNameMap;

        IDictionary<Object, Object> _nameToValueMap;

        readonly object _lockObj = new object();

        public Type Type
        {
            get { return _type; }
            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("value must be Enum", "value");
                _type = value;
                Initialize();
            }
        }

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            bool isInverse = false;
            if (parameter != null)
                isInverse = parameter.ToString() == "inverse";

            lock (_lockObj)
            {
                return isInverse ? _nameToValueMap[value] : _valueToNameMap[value];
            }
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            bool isInverse = false;
            if (parameter != null)
                isInverse = parameter.ToString() == "inverse";

            lock (_lockObj)
            {
                return isInverse ? _valueToNameMap[value] : _nameToValueMap[value];
            }
        }

        void Initialize()
        {
            lock (_lockObj)
            {
                _valueToNameMap = _type
                    .GetFields(BindingFlags.Static | BindingFlags.Public)
                    .ToDictionary(fi => fi.GetValue(null), GetLocalizedDescription);
                _nameToValueMap = _valueToNameMap
                    .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
                Clear();
                foreach (String name in _nameToValueMap.Keys)
                    Add(name);
            }
        }

        public void Initialize(Type type, IEnumerable<Object> availableObjects)
        {
            if (!type.IsEnum)
                throw new ArgumentException("value must be Enum", "value");
            _type = type;

            lock (_lockObj)
            {
                _valueToNameMap = _type
                    .GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(fi => availableObjects.Contains(fi.GetValue(null)))
                    .ToDictionary(fi => fi.GetValue(null), GetLocalizedDescription);
                _nameToValueMap = _valueToNameMap
                    .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
                Clear();
                foreach (String name in _nameToValueMap.Keys)
                    Add(name);
            }
        }

        static Object GetLocalizedDescription(FieldInfo fieldInfo)
        {
            var localizedValue = LocalizationManager.GetLocalizedValue(fieldInfo.Name);
            if (String.IsNullOrWhiteSpace(localizedValue))
            {
                var descriptionAttribute =
                    (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
                localizedValue = descriptionAttribute != null ? descriptionAttribute.Description : fieldInfo.Name;
            }
            return localizedValue;
        }

        public static IEnumerable<string> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException("t must be an Enum type", "t");
            var itemSource = new LocalizableEnumItemsSource { Type = t };
            return itemSource;
        }
    }
}
