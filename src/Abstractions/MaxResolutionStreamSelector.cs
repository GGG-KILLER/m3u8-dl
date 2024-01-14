using DotNetTools.SharpGrabber.Grabbed;

namespace m3u8Dl.Abstractions;

public sealed class MaxResolutionStreamSelector : IStreamSelector
{
    public GrabbedHlsStreamMetadata SelectStream(IReadOnlyList<GrabbedHlsStreamMetadata> streams) =>
        streams.OrderByDescending(s => s.Resolution?.Height ?? 0).ThenByDescending(s => s.Bandwidth).First();
}
