namespace EasyIntervals;

public static class IntervalTools
{
    /// <summary>
    /// Checks if <c>interval1</c> has any intersection with <c>interval2</c>.
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="interval1"></param>
    /// <param name="interval2"></param>
    /// <returns></returns>
    public static bool HasAnyIntersection<TLimit, TValue>(
        in Interval<TLimit, TValue> interval1, in Interval<TLimit, TValue> interval2) => HasAnyIntersection(interval1, interval2, Comparer<TLimit>.Default);

    /// <summary>
    /// Checks if <c>interval1</c> has any intersection with <c>interval2</c>.
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="interval1"></param>
    /// <param name="interval2"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static bool HasAnyIntersection<TLimit, TValue>(
        in Interval<TLimit, TValue> interval1, in Interval<TLimit, TValue> interval2, IComparer<TLimit> comparer)
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
    /// <typeparam name="TValue"></typeparam>
    /// <param name="interval"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool Covers<TLimit, TValue>(in Interval<TLimit, TValue> interval, in Interval<TLimit, TValue> other) => Covers(interval, other, Comparer<TLimit>.Default);

    /// <summary>
    /// Checks if <c>interval</c> covers <c>other</c>.
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="interval"></param>
    /// <param name="other"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static bool Covers<TLimit, TValue>(in Interval<TLimit, TValue> interval, in Interval<TLimit, TValue> other, IComparer<TLimit> comparer)
    {
        var startsComparison = comparer.Compare(interval.Start, other.Start);
        var endsComparison = comparer.Compare(interval.End, other.End);
        return (startsComparison < 0
                || (startsComparison == 0 && (interval.Type & IntervalType.StartClosed) >= (other.Type & IntervalType.StartClosed)))
            && (endsComparison > 0
                || (endsComparison == 0 && (interval.Type & IntervalType.EndClosed) >= (other.Type & IntervalType.EndClosed)));
    }

    /// <summary>
    /// Checks if end of <c>precedingInterval</c> touches start of <c>followingInterval</c>.
    /// <para>
    /// !IMPORTANT!: This method assumes that <c>precedingInterval</c> is lower than <c>followingInterval</c>.
    /// </para>
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="precedingInterval"></param>
    /// <param name="followingInterval"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    internal static bool Touch<TLimit, TValue>(in Interval<TLimit, TValue> precedingInterval, in Interval<TLimit, TValue> followingInterval, IComparer<TLimit> comparer) =>
        comparer.Compare(precedingInterval.End, followingInterval.Start) == 0
            && ((precedingInterval.Type & IntervalType.EndClosed) | (followingInterval.Type & IntervalType.StartClosed)) > 0;

    /// <summary>
    /// Merges 2 intervals.
    /// <para>
    /// !IMPORTANT!: This method assumes that <c>precedingInterval</c> is lower than <c>followingInterval</c> and they both have intersection.
    /// </para>
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="precedingInterval"></param>
    /// <param name="followingInterval"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    internal static Interval<TLimit, TValue> Merge<TLimit, TValue>(in Interval<TLimit, TValue> precedingInterval, in Interval<TLimit, TValue> followingInterval, IComparer<TLimit> comparer)
    {
        var startComparison = comparer.Compare(followingInterval.Start, precedingInterval.Start);
        var startIntervalType = startComparison == 0
            ? (followingInterval.Type | precedingInterval.Type) & IntervalType.StartClosed
            : precedingInterval.Type & IntervalType.StartClosed;
        var endComparison = comparer.Compare(followingInterval.End, precedingInterval.End);
        var endIntervalType = endComparison > 0
            ? followingInterval.Type & IntervalType.EndClosed
            : endComparison < 0
                ? precedingInterval.Type & IntervalType.EndClosed
                : (followingInterval.Type | precedingInterval.Type) & IntervalType.EndClosed;
        return (
            precedingInterval.Start,
            endComparison > 0 ? followingInterval.End : precedingInterval.End,
            startIntervalType | endIntervalType
        );
    }

    /// <summary>
    /// Merges 2 intervals.
    /// <para>
    /// !IMPORTANT!: This method assumes that <c>precedingInterval</c> is lower than <c>followingInterval</c> and they both have intersection.
    /// </para>
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="precedingInterval"></param>
    /// <param name="followingInterval"></param>
    /// <param name="mergeFunction"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    internal static Interval<TLimit, TValue> Merge<TLimit, TValue>(
        in Interval<TLimit, TValue> precedingInterval,
        in Interval<TLimit, TValue> followingInterval,
        Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue> mergeFunction,
        IComparer<TLimit> comparer)
    {
        var startComparison = comparer.Compare(followingInterval.Start, precedingInterval.Start);
        var startIntervalType = startComparison == 0
            ? (followingInterval.Type | precedingInterval.Type) & IntervalType.StartClosed
            : precedingInterval.Type & IntervalType.StartClosed;
        var endComparison = comparer.Compare(followingInterval.End, precedingInterval.End);
        var endIntervalType = endComparison > 0
            ? followingInterval.Type & IntervalType.EndClosed
            : endComparison < 0
                ? precedingInterval.Type & IntervalType.EndClosed
                : (followingInterval.Type | precedingInterval.Type) & IntervalType.EndClosed;
        return (
            precedingInterval.Start,
            endComparison > 0 ? followingInterval.End : precedingInterval.End,
            mergeFunction(precedingInterval, followingInterval),
            startIntervalType | endIntervalType
        );
    }
}
