using System;
using System.Globalization;
using System.Windows.Controls;
using Common;

namespace EventsReceiver.ValidationRules
{
    class AddressValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var adress = value as string;

                if (string.IsNullOrEmpty(adress))
                    throw new UriFormatException();

                if (!adress.StartsWith(HttpGlobals.SchemaPrefix))
                    adress = HttpGlobals.SchemaPrefix + adress;

                new Uri(adress);
            }
            catch (UriFormatException)
            {
                return new ValidationResult(false, "Invalid address");
            }

            return new ValidationResult(true, null);
        }
    }
}
