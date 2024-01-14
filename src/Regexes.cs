using System.Text.RegularExpressions;

namespace m3u8Dl;

public static partial class Regexes
{
    /// <summary>
    /// A regex to parse a header value according to Section 2.2 of RFC2822 (https://datatracker.ietf.org/doc/html/rfc2822#section-2.2).
    /// </summary>
    /// <remarks>
    /// This does not account for other Unicode characters because the RFC mandates that names and values be US-ASCII.
    /// </remarks>
    /// <returns></returns>
    [GeneratedRegex(@"^(?<name>[\x24-\x39\x3B-\x7E]+)[ \t]*:[ \t]*(?<value>[\x00-\x09\x0B-\x0C\x0E-\x7F]+)$")]
    public static partial Regex Header();
}
