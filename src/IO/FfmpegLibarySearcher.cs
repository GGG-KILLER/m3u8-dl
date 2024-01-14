using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace m3u8Dl.IO;

public static class FfmpegLibarySearcher
{
    private const string LIB_NAME = "libavutil.so";
    private static readonly ImmutableArray<string> libraryPaths = [
        "/usr/lib/x86_64-linux-gnu",
        "/usr/lib/",
        "/usr/lib64/",
        "/lib/x86_64-linux-gnu"
    ];

    public static string? FindFfmpegLibraryPath(CancellationToken cancellationToken = default)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            foreach (var path in libraryPaths)
            {
                if (Directory.Exists(path) && HasFfmpeg(path))
                    return path;
            }

            var libaryPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")?.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (libaryPath?.Length > 0)
            {
                foreach (var path in libaryPath)
                {
                    if (HasFfmpeg(path))
                        return path;
                }
            }

            static bool HasFfmpeg(string path) => Directory.EnumerateFiles(path, $"{LIB_NAME}*").Any();
        }
        return null;
    }
}
