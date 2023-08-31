namespace Intervals.Tools;

public static class IntervalTools
{
    /// <summary>
    /// Checks if <c>interval1</c> has any intersection with <c>interval2</c>.
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <param name="interval1"></param>
    /// <param name="interval2"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static bool HasAnyIntersection<TLimit>(
        Interval<TLimit> interval1, Interval<TLimit> interval2, IComparer<TLimit> comparer)
    {
        var startEndComparison = comparer.Compare(interval1.Start, interval2.End);
        var endStartComparison = comparer.Compare(interval1.End, interval2.Start);
        if (startEndComparison > 0 || endStartComparison < 0)
        {
            return false;
        }

        var overlapsStartEnd = startEndComparison < 0
            || ((interval1.Type & IntervalType.StartClosed) > 0
                && (interval2.Type & IntervalType.EndClosed) > 0
                && startEndComparison == 0);
        var overlapsEndStart = endStartComparison > 0
            || ((interval1.Type & IntervalType.EndClosed) > 0
                && (interval2.Type & IntervalType.StartClosed) > 0
                && endStartComparison == 0);
        return overlapsStartEnd && overlapsEndStart;
    }

    /// <summary>
    /// Checks if <c>interval</c> covers <c>other</c>.
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <param name="interval"></param>
    /// <param name="other"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static bool Covers<TLimit>(Interval<TLimit> interval, Interval<TLimit> other, IComparer<TLimit> comparer)
    {
        var startsComparison = comparer.Compare(interval.Start, other.Start);
        var endsComparison = comparer.Compare(interval.End, other.End);
        return (startsComparison < 0
                || (startsComparison == 0 && (interval.Type & IntervalType.StartClosed) <= (other.Type & IntervalType.StartClosed)))
            && (endsComparison > 0
                || (endsComparison == 0 && (interval.Type & IntervalType.EndClosed) >= (other.Type & IntervalType.EndClosed)));
    }

    /// <summary>
    /// Checks if end of <c>before</c> interval touches start of <c>after</c> interval.
    /// <para>
    /// !IMPORTANT!: This method assumes that <c>before</c> interval is lower than <c>after</c> interval.
    /// </para>
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    internal static bool Touch<TLimit>(Interval<TLimit> before, Interval<TLimit> after, IComparer<TLimit> comparer) =>
        comparer.Compare(before.End, after.Start) == 0
            && ((before.Type & IntervalType.EndClosed) | (after.Type & IntervalType.StartClosed)) > 0;

    /// <summary>
    /// Merges 2 intervals.
    /// <para>
    /// !IMPORTANT!: This method assumes that <c>before</c> interval is lower than <c>after</c> interval and they both have intersection.
    /// </para>
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    internal static Interval<TLimit> Merge<TLimit>(Interval<TLimit> before, Interval<TLimit> after, IComparer<TLimit> comparer)
    {
        var startComparison = comparer.Compare(after.Start, before.Start);
        var startIntervalType = startComparison == 0
            ? (after.Type | before.Type) & IntervalType.StartClosed
            : before.Type & IntervalType.StartClosed;
        var endComparison = comparer.Compare(after.End, before.End);
        var endIntervalType = endComparison > 0
            ? after.Type & IntervalType.EndClosed
            : endComparison < 0
                ? before.Type & IntervalType.EndClosed
                : (after.Type | before.Type) & IntervalType.EndClosed;
        return (
            before.Start,
            endComparison > 0 ? after.End : before.End,
            startIntervalType | endIntervalType
        );
    }
}
