using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IntervalTree;

namespace Intervals.Tools.Playground;

[ShortRunJob]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class IntervalCollectionsInitializationBenchmarks
{
    private const int TotalIntervalsCount = 1_000_000;
    private const int MaxStartLimit = 10_000_000;
    private const int MaxIntervalLength = 1_000;

    private readonly ISet<Interval<int>> _intervals;
    private readonly SortedSet<Interval<int>> _sortedSetIntervals;
    private readonly List<Interval<int>> _seededIntersectionIntervals;
    private readonly Random _random;

    public IntervalCollectionsInitializationBenchmarks()
    {
        _random = new Random();
        _intervals = BenchmarkTools.InitRandomIntervals(TotalIntervalsCount, MaxStartLimit, MaxIntervalLength);
        _sortedSetIntervals = new SortedSet<Interval<int>>(_intervals, IntervalComparer<int>.Create(Comparer<int>.Default));
        _seededIntersectionIntervals = Enumerable.Range(0, 1_000)
            .Select(i => BenchmarkTools.CreateRandomInterval(0, MaxStartLimit))
            .ToList();
    }

    [Benchmark]
    public void Initialize_IntervalSet()
    {
        var intervalSet = new IntervalSet<int>(_intervals);
    }

    [Benchmark]
    public void Initialize_IntervalTree()
    {
        var intervalTree = new IntervalTree<int, string>();
        foreach (var itv in _intervals)
        {
            intervalTree.Add(itv.Start, itv.End, $"{itv.Start}, {itv.End}");
        }
    }
}