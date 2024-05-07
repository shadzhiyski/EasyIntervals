namespace EasyIntervals;

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

    public int Compare(Interval<TLimit, TValue> int1, Interval<TLimit, TValue> int2)
    {
        var comparison = _limitComparer.Compare(int1.Start, int2.Start);
        if (comparison != 0)
        {
            return comparison;
        }

        var startType1 = int1.Type & IntervalType.EndOpen;
        var startType2 = int2.Type & IntervalType.EndOpen;
        if (startType1 != startType2)
        {
            return startType2.CompareTo(startType1);
        }

        var endComparison = _limitComparer.Compare(int1.End, int2.End);
        if (endComparison != 0)
        {
            return endComparison;
        }

        var endType1 = int1.Type & IntervalType.StartOpen;
        var endType2 = int2.Type & IntervalType.StartOpen;
        return endType1.CompareTo(endType2);
    }
}