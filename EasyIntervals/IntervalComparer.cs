namespace EasyIntervals;

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

    public int Compare(Interval<TLimit> interval1, Interval<TLimit> interval2)
    {
        var comparison = _limitComparer.Compare(interval1.Start, interval2.Start);
        if (comparison != 0)
        {
            return comparison;
        }

        var startType1 = interval1.Type & IntervalType.StartClosed;
        var startType2 = interval2.Type & IntervalType.StartClosed;
        if (startType1 != startType2)
        {
            return startType2.CompareTo(startType1);
        }

        var endComparison = _limitComparer.Compare(interval1.End, interval2.End);
        if (endComparison != 0)
        {
            return endComparison;
        }

        var endType1 = interval1.Type & IntervalType.EndClosed;
        var endType2 = interval2.Type & IntervalType.EndClosed;
        return endType1.CompareTo(endType2);
    }
}

internal class IntervalComparer<TLimit, TValue> : IComparer<Interval<TLimit, TValue>>
{
    private readonly IComparer<TLimit> _limitComparer;

    protected IntervalComparer(IComparer<TLimit> limitComparer)
    {
        _limitComparer = limitComparer;
    }

    public static IntervalComparer<TLimit, TValue> Create(IComparer<TLimit> limitComparer)
    {
        return new IntervalComparer<TLimit, TValue>(limitComparer);
    }

    public int Compare(Interval<TLimit, TValue> interval1, Interval<TLimit, TValue> interval2)
    {
        var comparison = _limitComparer.Compare(interval1.Start, interval2.Start);
        if (comparison != 0)
        {
            return comparison;
        }

        var startType1 = interval1.Type & IntervalType.StartClosed;
        var startType2 = interval2.Type & IntervalType.StartClosed;
        if (startType1 != startType2)
        {
            return startType2.CompareTo(startType1);
        }

        var endComparison = _limitComparer.Compare(interval1.End, interval2.End);
        if (endComparison != 0)
        {
            return endComparison;
        }

        var endType1 = interval1.Type & IntervalType.EndClosed;
        var endType2 = interval2.Type & IntervalType.EndClosed;
        return endType1.CompareTo(endType2);
    }
}