using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace m3u8Dl.IO;

public static class FfmpegLibarySearcher
{
    private static FunctionResolverBase? _functionResolver;

    private static readonly ImmutableArray<string> libraryPaths = [
        "/usr/lib/x86_64-linux-gnu",
        "/usr/lib/",
        "/usr/lib64/",
        "/lib/x86_64-linux-gnu"
    ];

    public static string GetNativeLibraryName(string libraryName)
    {
        if (_functionResolver is null)
        {
            Interlocked.CompareExchange(ref _functionResolver, (FunctionResolverBase)FunctionResolverFactory.Create(), null);
        }

        var version = ffmpeg.LibraryVersionMap[libraryName];
        return UnsafeGetNativeLibraryName(_functionResolver, libraryName, version);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetNativeLibraryName")]
        extern static string UnsafeGetNativeLibraryName(FunctionResolverBase functionResolver, string libraryName, int version);
    }

    public static IEnumerable<string> ListNotFoundNativeLibraries(string ffmpegPath)
    {
        foreach (var versionlessName in ffmpeg.LibraryVersionMap.Keys)
        {
            var name = GetNativeLibraryName(versionlessName);
            if (!File.Exists(Path.Combine(ffmpegPath, name)))
                yield return name;
        }
    }

    public static string? FindFfmpegLibraryPath(CancellationToken cancellationToken = default)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            foreach (var path in libraryPaths)
            {
                if (Directory.Exists(path) && !ListNotFoundNativeLibraries(path).Any())
                    return path;
            }

            var libaryPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (libaryPath?.Length > 0)
            {
                foreach (var path in libaryPath)
                {
                    if (!ListNotFoundNativeLibraries(path).Any())
                        return path;
                }
            }
        }
        return null;
    }
}
