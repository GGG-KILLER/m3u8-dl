namespace m3u8Dl.Abstractions;

public interface IProgressReporter
{
    /// <summary>
    /// Sets the current task being executed, to print when progress is updated.
    /// </summary>
    /// <param name="description">
    /// The current task being executed, in the present tense (e.g.: "Downloading file X")
    /// </param>
    ValueTask SetCurrentTask(string description);

    /// <summary>
    /// Reports the progress on the current task with the processing counts.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    ValueTask ReportProgress(long current, long total);

    /// <summary>
    /// Begins a scope for an activity without printing anything.
    /// </summary>
    /// <returns></returns>
    ValueTask<IAsyncDisposable> BeginScope();
    /// <summary>
    /// Begins a scope for an activite and prints the provided message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    ValueTask<IAsyncDisposable> BeginScope(string message);

    ValueTask ReportInformation(string message);
    ValueTask ReportWarning(string message);
    ValueTask ReportError(string message);
}
