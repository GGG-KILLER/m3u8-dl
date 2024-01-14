using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace m3u8Dl.Model;

[TypeConverter(typeof(HeaderTypeConverter))]
public readonly record struct Header(string Name, string Value)
{
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Name}: {Value}");
}

public sealed class HeaderTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) => destinationType == typeof(string);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string strValue)
            throw new FormatException("Cannot convert a non-string to a header.");

        var match = Regexes.Header().Match(strValue);
        if (!match.Success)
            throw new FormatException($"Value '{strValue}' is not a valid header value.");

        return new Header(match.Groups["name"].Value, match.Groups["value"].Value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
            throw new InvalidOperationException("Cannot convert to types that are not strings.");
        if (value is not Header header)
            throw new InvalidOperationException("Cannot convert to string values that are not of type Header.");

        return header.ToString();
    }
}