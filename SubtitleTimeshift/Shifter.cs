using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            using (var reader = new StreamReader(input, encoding, false, bufferSize, leaveOpen))
            using (var writer = new StreamWriter(output, encoding, bufferSize, leaveOpen))
            {
                string line;
                var arrow = " --> ";
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var outputLine = line;
                    var stamps = line.Split(new[] { arrow }, StringSplitOptions.RemoveEmptyEntries);
                    if (stamps.Length == 2)
                    {
                        var start = shiftStamp(stamps[0], timeSpan);
                        var end = shiftStamp(stamps[1], timeSpan);
                        outputLine = $"{start}{arrow}{end}";
                    }

                    await writer.WriteLineAsync(outputLine);
                }
            }
        }

        static string shiftStamp(string stampText, TimeSpan timeSpan)
        {
            var nums = stampText.Split(new[] { ':', ',', '.', '|' })
                                .Select(s => int.Parse(s))
                                .ToArray();

            var result = new TimeSpan(0, nums[0], nums[1], nums[2], nums[3])
                          .Add(timeSpan)
                          .ToString(@"hh\:mm\:ss\.fff");

            return result;
        }
    }
}
