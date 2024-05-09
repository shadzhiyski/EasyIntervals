using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace NeatIntervals.Playground;

[ShortRunJob]
// [InProcess]
// [NativeMemoryProfiler]
[MemoryDiagnoser]
// [MinColumn, MaxColumn, Q1Column, Q3Column, AllStatisticsColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class IntervalCollectionsBenchmarks
{
    private const int TotalIntervalsCount = 1_000_000;
    private const int IntersectionIntervalsCount = 1_000;
    private const int MaxStartLimit = 10_000_000;
    private const int MaxIntervalLength = 1_000;

    private const int MaxIntersectionIntervalLength = 100_000;

    private readonly ISet<Interval<int, int?>> _intervals;
    private readonly List<Interval<int, int?>> _seededIntersectionIntervals;

    private readonly IntervalSet<int, int?> _intervalSet;
    private readonly SortedSet<Interval<int, int?>> _sortedSet;

    public IntervalCollectionsBenchmarks()
    {
        _intervals = BenchmarkTools.CreateRandomIntervals(TotalIntervalsCount, MaxStartLimit, MaxIntervalLength);
        _seededIntersectionIntervals = Enumerable.Range(0, IntersectionIntervalsCount)
            .Select(i => BenchmarkTools.CreateRandomInterval(MaxStartLimit, MaxIntersectionIntervalLength))
            .ToList();

        _intervalSet = new IntervalSet<int, int?>(_intervals);
        _sortedSet = new SortedSet<Interval<int, int?>>(_intervals, IntervalComparer<int, int?>.Create(Comparer<int>.Default));
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_IntervalSet()
    {
        var intervalSet = _intervalSet;
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var _ = intervalSet.Intersect(intersectionInterval);
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_IntervalSet_AfterIntervalsMerge()
    {
        var intervalSet =_intervalSet.Merge();
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var _ = intervalSet.Intersect(intersectionInterval);
        }
    }

    [Benchmark]
    public void TestManyConsecutiveIntersections_SortedSet()
    {
        var intervalSet = _sortedSet;
        foreach (var intersectionInterval in _seededIntersectionIntervals)
        {
            var _ = new SortedSet<Interval<int, int?>>(intervalSet.GetViewBetween(
                    (intersectionInterval.Start, intersectionInterval.Start, IntervalType.Closed),
                    (intersectionInterval.End, intersectionInterval.End, IntervalType.Closed)),
                    intervalSet.Comparer);
        }
    }

    [Benchmark]
    public void Test100XMoreIntersectionsThanInserts_IntervalSet()
    {
        var seededIntervals = _intervals.Take(10_000);
        var intervalSet = new IntervalSet<int, int?>();
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
    public void Test100XMoreIntersectionsThanInserts_SortedSet()
    {
        var intervalSet = new SortedSet<Interval<int, int?>>(IntervalComparer<int, int?>.Create(Comparer<int>.Default));
        var seededIntervals = _intervals.Take(10_000);
        foreach (var itv in seededIntervals)
        {
            intervalSet.Add((itv.Start, itv.End, itv.Type));
            var intersectionIntervals = _seededIntersectionIntervals.Take(100);
            foreach (var intersectionInterval in intersectionIntervals)
            {
                var _ = new SortedSet<Interval<int, int?>>(intervalSet.GetViewBetween(
                        (intersectionInterval.Start, intersectionInterval.Start, IntervalType.Closed),
                        (intersectionInterval.End, intersectionInterval.End, IntervalType.Closed)),
                        intervalSet.Comparer);
            }
        }
    }
}