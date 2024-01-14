using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Converter;
using DotNetTools.SharpGrabber.Grabbed;
using m3u8Dl.Model;

namespace m3u8Dl.Net;

public class StreamDownloader : IDisposable
{
    private readonly DownloadSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly IGrabber _grabber;
    private bool disposedValue;

    public StreamDownloader(DownloadSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _httpClient = new HttpClient { BaseAddress = settings.BaseUri };
        foreach (var header in settings.Headers)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);
        }

        _grabber = GrabberBuilder.New()
            .UseServices(new GrabberServices(httpClientProvider: () => _httpClient))
            .AddHls()
            .Build();
    }

    public async Task Download(Uri streamUri, CancellationToken cancellationToken = default)
    {
        await using var _ = await _settings.ProgressReporter.BeginScope($"Downloading stream from {streamUri}...");
        var grabResult = await _grabber.GrabAsync(streamUri, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var reference = grabResult.Resource<GrabbedHlsStreamReference>();
        if (reference != null)
        {
            // Redirect to an M3U8 playlist
            await Download(reference.ResourceUri, cancellationToken);
            return;
        }

        var metadataResources = grabResult.Resources<GrabbedHlsStreamMetadata>().ToArray();
        if (metadataResources.Length > 0)
        {
            // Pick a stream
            var stream = _settings.StreamSelector.SelectStream(metadataResources);
            cancellationToken.ThrowIfCancellationRequested();

            // Get information from the HLS stream
            GrabbedHlsStream? grabbedStream = await stream.Stream.Value;
            await Download(grabbedStream, stream, grabResult, cancellationToken);
            return;
        }

        await _settings.ProgressReporter.ReportInformation($"Unknown results obtained from URL: [{string.Join(", ", grabResult.Resources.Select(g => g.GetType().FullName))}]").ConfigureAwait(false);
        await _settings.ProgressReporter.ReportError("Unable to obtain the HLS stream from the provided URL.").ConfigureAwait(false);
    }

    private async Task Download(GrabbedHlsStream stream, GrabbedHlsStreamMetadata metadata, GrabResult grabResult, CancellationToken cancellationToken)
    {
        var segmentCount = stream.Segments.Count;

        await _settings.ProgressReporter.ReportInformation($"Segment count: {segmentCount}").ConfigureAwait(false);
        await _settings.ProgressReporter.ReportInformation($"Stream duration: {stream.Length}").ConfigureAwait(false);

        var outputPath = _settings.OutputPath;
        var outputExt = Path.GetExtension(outputPath)[1..];
        await _settings.ProgressReporter.ReportInformation($"Stream format: {metadata.StreamFormat.Mime} (.{metadata.StreamFormat.Extension})");
        await _settings.ProgressReporter.ReportInformation($"Output format: {metadata.OutputFormat.Mime} (.{metadata.OutputFormat.Extension})");
        if (outputExt != metadata.OutputFormat.Extension)
        {
            await _settings.ProgressReporter.ReportWarning($"Provided output file has a different extension ({outputExt}) than the suggested one ({metadata.OutputFormat.Extension}).").ConfigureAwait(false);
            await _settings.ProgressReporter.ReportWarning($"This means that the final file may not have the correct extension (and by extension, not work properly).").ConfigureAwait(false);
        }

        var segmentFiles = new List<string>();
        try
        {
            await using (var _ = await _settings.ProgressReporter.BeginScope("Downloading segments..."))
            {
                await _settings.ProgressReporter.SetCurrentTask($"Downloading {segmentCount} segments...");
                await _settings.ProgressReporter.ReportProgress(0, segmentCount);
                for (var i = 0; i < segmentCount; i++)
                {
                    var segment = stream.Segments[i];

                    var segmentPath = Path.Combine(_settings.TempPath, $"{Guid.NewGuid():N}.m3u8-dl");
                    segmentFiles.Add(segmentPath);

                    using var responseStream = await _httpClient.GetStreamAsync(segment.Uri, cancellationToken);
                    using var inputStream = await grabResult.WrapStreamAsync(responseStream);
                    using var outputStream = new FileStream(segmentPath, FileMode.Create);
                    await inputStream.CopyToAsync(outputStream, cancellationToken);

                    await _settings.ProgressReporter.ReportProgress(i + 1, segmentCount);
                }
            }
            await _settings.ProgressReporter.ReportInformation("Finished downloading segments.");

            await CreateOutputFile(segmentFiles, cancellationToken);
        }
        finally
        {
            await using (var _ = await _settings.ProgressReporter.BeginScope("Cleaning up temporary files..."))
            {
                foreach (var tempFile in segmentFiles)
                {
                    try
                    {
                        if (File.Exists(tempFile))
                            File.Delete(tempFile);
                    }
                    catch
                    {
                        await _settings.ProgressReporter.ReportError($"Failed to delete temporary file: {tempFile}");
                    }
                }
            }
            await _settings.ProgressReporter.ReportInformation("Temporary files cleaned up.");
        }
    }

    private async Task CreateOutputFile(List<string> segmentFiles, CancellationToken cancellationToken)
    {
        await _settings.ProgressReporter.ReportInformation("Converting downloaded stream...");

        var concatenator = new MediaConcatenator(_settings.OutputPath)
        {
            OutputMimeType = MimeTypes.GetMimeType(_settings.OutputPath),
            OutputExtension = Path.GetExtension(_settings.OutputPath)[1..],
        };
        foreach (var segmentFile in segmentFiles)
            concatenator.AddSource(segmentFile);
        cancellationToken.ThrowIfCancellationRequested();
        concatenator.Build();

        await _settings.ProgressReporter.ReportInformation("Output file generated successfully.");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _httpClient.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~StreamDownloader()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
