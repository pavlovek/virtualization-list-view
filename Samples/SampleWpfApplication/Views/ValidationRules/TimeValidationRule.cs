using System;
using System.Globalization;
using System.Windows.Controls;

namespace SampleWpfApplication.Views.ValidationRules
{
    public class TimeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var val = value as string;
            TimeSpan ts = new TimeSpan();
            if (TimeSpan.TryParse(val, out ts) && val.Contains(":"))
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "Please, enter correct time. For example: 06:00");

        }
    }
}
