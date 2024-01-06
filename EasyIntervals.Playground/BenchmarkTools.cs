using System.Security.Cryptography;

namespace EasyIntervals.Playground;

public class BenchmarkTools
{
    public static Interval<int> CreateRandomInterval(int maxStartLimit, int maxIntervalLength)
    {
        var start = RandomNumberGenerator.GetInt32(0, maxStartLimit + 1);
        var length = RandomNumberGenerator.GetInt32(1, maxIntervalLength + 1);

        return new Interval<int>(start, start + length);
    }

    public static ISet<Interval<int>> CreateRandomIntervals(int totalIntervalsCount, int maxStartLimit, int maxIntervalLength)
    {
        var random = new Random();
        var intervals = Enumerable.Range(0, totalIntervalsCount)
            .Select(i =>
            {
                var start = RandomNumberGenerator.GetInt32(maxStartLimit);
                var end = RandomNumberGenerator.GetInt32(start, start + maxIntervalLength + 1);
                return new Interval<int>(start, end, IntervalType.Closed);
            })
            .ToHashSet();

        return intervals;
    }
}
