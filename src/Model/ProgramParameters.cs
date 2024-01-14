using Cocona;
using m3u8Dl.Abstractions;
using m3u8Dl.Validation;

namespace m3u8Dl.Model;

public sealed record ProgramParameters(

    [Option("ffmpeg", Description = "The path to the ffmpeg libraries. Optional in case the program can find the ffmpeg version on its own.")]
    [DirectoryExists]
    string? FfmpegPath,

    [Option("temp-path", Description = "The temporary path to which fragments will be downloaded to.")]
    [DirectoryExists]
    string? TempPath,

    [Option("choose-stream", Description = "Whether to display available HLS streams for the user to choose instead of picking the highest resolution one.")]
    bool LetUserChooseStream,

    [Option("output", ['o'], Description = "Where to save the final file to.")]
    string OutputPath,

    [Option("quiet", ['q'], Description = "Whether to not output any information-level logs.")]
    bool QuietMode,

    [Option("color", Description = "The color mode to use when running this program.")]
    ColorMode ColorMode = ColorMode.Auto
) : ICommandParameterSet;
