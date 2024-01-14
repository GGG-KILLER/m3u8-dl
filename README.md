# m3u8-dl

This is a tool that's meant to help people trying to download HLS streams easily.

## Requirements

To run this, you'll need to have the FFmpeg libraries installed.

## Quick Start

To use this, go into the Chrome DevTools Networking tab and copy the m3u8 request as curl.

Then replace `curl` with `m3u8-dl`, add the `-o` flag for the output.

## Caveats

This tool, **on Linux**, attempts to find the FFmpeg libraries on its own however it cannot always be successful, for this reason, the `--ffmpeg` flag is available to tell the program where the FFmpeg libraries are located.
Alternatively, **if on Linux**, one can also provide the location of the FFmpeg libraries through the `LD_LIBRARY_PATH` environment variable.

## Parameters

```
Usage: m3u8-dl [--header <Header>...] [--compressed] [--ffmpeg <String>] [--temp-path <String>] [--choose-stream] [--output <String>] [--quiet] [--color <ColorMode>] [--help] [--version] stream-uri

m3u8-dl

Arguments:
  0: stream-uri     (Required)

Options:
  -H, --header <Header>...    Header to use when contacting the server (Required)
  --compressed                Ignored by the application, but kept to minimize modifications to the copied command line
  --ffmpeg <String>           The path to the ffmpeg libraries. Optional in case the program can find the ffmpeg version on its own.
  --temp-path <String>        The temporary path to which fragments will be downloaded to.
  --choose-stream             Whether to display available HLS streams for the user to choose instead of picking the highest resolution one.
  -o, --output <String>       Where to save the final file to. (Required)
  -q, --quiet                 Whether to not output any information-level logs.
  --color <ColorMode>         The color mode to use when running this program. (Default: Auto) (Allowed values: Never, Auto, Always)
  -h, --help                  Show help message
  --version                   Show version
```