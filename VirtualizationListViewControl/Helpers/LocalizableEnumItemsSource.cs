using System;
using System.Collections.Concurrent;
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
        private Type _type;

        private ConcurrentDictionary<object, object> _valueToNameMap;

        private ConcurrentDictionary<object, object> _nameToValueMap;

        public Type Type
        {
            get { return _type; }
            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("value must be Enum", nameof(value));
                _type = value;
                Initialize();
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var isInverse = false;
            if (parameter != null)
                isInverse = parameter.ToString() == "inverse";

            return isInverse ? _nameToValueMap[value] : _valueToNameMap[value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            bool isInverse = false;
            if (parameter != null)
                isInverse = parameter.ToString() == "inverse";

            return isInverse ? _valueToNameMap[value] : _nameToValueMap[value];
        }

        void Initialize()
        {
            _valueToNameMap = new ConcurrentDictionary<object, object>(_type
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .ToDictionary(fi => fi.GetValue(null), GetLocalizedDescription));
            _nameToValueMap = new ConcurrentDictionary<object, object>(_valueToNameMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key));
            Clear();
            foreach (String name in _nameToValueMap.Keys)
                Add(name);
        }

        public void Initialize(Type type, IEnumerable<object> availableObjects)
        {
            if (!type.IsEnum)
                throw new ArgumentException("type must be Enum", nameof(type));
            _type = type;

            _valueToNameMap = new ConcurrentDictionary<object, object>(_type
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(fi => availableObjects.Contains(fi.GetValue(null)))
                .ToDictionary(fi => fi.GetValue(null), GetLocalizedDescription));
            _nameToValueMap = new ConcurrentDictionary<object, object>(_valueToNameMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key));
            Clear();
            foreach (String name in _nameToValueMap.Keys)
                Add(name);
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
                throw new ArgumentException("t must be an Enum type", nameof(t));
            var itemSource = new LocalizableEnumItemsSource { Type = t };
            return itemSource;
        }
    }
}
