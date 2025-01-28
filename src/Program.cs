using Cocona;
using DotNetTools.SharpGrabber.Converter;
using m3u8Dl.Abstractions;
using m3u8Dl.IO;
using m3u8Dl.Model;
using m3u8Dl.Net;

await CoconaLiteApp.RunAsync(async (CurlParameters curlParams, ProgramParameters programParams, [Argument] Uri streamUri, CoconaAppContext ctx) =>
{
    var baseUri = new UriBuilder(streamUri);
    baseUri.Path = Path.GetDirectoryName(baseUri.Path) + '/';

    var tempPath = programParams.TempPath;
    if (string.IsNullOrWhiteSpace(tempPath))
        tempPath = Path.GetTempPath();

    var reporter = new ConsoleProgressReporter(programParams.ColorMode, programParams.QuietMode);

    if (!string.IsNullOrWhiteSpace(programParams.FfmpegPath))
    {
        var failed = false;
        await using var _ = await reporter.BeginScope("Checking ffmpeg library path...");
        foreach (var notFound in FfmpegLibarySearcher.ListNotFoundNativeLibraries(programParams.FfmpegPath))
        {
            failed = true;
            await reporter.ReportWarning($"Library not found: {notFound}");
        }
        if (failed)
            return -1;

        MediaLibrary.Load(programParams.FfmpegPath);
    }
    else
    {
        var path = FfmpegLibarySearcher.FindFfmpegLibraryPath(ctx.CancellationToken);
        if (!string.IsNullOrWhiteSpace(path))
        {
            MediaLibrary.Load(path);
        }
        else
        {
            await reporter.ReportWarning("Couldn't find ffmpeg library path automatically.");
            return -1;
        }
    }

    string ffmpegVersion;
    try
    {
        ffmpegVersion = MediaLibrary.FFMpegVersion();
    }
    catch (Exception ex)
    {
        await reporter.ReportError($"Could not load ffmpeg libraries. Please ensure you have provided them using the --ffmpeg flag.");
        await reporter.ReportError(ex.ToString());
        return -1;
    }

    await reporter.ReportInformation($"Using FFmpeg version {ffmpegVersion}");

    var settings = new DownloadSettings(
        baseUri.Uri,
        curlParams.Headers,
        tempPath,
        programParams.OutputPath,
        reporter)
    {
        StreamSelector = programParams.LetUserChooseStream ? new ConsoleStreamSelector() : new MaxResolutionStreamSelector()
    };

    using var downloader = new StreamDownloader(settings);
    try { await downloader.Download(streamUri, ctx.CancellationToken); }
    catch (OperationCanceledException) { }
    catch (Exception ex) { await reporter.ReportError(ex.ToString()); return -2; }
    return 0;
});
