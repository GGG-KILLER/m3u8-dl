using System.ComponentModel;
using m3u8Dl.Model;

namespace m3u8Dl.Test.Model;

public class HeaderTypeConverterTests
{
    [Theory]
    [InlineData("authority: example.com", "authority", "example.com")]
    [InlineData("accept: */*", "accept", "*/*")]
    [InlineData("accept-language: en-US", "accept-language", "en-US")]
    [InlineData("cache-control: no-cache", "cache-control", "no-cache")]
    [InlineData("dnt: 1", "dnt", "1")]
    [InlineData("origin: https://example.com", "origin", "https://example.com")]
    [InlineData("pragma: no-cache", "pragma", "no-cache")]
    [InlineData("referer: https://example.com/", "referer", "https://example.com/")]
    [InlineData("sec-ch-ua: \"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"", "sec-ch-ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"")]
    [InlineData("sec-ch-ua-mobile: ?0", "sec-ch-ua-mobile", "?0")]
    [InlineData("sec-ch-ua-platform: \"Linux\"", "sec-ch-ua-platform", "\"Linux\"")]
    [InlineData("sec-fetch-dest: empty", "sec-fetch-dest", "empty")]
    [InlineData("sec-fetch-mode: cors", "sec-fetch-mode", "cors")]
    [InlineData("sec-fetch-site: cross-site", "sec-fetch-site", "cross-site")]
    [InlineData("user-agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36", "user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")]
    public void HeaderTypeConverter_ConvertFrom_CorrectlyParsesValues(string raw, string name, string value)
    {
        // Given
        var converter = TypeDescriptor.GetConverter(typeof(Header));

        // When
        var result = converter.ConvertFrom(raw);

        // Then
        var header = Assert.IsType<Header>(result);
        Assert.Equal(name, header.Name);
        Assert.Equal(value, header.Value);
    }

    [Theory]
    [InlineData("accept\r\n: something")]
    [InlineData("accept: some\r\nthing")]
    public void HeaderTypeConverter_ConvertFrom_ThrowsExceptionsForInvalidHeaders(string raw)
    {
        // Given
        var converter = TypeDescriptor.GetConverter(typeof(Header));

        // Then
        Assert.Throws<FormatException>(() => converter.ConvertFrom(raw));
    }

    [Theory]
    [MemberData(nameof(TestHeaders))]
    public void HeaderTypeConverter_ConvertTo_CorrectlyFormatsHeaders(Header header, string expected)
    {
        // Given
        var converter = TypeDescriptor.GetConverter(header);

        // When
        var converted = converter.ConvertTo(header, typeof(string));

        // Then
        var str = Assert.IsType<string>(converted);
        Assert.Equal(expected, str);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(long))]
    [InlineData(typeof(object))]
    public void HeaderTypeConverter_ConvertTo_ThrowsExceptionForInvalidTypes(Type type)
    {
        // Given
        var header = new Header("accept", "*/*");
        var converter = TypeDescriptor.GetConverter(header);

        // Then
        Assert.Throws<InvalidOperationException>(() => converter.ConvertTo(header, type));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2L)]
    [InlineData("hello there")]
    public void HeaderTypeConverter_ConvertTo_ThrowsExceptionForInvalidObjects(object value)
    {
        // Given
        var converter = TypeDescriptor.GetConverter(typeof(Header));

        // Then
        Assert.Throws<InvalidOperationException>(() => converter.ConvertTo(value, typeof(string)));
    }

    public static IEnumerable<object[]> TestHeaders => [
        [new Header("Dnt", "1"), "Dnt: 1"],
        [new Header("authority", "example.com"), "authority: example.com"],
        [new Header("accept", "*/*"), "accept: */*"],
        [new Header("accept-language", "en-US"), "accept-language: en-US"],
        [new Header("cache-control", "no-cache"), "cache-control: no-cache"],
        [new Header("dnt", "1"), "dnt: 1"],
        [new Header("origin", "https://example.com"), "origin: https://example.com"],
        [new Header("pragma", "no-cache"), "pragma: no-cache"],
        [new Header("referer", "https://example.com/"), "referer: https://example.com/"],
        [new Header("sec-ch-ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\""), "sec-ch-ua: \"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\""],
        [new Header("sec-ch-ua-mobile", "?0"), "sec-ch-ua-mobile: ?0"],
        [new Header("sec-ch-ua-platform", "\"Linux\""), "sec-ch-ua-platform: \"Linux\""],
        [new Header("sec-fetch-dest", "empty"), "sec-fetch-dest: empty"],
        [new Header("sec-fetch-mode", "cors"), "sec-fetch-mode: cors"],
        [new Header("sec-fetch-site", "cross-site"), "sec-fetch-site: cross-site"],
        [new Header("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"), "user-agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"],
    ];
}
