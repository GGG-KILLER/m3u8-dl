using DotNetTools.SharpGrabber.Grabbed;

namespace m3u8Dl.Abstractions;

public interface IStreamSelector
{
    GrabbedHlsStreamMetadata SelectStream(IReadOnlyList<GrabbedHlsStreamMetadata> streams);
}
