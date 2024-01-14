using Cocona;
using DotNetTools.SharpGrabber.Converter;
using m3u8Dl.Abstractions;
using m3u8Dl.IO;
using m3u8Dl.Model;
using m3u8Dl.Net;

await CoconaApp.RunAsync(async (CurlParameters curlParams, ProgramParameters programParams, [Argument] Uri streamUri, CoconaAppContext ctx) =>
{
    var baseUri = new UriBuilder(streamUri);
    baseUri.Path = Path.GetDirectoryName(baseUri.Path) + '/';

    var tempPath = programParams.TempPath;
    if (string.IsNullOrWhiteSpace(tempPath))
        tempPath = Path.GetTempPath();

    var reporter = new ConsoleProgressReporter(programParams.ColorMode, programParams.QuietMode);

    if (!string.IsNullOrWhiteSpace(programParams.FfmpegPath))
    {
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
        }
    }

    string ffmpegVersion;
    try
    {
        ffmpegVersion = MediaLibrary.FFMpegVersion();
    }
    catch
    {
        await reporter.ReportError($"Could not find ffmpeg libraries. Please provide them using the --ffmpeg flag.");
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
    return 0;
});
