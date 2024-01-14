using Cocona;

namespace m3u8Dl.Model;

public sealed record CurlParameters(

    [Option("header", ['H'], Description = "Header to use when contacting the server")]
    [ValidHeader]
    IReadOnlyList<Header> Headers,

    [Option("compressed", Description = "Ignored by the application, but kept to minimize modifications to the copied command line")]
    bool Ignored1

) : ICommandParameterSet;
