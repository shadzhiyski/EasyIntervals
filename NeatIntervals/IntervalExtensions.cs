using NeatIntervals;

public static class IntervalExtensions
{
    /// <summary>
    /// Checks if <c>interval</c> has any intersection with <c>other</c> interval by <c>intersectionType</c>.
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="interval"></param>
    /// <param name="other"></param>
    /// <param name="intersectionType">intersectionType</param>
    /// <returns></returns>
    public static bool HasIntersection<TLimit, TValue>(
            this Interval<TLimit, TValue> interval,
            Interval<TLimit, TValue> other,
            IntersectionType intersectionType) => HasIntersection(interval, other, intersectionType, Comparer<TLimit>.Default);

    /// <summary>
    /// Checks if <c>interval</c> has any intersection with <c>other</c> interval by <c>intersectionType</c> and <c>comparer</c>.
    /// </summary>
    /// <typeparam name="TLimit"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="interval"></param>
    /// <param name="other"></param>
    /// <param name="intersectionType">intersectionType</param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static bool HasIntersection<TLimit, TValue>(
            this Interval<TLimit, TValue> interval,
            Interval<TLimit, TValue> other,
            IntersectionType intersectionType,
            IComparer<TLimit> comparer) => intersectionType switch
        {
            IntersectionType.Any => IntervalTools.HasAnyIntersection(interval, other, comparer),
            IntersectionType.Cover => IntervalTools.Covers(interval, other, comparer),
            IntersectionType.Within => IntervalTools.Covers(other, interval, comparer),
            _ => throw new InvalidOperationException("Invalid intersection type.")
        };
}