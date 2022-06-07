namespace Intervals.Tools;

internal class IntervalComparer<TLimit> : IComparer<Interval<TLimit>>
{
    private readonly IComparer<TLimit> _limitComparer;

    protected IntervalComparer(IComparer<TLimit> limitComparer)
    {
        _limitComparer = limitComparer;
    }

    public static IntervalComparer<TLimit> Create(IComparer<TLimit> limitComparer)
    {
        return new IntervalComparer<TLimit>(limitComparer);
    }

    public int Compare(Interval<TLimit> int1, Interval<TLimit> int2)
    {
        var comparison = _limitComparer.Compare(int1.Start, int2.Start);
        if (comparison != 0)
        {
            return comparison;
        }

        var startType1 = int1.Type & IntervalType.StartClosed;
        var startType2 = int2.Type & IntervalType.StartClosed;
        if (startType1 != startType2)
        {
            return startType2.CompareTo(startType1);
        }

        var endComparison = _limitComparer.Compare(int1.End, int2.End);
        if (endComparison != 0)
        {
            return endComparison;
        }

        var endType1 = int1.Type & IntervalType.EndClosed;
        var endType2 = int2.Type & IntervalType.EndClosed;
        return endType1.CompareTo(endType2);
    }
}