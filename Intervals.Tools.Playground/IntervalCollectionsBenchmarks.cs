using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IntervalTree;

namespace Intervals.Tools.Playground;

[ShortRunJob]
// [NativeMemoryProfiler]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class IntervalCollectionsBenchmarks
{
    private const int TotalIntervalsCount = 1_000_000;
    private const int MaxStartLimit = 10_000_000;
    private const int MaxIntervalLength = 1_000;

    private static List<Interval<int>> InitRandomIntervals()
    {
        var random = new Random();
        var intervals = Enumerable.Range(0, TotalIntervalsCount)
            .Select(i =>
            {
                var start = random.Next(MaxStartLimit);
                var end = random.Next(start, start + MaxIntervalLength);
                return new Interval<int>(start, end, IntervalType.Closed);
            })
            .Distinct()
            .ToList();

        return intervals;
    }

    private readonly List<Interval<int>> _intervals;
    private readonly List<Interval<int>> _seededIntersectionIntervals;
    private readonly Random _random;

    public IntervalCollectionsBenchmarks()
    {
        _random = new Random();
        _intervals = InitRandomIntervals();
        _seededIntersectionIntervals = Enumerable.Range(0, 1_000)
            .Select(i => CreateRandomInterval(0, MaxStartLimit))
            .ToList();
    }

    [Benchmark]
    public void InitializeIntervalSet()
    {
        var intervalSet = new IntervalSet<int>(_intervals);
    }

    [Benchmark]
    public void InitializeIntervalTree()
    {
        var intervalTree = new IntervalTree<int, string>();
        foreach (var itv in _intervals)
        {
            intervalTree.Add(itv.Start, itv.End, $"{itv.Start}, {itv.End}");
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersectionsIntervalSet()
    {
        var intervalSet = new IntervalSet<int>(_intervals);
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var result = intervalSet.Intersect(intersectionInterval);
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersectionsAfterMergeIntervalSet()
    {
        var intervalSet = new IntervalSet<int>(_intervals).Merge();
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var result = intervalSet.Intersect(intersectionInterval);
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersectionsIntervalTree()
    {
        var intervalTree = new IntervalTree<int, string>();
        foreach (var interval in _intervals)
        {
            intervalTree.Add(interval.Start, interval.End, $"{interval.Start}, {interval.End}");
        }

        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var result = intervalTree.Query(intersectionInterval.Start, intersectionInterval.End);
        }
    }

    Interval<int> CreateRandomInterval(int minStart, int maxEnd)
    {
        var start = _random!.Next(minStart, maxEnd);
        var length = _random.Next(0, maxEnd - minStart);

        return new Interval<int>(start, start + length);
    }
}