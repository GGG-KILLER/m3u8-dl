using m3u8Dl.Abstractions;

namespace m3u8Dl.Model;

public sealed record DownloadSettings(
    Uri BaseUri,
    IEnumerable<Header> Headers,
    string TempPath,
    string OutputPath,
    IProgressReporter ProgressReporter
)
{
    public IStreamSelector StreamSelector { get; init; } = new MaxResolutionStreamSelector();
}
