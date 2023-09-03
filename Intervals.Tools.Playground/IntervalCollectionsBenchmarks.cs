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
    private const int TotalIntervalsCount = 250_000;
    private const int IntersectionIntervalsCount = 100;
    private const int MaxStartLimit = 10_000_000;
    private const int MaxIntervalLength = 1_000;

    private readonly ISet<Interval<int>> _intervals;
    private readonly List<Interval<int>> _seededIntersectionIntervals;

    public IntervalCollectionsBenchmarks()
    {
        _intervals = BenchmarkTools.CreateRandomIntervals(TotalIntervalsCount, MaxStartLimit, MaxIntervalLength);
        _seededIntersectionIntervals = Enumerable.Range(0, IntersectionIntervalsCount)
            .Select(i => BenchmarkTools.CreateRandomInterval(0, MaxStartLimit))
            .ToList();
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_IntervalSet()
    {
        var intervalSet = new IntervalSet<int>(_intervals);
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var result = intervalSet.Intersect(intersectionInterval);
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_IntervalSet_AfterIntervalsMerge()
    {
        var intervalSet = new IntervalSet<int>(_intervals).Merge();
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var result = intervalSet.Intersect(intersectionInterval);
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_IntervalTree()
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

    [Benchmark]
    public void Test100XMoreIntersectionsThanInserts_IntervalSet()
    {
        var seededIntervals = _intervals.Take(1_000);
        var intervalSet = new IntervalSet<int>();
        foreach (var interval in seededIntervals)
        {
            intervalSet.Add(interval);
            for (int j = 0; j < 100; j++)
            {
                var intersectionInterval = BenchmarkTools.CreateRandomInterval(0, MaxStartLimit);
                var result = intervalSet.Intersect(intersectionInterval);
            }
        }
    }

    [Benchmark]
    public void Test100XMoreIntersectionsThanInserts_IntervalTree()
    {
        var seededIntervals = _intervals.Take(1_000);
        var intervalTree = new IntervalTree<int, string>();
        foreach (var interval in seededIntervals)
        {
            intervalTree.Add(interval.Start, interval.End, $"{interval.Start}, {interval.End}");
            for (int j = 0; j < 100; j++)
            {
                var intersectionInterval = BenchmarkTools.CreateRandomInterval(0, MaxStartLimit);
                var result = intervalTree.Query(intersectionInterval.Start, intersectionInterval.End);
            }
        }
    }
}