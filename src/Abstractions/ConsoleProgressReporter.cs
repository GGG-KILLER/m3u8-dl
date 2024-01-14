using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace m3u8Dl.Abstractions;

public enum ColorMode
{
    Never,
    Auto,
    Always
}

public sealed class ConsoleProgressReporter(ColorMode colorMode = ColorMode.Auto, bool quietMode = false) : IProgressReporter
{
    private readonly Stack<Scope> _scopes = new();
    private string? _currentTask;
    private ulong _currentPos = ulong.MaxValue;

    private void WriteWithColor(ConsoleColor color, string text)
    {
        Unsafe.SkipInit(out ConsoleColor c);
        if (colorMode == ColorMode.Always || (colorMode == ColorMode.Auto && !Console.IsInputRedirected))
        {
            c = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }
        Console.Write(text);
        if (colorMode == ColorMode.Always || (colorMode == ColorMode.Auto && !Console.IsInputRedirected))
        {
            Console.ForegroundColor = c;
        }
    }

    private void WriteIndent() => Console.Write(new string(' ', _scopes.Count * 2));

    public ValueTask ReportError(string value)
    {
        _currentPos = ulong.MaxValue;
        WriteIndent();
        WriteWithColor(ConsoleColor.Red, "FAIL: ");
        Console.WriteLine(value);
        return ValueTask.CompletedTask;
    }

    public ValueTask ReportInformation(string value)
    {
        if (!quietMode)
        {
            _currentPos = ulong.MaxValue;
            WriteIndent();
            WriteWithColor(ConsoleColor.Blue, "INFO: ");
            Console.WriteLine(value);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask ReportWarning(string value)
    {
        _currentPos = ulong.MaxValue;
        WriteIndent();
        WriteWithColor(ConsoleColor.Yellow, "WARN: ");
        Console.WriteLine(value);
        return ValueTask.CompletedTask;
    }

    public async ValueTask ReportProgress(long current, long total)
    {
        Unsafe.SkipInit(out ulong pos);
        if (!Console.IsInputRedirected)
        {
            if (_currentPos != ulong.MaxValue)
            {
                var (top, left) = unpack(_currentPos);
                Console.SetCursorPosition(left, top);
            }

            // Gotta add a line if we're at the end of the "screen"
            var y = Console.CursorTop;
            if (y == Console.BufferHeight - 1)
            {
                Console.WriteLine();
                Console.SetCursorPosition(0, --y);
            }

            pos = pack(Console.CursorTop, Console.CursorLeft);
        }

        await ReportInformation($"{_currentTask} {current / (double)total:P} ({current}/{total})");

        if (!Console.IsInputRedirected)
            _currentPos = pos;

        static ulong pack(int a, int b)
        {
            Span<int> vals = [a, b];
            return MemoryMarshal.Cast<int, ulong>(vals)[0];
        }

        static (int, int) unpack(ulong val)
        {
            var spanA = new Span<ulong>(ref val);
            var spanB = MemoryMarshal.Cast<ulong, int>(spanA);
            return (spanB[0], spanB[1]);
        }
    }

    public ValueTask SetCurrentTask(string description)
    {
        _currentPos = ulong.MaxValue;
        _currentTask = description;
        return ValueTask.CompletedTask;
    }

    public ValueTask<IAsyncDisposable> BeginScope()
    {
        var scope = new Scope(this, null);
        _scopes.Push(scope);
        return ValueTask.FromResult<IAsyncDisposable>(scope);
    }

    public async ValueTask<IAsyncDisposable> BeginScope(string message)
    {
        await ReportInformation(message);

        var scope = new Scope(this, message);
        _scopes.Push(scope);
        return scope;
    }

    private sealed class Scope(ConsoleProgressReporter self, string? name) : IAsyncDisposable
    {
        private readonly string? _name = name;

        public ValueTask DisposeAsync()
        {
            var popped = self._scopes.Pop();
            if (!ReferenceEquals(popped, this))
                throw new InvalidOperationException($"Scope '{_name}' was not disposed in the correct order (scope '{popped._name}' was disposed instead).");
            return ValueTask.CompletedTask;
        }
    }
}
