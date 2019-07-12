using System;
using System.ComponentModel;

namespace LibraryCompare.Core.Enums
{
    public class EnumToStringUsingDescription : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(Enum));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (!(destinationType == typeof(string)))
            {
                throw new ArgumentException("Can only convert to string.", nameof(destinationType));
            }

            if (!(value.GetType().BaseType == typeof(Enum)))
            {
                throw new ArgumentException("Can only convert an instance of enum.", nameof(value));
            }

            var name = value.ToString();
            var attrs = value.GetType().GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attrs.Length > 0) ? ((DescriptionAttribute)attrs[0]).Description : name;
        }
    }
}
