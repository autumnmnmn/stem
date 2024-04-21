using Utilities.DefaultDictionary;
using Utilities.Extensions;
using Utilities.Launcher;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Sandbox.ScratchPad;

public class ScratchPad : IRunnable
{

    public static class FB
    {
        private record FBDatum(int Index, string? Name = null);

        private static IEnumerable<FBDatum> Nums()
        {
            int iterator = 1;
            while (true)
            {
                yield return new FBDatum(iterator++);
            }
        }

        private static Func<FBDatum, FBDatum> CreateRuleFunction((int mod, string name) ruleDescriptor)
        {
            return ch => (ch.Index % ruleDescriptor.mod, ch.Name) switch
            {
                (0, null) => new FBDatum(ch.Index, ruleDescriptor.name),
                (0, string s) => new FBDatum(ch.Index, $"{s}{ruleDescriptor.name}"),
                _ => ch
            };
        }

        private static string Resolve(FBDatum c)
        {
            return c.Name ?? c.Index.ToString();
        }

        public static IEnumerable<string> Run(int count, params (int, string)[] rules)
        {
            var data = Nums().Take(count);
            foreach (var ruleFunction in rules.Select(CreateRuleFunction))
            {
                data = data.Select(ruleFunction);
            }
            return data.Select(Resolve);
        }
    }
    
    public enum TransmissionCode {
        EXIT = 69420,
        COMMAND = 3,
        IMAGE_RESULT = 1
    }

    public void Run() {
        Console.WriteLine("foo");
    }

    public void Run_0() {
        var l = new SocketListener<TransmissionCode>(TransmissionCode.EXIT, "/tmp/sd_transmitter.s");

        l.On(TransmissionCode.COMMAND, (contentLength, content) => {
            Console.WriteLine($"RECEIVED A COMMAND WITH {contentLength} BYTES OF DATA:");
            Console.WriteLine(Encoding.ASCII.GetString(content));
        });

        l.On(TransmissionCode.IMAGE_RESULT, (contentLength, content) => {
            Console.WriteLine($"{contentLength} bytes");
            var w = BitConverter.ToInt32(content, 0);
            var h = BitConverter.ToInt32(content, 4);
            Console.WriteLine($"({w} x {h} x 3) + 2 = {w * h * 3 + 2}");
        });

        l.Start();

        Console.ReadKey();

        l.Cancel();
    }

    

    public void DrilRun()
    {
        var lines = File.ReadAllLines("~/data0/media/Documents/Corpi/all_dril_tweets.txt");

        bool nextLineIsATweet = true;

        var tweets = new List<string>();

        foreach (var line in lines)
        {
            if (nextLineIsATweet)
            {
                tweets.Add(line);
                nextLineIsATweet = !nextLineIsATweet;
            }
            else if (string.IsNullOrEmpty(line))
            {
                nextLineIsATweet = !nextLineIsATweet;
            }
        }

        var rng = new Random();

        foreach (var tweet in tweets)
        {
            if (rng.NextDouble() < 0.001)
            {
                tweet.Dump();
                "".Dump();
            }
        }
    }
}

internal static class ScratchPadExtensions
{
    public static void Dump<T>(this T @object) => Console.WriteLine(@object?.ToString() ?? "<null>");
}
