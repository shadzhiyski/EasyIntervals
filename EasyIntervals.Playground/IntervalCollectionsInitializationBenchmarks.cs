using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace EasyIntervals.Playground;

[ShortRunJob]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class IntervalCollectionsInitializationBenchmarks
{
    private const int TotalIntervalsCount = 1_000_000;
    private const int MaxStartLimit = 10_000_000;
    private const int MaxIntervalLength = 1_000;

    private readonly IList<Interval<int, int?>> _intervals;

    public IntervalCollectionsInitializationBenchmarks()
    {
        _intervals = BenchmarkTools.CreateRandomIntervals(TotalIntervalsCount, MaxStartLimit, MaxIntervalLength).ToList();
    }

    [Benchmark]
    public void Initialize_IntervalSet()
    {
        var _ = new IntervalSet<int, int?>(_intervals);
    }

    [Benchmark]
    public void Initialize_SortedSet()
    {
        var _ = new SortedSet<Interval<int, int?>>(_intervals, IntervalComparer<int, int?>.Create(Comparer<int>.Default));
    }

    [Benchmark]
    public void UnionWith_IntervalSet()
    {
        var intervalSet = new IntervalSet<int, int?>(_intervals.Take(_intervals.Count / 3));
        intervalSet.UnionWith(_intervals.Skip(_intervals.Count / 3));
    }

    [Benchmark]
    public void UnionWith_SortedSet()
    {
        var intervalSet = new SortedSet<Interval<int, int?>>(_intervals.Take(_intervals.Count / 3), IntervalComparer<int, int?>.Create(Comparer<int>.Default));
        intervalSet.UnionWith(_intervals.Skip(_intervals.Count / 3));
    }
}