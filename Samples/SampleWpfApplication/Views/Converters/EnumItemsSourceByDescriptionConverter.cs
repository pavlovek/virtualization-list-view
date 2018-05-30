using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace SampleWpfApplication.Views.Converters
{
    public class EnumItemsSourceByDescriptionConverter : ObservableCollection<String>, IValueConverter
    {
        private Type _type;
        private IDictionary<Object, Object> _valueToNameMap;
        private IDictionary<Object, Object> _nameToValueMap;

        public Type Type
        {
            get { return _type; }
            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("Type is not an enum", "value");
                _type = value;
                Initialize();
            }
        }

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                var underlyingValue = value;
                if (value is int)
                    underlyingValue = Enum.Parse(_type, Enum.GetName(_type, value));

                return _valueToNameMap[underlyingValue];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return _nameToValueMap[value];
        }

        void Initialize()
        {
            _valueToNameMap = _type
              .GetFields(BindingFlags.Static | BindingFlags.Public)
              .ToDictionary(fi => fi.GetValue(null), GetDescription);
            _nameToValueMap = _valueToNameMap
              .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            Clear();
            foreach (String name in _nameToValueMap.Keys)
                Add(name);
        }

        static Object GetDescription(FieldInfo fieldInfo)
        {
            var descriptionAttribute =
              (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return descriptionAttribute != null ? descriptionAttribute.Description : fieldInfo.Name;
        }
    }
}
