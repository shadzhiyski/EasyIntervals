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
    private const int IntersectionIntervalsCount = 1_000;
    private const int MaxStartLimit = 10_000_000;
    private const int MaxIntervalLength = 1_000;

    private const int MaxIntersectionIntervalLength = 100_000;

    private readonly ISet<Interval<int>> _intervals;
    private readonly List<Interval<int>> _seededIntersectionIntervals;

    public IntervalCollectionsBenchmarks()
    {
        _intervals = BenchmarkTools.CreateRandomIntervals(TotalIntervalsCount, MaxStartLimit, MaxIntervalLength);
        _seededIntersectionIntervals = Enumerable.Range(0, IntersectionIntervalsCount)
            .Select(i => BenchmarkTools.CreateRandomInterval(MaxStartLimit, MaxIntersectionIntervalLength))
            .ToList();
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_IntervalSet()
    {
        var intervalSet = new IntervalSet<int>(_intervals);
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var _ = intervalSet.Intersect(intersectionInterval);
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_IntervalSet_AfterIntervalsMerge()
    {
        var intervalSet = new IntervalSet<int>(_intervals).Merge();
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var _ = intervalSet.Intersect(intersectionInterval);
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
            var _ = intervalTree.Query(intersectionInterval.Start, intersectionInterval.End);
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
            var intersectionIntervals = _seededIntersectionIntervals.Take(100);
            foreach (var intersectionInterval in intersectionIntervals)
            {
                var _ = intervalSet.Intersect(intersectionInterval);
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
            var intersectionIntervals = _seededIntersectionIntervals.Take(100);
            foreach (var intersectionInterval in intersectionIntervals)
            {
                var _ = intervalTree.Query(intersectionInterval.Start, intersectionInterval.End);
            }
        }
    }
}