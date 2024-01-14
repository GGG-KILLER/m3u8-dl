using System.Globalization;
using DotNetTools.SharpGrabber.Grabbed;

namespace m3u8Dl.Abstractions;

public sealed class ConsoleStreamSelector : IStreamSelector
{
    public GrabbedHlsStreamMetadata SelectStream(IReadOnlyList<GrabbedHlsStreamMetadata> streams)
    {
        if (streams.Count == 1)
            return streams[0];

        var idx = -1;
        do
        {
            Console.WriteLine("Please select a stream from the following:");
            foreach (var stream in streams)
                Console.WriteLine($"""
                      {idx}. {stream.Name}
                        Resolution = {stream.Resolution}
                        Bandwidth = {stream.Bandwidth}
                        Format = {stream.StreamFormat}
                    """);

            if (!int.TryParse(Console.ReadLine(), NumberStyles.None, CultureInfo.CurrentUICulture, out idx))
            {
                Console.WriteLine("Invalid number picked, please try again.");
                idx = -1;
            }
        }
        while (idx < 0 || idx >= streams.Count);

        return streams[idx];
    }
}