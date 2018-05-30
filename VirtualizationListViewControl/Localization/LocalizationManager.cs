using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace VirtualizationListViewControl.Localization
{
    /// <summary>
    /// Localization manager
    /// </summary>
    internal static class LocalizationManager
    {
        /// <summary>
        /// Default localization
        /// </summary>
        private static ResourceManager _defaultLocalization;

        /// <summary>
        /// Set specify localization
        /// </summary>
        /// <param name="localization">Specify localization</param>
        public static void SetLocalization(ResourceManager localization)
        {
            if (localization == null)
                return;

            if (_defaultLocalization == null)
                _defaultLocalization = LocalizationDictionary.ResourceManager;

            var resManagerProp =
                typeof(LocalizationDictionary).GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
            resManagerProp.SetValue(null, localization);
        }

        /// <summary>
        /// Set default localization
        /// </summary>
        public static void SetDefaultLocalization()
        {
            var resManagerProp =
                typeof(LocalizationDictionary).GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
            resManagerProp.SetValue(null, _defaultLocalization);
        }

        /// <summary>
        /// Get localized field value
        /// </summary>
        /// <param name="fieldName">Field Name</param>
        /// <returns>Localized field value</returns>
        public static string GetLocalizedValue(string fieldName)
        {
            if (String.IsNullOrWhiteSpace(fieldName))
                return String.Empty;

            return LocalizationDictionary.ResourceManager.GetString(fieldName);
        }

        /// <summary>
        /// Get specify field name by his localized value
        /// </summary>
        /// <param name="localizedValue">Field's localized value</param>
        /// <returns>Filed name</returns>
        public static string GetFieldNameByLocalizatedValue(string localizedValue)
        {
            if (String.IsNullOrWhiteSpace(localizedValue))
                return String.Empty;

            var entry = LocalizationDictionary.ResourceManager
                .GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true)
                .OfType<DictionaryEntry>()
                .FirstOrDefault(e => e.Value.ToString() == localizedValue);
            var key = entry.Key.ToString();

            return key;
        }
    }
}
